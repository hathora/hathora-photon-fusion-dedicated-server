// Created by dylan@hathora.dev

using System;
using System.Net;
using System.Threading.Tasks;
using Fusion;
using Fusion.Sample.DedicatedServer;
using Fusion.Sample.DedicatedServer.Utils;
using Hathora.Core.Scripts.Runtime.Server;
using Hathora.Core.Scripts.Runtime.Server.Models;
using UnityEngine;
using UnityEngine.SceneManagement;
using Assert = UnityEngine.Assertions.Assert;

namespace HathoraPhoton
{
    /// <summary>
    /// Modified from the demo-included `ServerManagerDefault.cs` - Changes:
    /// - Added `|| UNITY_EDITOR` to `#if UNITY_SERVER` to allow optional Editor debugging as server.
    /// - Added server + client wrappers for clarity, since #if wrappers get confusing.
    /// - Added Editor debugging + Hathora logic to ServerManagerDefault.
    /// - (!) This does *not* create a Hathora Lobby; just a Room.
    /// </summary>
    public class HathoraPhotonServerMgr : ServerManagerBase
    {
        #region vars
        private enum EditorStartType
        {
            Client,
            Server,
        }

        [SerializeField]
        private EditorStartType editorStartType = EditorStartType.Client;

        private HathoraServerMgr hathoraServerMgr => HathoraServerMgr.Singleton;
        
        /// <summary>
        /// We can run server events if:
        /// 1. We are UNITY_SERVER
        /// 2. We are Editor && EditorStartType is Server (mocking Server)
        /// </summary>
        bool canRunServerEvents
        {
            get {
#if UNITY_EDITOR
                if (editorStartType != EditorStartType.Server)
                    return false;
#elif !UNITY_SERVER
            return false;
#endif

                return true; // canRunServerEvents
            }
        }
        #endregion // vars
        

        #region Init
        private void Awake()
        {
            Debug.Log($"[{nameof(HathoraPhotonServerMgr)}] Awake: " +
                $"canRunServerEvents? {canRunServerEvents}");
            
            if (!canRunServerEvents)
                return;
            
            // Subscribe to Hathora server initd event
            HathoraServerMgr.OnInitializedEvent += onHathoraServerInitd;
        }

        private void Start()
        {
            if (!canRunServerEvents)
                startClient(); // onHathoraServerInitd() will never be called
        }

        /// <summary>
        /// Triggers when HathoraServerMgr finishes getting HathoraServerContext async.
        /// (!) Does not trigger if Init fails.
        /// - Subscribed @ Awake
        /// - Unsubscribed @ OnDestroy
        /// - VaLidates -> then curries to initDedicatedServer Task
        /// </summary>
        /// <param name="hathoraServerContext"></param>
        private void onHathoraServerInitd(HathoraServerContext hathoraServerContext)
        {
            string logPrefix = $"[{nameof(HathoraPhotonServerMgr)}.{nameof(onHathoraServerInitd)}]";
            
            // Validate
            assertHathoraDeployServerReqs();
            
            string deployErrMsg = $"{logPrefix} Expected deployInfo: If debugging locally, are you sure " +
                "you pasted an *active* ProcessId to `HathoraPhotonManager.HathoraServerMgr.DebugEditorMockProcId?` " +
                "Inactive Processes despawn in 5m - perhaps timed out?";
            Assert.IsNotNull(hathoraServerContext?.FirstActiveRoomForProcess, deployErrMsg);
            
            // Hathora server is ready - now init Photon dedicated server logic
            _ = startPhotonDedicatedServer(hathoraServerContext);   
        }
        
        /// <summary>Start Photon Dedicated Server</summary>
        private async Task startPhotonDedicatedServer(HathoraServerContext hathoraServerContext)
        {
            // Continue with start the Dedicated Server
            string logPrefix = $"[HathoraPhotonServerMgr.{nameof(startPhotonDedicatedServer)}]";
            Debug.Log($"{logPrefix} Starting dedicated server");
            Application.targetFrameRate = 30;

            DedicatedServerConfig config = DedicatedServerConfig.Resolve();
            
            #region Hathora Edits
            // Set container (!public) port
            ushort containerPort = (ushort)hathoraServerMgr.HathoraServerConfig.HathoraDeployOpts.ContainerPortSerializable.Port;
            config.Port = containerPort; // Default == 7777
            
            // Set public Room ip:port
            (IPAddress Ip, ushort Port) roomIpPort = await hathoraServerContext.GetHathoraServerIpPortAsync();
            
            config.PublicIP = roomIpPort.Ip.ToString();
            config.PublicPort = roomIpPort.Port;

            Debug.Log($"{logPrefix} Used hathoraServerContext to set Photon `config`:\n" +
                $"1. Internal bind container set to port: {config.Port}\n" +
                $"2. Photon public config.PublicIp and .PublicPort set to ip:port: `{config.PublicIP}:{config.PublicPort}\n`");
            #endregion // Hathora Edits
            
            // Get HATHORA_REGION and convert to closest Photon region - and set in start config
            string matchingPhotonRegion = HathoraRegionUtility.HathoraToPhoton(hathoraServerContext.EnvVarRegion);
            config.Region = matchingPhotonRegion;
            
            Debug.Log(config);

            // Start a new Runner instance
            NetworkRunner runner = Instantiate(_runnerPrefab);

            // Start the Server
            StartGameResult result = null;
            try
            {
                result = await StartSimulation(runner, config);
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} StartSimulation => Error: {e.Message} " +
                    $"// startGameResult=={result}");
                throw;
            }

            // Check if all went fine
            if (result.Ok)
                Log.Debug($"{logPrefix} Runner Start DONE");
            else
            {
                // Quit the application if startup fails
                Log.Debug($"{logPrefix} Error while starting Server: {result.ShutdownReason}");

                // it can be used any error code that can be read by an external application
                // using 0 means all went fine
                Application.Quit(1);
            }
        }
        
        /// <summary>Load scene 1 (Menu) as Client</summary>
        private void startClient()
        {
            Debug.Log($"[HathoraPhotonServerMgr] {nameof(startClient)} (`1.Menu` scene)");
            SceneManager.LoadScene((int)SceneDefs.MENU, LoadSceneMode.Single);
        }
        #endregion // Init

        
        #region Utils
        /// <summary>Throws if !valid with verbose instructions on how to fix the issue</summary>
        private void assertHathoraDeployServerReqs()
        {
            string logPrefix = $"[HathoraPhotonServerMgr.{nameof(assertHathoraDeployServerReqs)}]";

            bool isHathoraServerSerializedInScene = hathoraServerMgr; 
            Assert.IsTrue(isHathoraServerSerializedInScene, $"{logPrefix} !isHathoraServerSerializedInScene: " +
                "Did you add `HathoraPhotonManager` prefab variant GameObject to the `0.Launch_Default` scene?");

            bool hasHathoraServerConfig = hathoraServerMgr.HathoraServerConfig; 
            Assert.IsTrue(hasHathoraServerConfig, $"{logPrefix} isHathoraServerSerializedInScene, but " +
                "expected hasHathoraServerConfig: Did you serialize your selected " +
                "config? Create/find a config via top menu: `Hathora/ConfigFinder` or via `Assets/Hathora/`");

            bool isServerDeployedOnHathora = hathoraServerMgr.IsDeployedOnHathora;
            Assert.IsTrue(isServerDeployedOnHathora, $"{logPrefix} {nameof(hathoraServerMgr.IsDeployedOnHathora)}, " +
                "but Expected isServerDeployedOnHathora: If you're trying to mock a deployed Server as localhost:\n" +
                "(1) Create a Room via HathoraServerConfig (or via web console) and await ready =>\n" +
                "(2) Copy the `ProcessId` from the Hathora web console next to the new Room =>\n" +
                "(3) Paste the `ProcessId` to `HathoraPhotonManager.HathoraServerMgr.DebugEditorMockProcId` " +
                "(you only have 5m before the server Room Process idles out) =>\n" +
                "This will go through the flow of getting Process=>Room=>Lobby");
        }
        #endregion // Utils


        private void OnDestroy()
        {
            // Unsub to events
            HathoraServerMgr.OnInitializedEvent -= onHathoraServerInitd;
        }
    }
}
