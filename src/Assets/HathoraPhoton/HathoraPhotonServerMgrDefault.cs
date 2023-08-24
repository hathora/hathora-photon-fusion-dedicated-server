// Created by dylan@hathora.dev

using System;
using System.Threading.Tasks;
using Fusion.Sample.DedicatedServer;
using Hathora.Core.Scripts.Runtime.Server;
using Hathora.Core.Scripts.Runtime.Server.Models;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace HathoraPhoton
{
    /// <summary>
    /// Adds Editor debugging + Hathora logic to ServerManagerDefault.
    /// </summary>
    public class HathoraPhotonServerMgrDefault : ServerManagerDefault
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
        #endregion // vars
        
        
        /// <summary>Add Editor debug options for Server</summary>
        protected override async Task OnStart()
        {
            Debug.Log("[HathoraPhotonServerMgrDefault] @ OnStart");
            
#if !UNITY_EDITOR
            // !Editor -- Continue as normal >>
            await base.OnStart();
#endif
            
            // Allow us to debug Server (instead of just Client) within the Editor
            bool isEditorMockedAsServer = Application.isEditor && editorStartType == EditorStartType.Server;
            if (isEditorMockedAsServer)
                await LoadGameAsDedicatedServer();
            else
                LoadMenuAsClient();
        }

        /// <summary>
        /// If we're deployed in Hathora, we have access to the ProcessId, where we may get:
        /// 1. Process (host, ip, port)
        /// 1. Room (RoomId)
        /// 2. Lobby (InitialConfig for arbitrary data to show in a Lobby list of props)
        /// </summary>
        /// <returns></returns>
        protected override async Task LoadGameAsDedicatedServer()
        {
            string logPrefix = $"[HathoraPhotonServerMgrDefault.{nameof(LoadGameAsDedicatedServer)}]";

            // Validate reqs
            assertHathoraDeployServerReqs();
            
            // ##################################################################################################
            // Get Hathora init deploy info (Process => Room => Lobby).
            // If we made the Room via Hathora web console || HathoraServerConfig, we have !Lobby.
            // No Lobby (for +arbitrary props like room display name to show to other players in a room browser)
            // is ok if we only want to just minimally test, but we should probably add a lobby later.
            // ##################################################################################################

            // deployInffo.Lobby will be null (expected)
            HathoraServerContext serverContext = await hathoraServerMgr.GetCachedServerContextAsync();

            string deployErrMsg = $"{logPrefix} Expected deployInfo: If debugging locally, are you sure " +
                "you pasted an *active* ProcessId to `HathoraPhotonManager.HathoraServerMgr.DebugEditorMockProcId?` " +
                "Inactive Processes despawn in 5m - perhaps timed out?";
            Assert.IsNotNull(serverContext, deployErrMsg);
            Assert.IsTrue(serverContext.CheckIsValid(), deployErrMsg);

            throw new NotImplementedException("TODO: Set the public IP:port to the Photon config - where is this?");
            
            // Continue as normal @ base
            await base.LoadGameAsDedicatedServer();
        }

        /// <summary>Throws if !valid with verbose instructions on how to fix the issue</summary>
        private void assertHathoraDeployServerReqs()
        {
            string logPrefix = $"[HathoraPhotonServerMgrDefault.{nameof(assertHathoraDeployServerReqs)}]";

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
    }
}
