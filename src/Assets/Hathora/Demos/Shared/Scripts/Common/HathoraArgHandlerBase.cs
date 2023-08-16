// Created by dylan@hathora.dev

using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hathora.Demos.Shared.Scripts.Common
{
    /// <summary>
    /// Commandline helper - run via `-_mode {server|client|host} -memo {someStr}`.
    /// (!) `-scene` is loaded / awaited before any other cmd.
    /// Unity: Command Line Helper | https://docs-multiplayer.unity3d.com/netcode/current/tutorials/command-line-helper/index.html  
    /// </summary>
    public abstract class HathoraArgHandlerBase : MonoBehaviour
    {
        #region vars
        private static bool _sceneArgConsumed = false;
        
        /// <summary>We only trigger this once</summary>
        public static bool SceneArgConsumed
        {
            get => _sceneArgConsumed;
            set 
            {
                if (value)
                    Debug.Log($"[HathoraArgHandlerBase] SceneArgConsumed @ " +
                        SceneManager.GetActiveScene().name);
                
                _sceneArgConsumed = value;
            }
        }

        
        private static bool _modeArgConsumed;
        
        /// <summary>We only trigger this once</summary>
        public static bool ModeArgConsumed
        {
            get => _modeArgConsumed;
            set 
            {
                if (value)
                    Debug.Log($"[HathoraArgHandlerBase] ModeArgConsumed @ {SceneManager.GetActiveScene().name}");
                
                _modeArgConsumed = value;
            }
        }
        #endregion // vars
        
        
        private async void Start() => await InitArgs();

        /// <summary>
        /// (!) Some args like `-scene` and `-mode` are statically consumed only once
        /// (eg: reloading the scene won't apply them).</summary>
        /// <param name="_cmdLineArgsOverrideList">Perhaps for mock testing</param>
        protected virtual async Task InitArgs(Dictionary<string, string> _cmdLineArgsOverrideList = null)
        {
            Debug.Log($"[HathoraArgHandlerBase] Init @ scene: {gameObject.scene.name} " +
                "(before consuming `-scene` arg, if exists)");
            
            if (Application.isEditor && _cmdLineArgsOverrideList == null)
                return;

            Dictionary<string, string> args = _cmdLineArgsOverrideList ?? GetCommandlineArgs();

            // -scene {string} // Load scene by name
            if (args.TryGetValue("-scene", out string sceneName) && !string.IsNullOrEmpty(sceneName))
                await InitArgScene(sceneName);
            
            // -_mode {server|client|host} // Logs and start netcode
            if (args.TryGetValue("-mode", out string mode))
                InitArgMode(mode);
            
            // -memo {string} // Show arbitrary text at bottom of screen
            if (args.TryGetValue("-memo", out string memoStr) && !string.IsNullOrEmpty(memoStr))
                InitArgMemo(memoStr);
        }

        /// <summary>
        /// For the demo:
        /// - "HathoraDemoScene-Menu"
        /// - "HathoraDemoScene-FishNet"
        /// - "HathoraDemoScene-Mirror"
        /// - "HathoraDemoScene-UnityNGO"
        /// </summary>
        /// <param name="_sceneName"></param>
        protected virtual async Task InitArgScene(string _sceneName)
        {
            if (SceneArgConsumed)
            {
                Debug.LogWarning("[HathoraArgHandlerBase.InitMode] SceneArgConsumed, already");
                return;
            }
            
            await HathoraNetPlatformSelector.LoadSceneOnceFromArgAsync(_sceneName);
        }

        /// <summary>Override me -> Set memoStr in UI</summary>
        /// <param name="_memoStr"></param>
        protected virtual void InitArgMemo(string _memoStr) =>
            Debug.Log($"[HathoraArgHandler.InitMemo] {_memoStr}");
        
        /// <summary>
        /// - "server" -> StartServer()
        /// - "client" -> StartClient()
        /// - "host" -> StartHost() // server+client together
        /// </summary>
        protected virtual void InitArgMode(string _mode)
        {
            if (ModeArgConsumed)
            {
                Debug.LogWarning("[HathoraArgHandlerBase.InitMode] ModeArgConsumed, already");
                return;
            }
            
            Debug.Log($"[HathoraArgHandlerBase.InitMode] {_mode}");
            ModeArgConsumed = true;

            switch (_mode)
            {
                case "server":
                    ArgModeStartServer();
                    break;
                
                case "client":
                    ArgModeStartClient();
                    break;
                
                case "host":
                    ArgModeStartHost();
                    break;
            }
        }

        protected virtual void ArgModeStartServer() =>
            Debug.Log("[HathoraArgHandlerBase] StartServer");

        protected virtual void ArgModeStartClient() =>
            Debug.Log("[HathoraArgHandlerBase] StartClient");

        /// <summary>
        /// Both server *and* client.
        /// Most NetCode just allows StartServer() -> StartClient(); override if not.
        /// </summary>
        protected virtual void ArgModeStartHost()
        {
            Debug.Log("[HathoraArgHandlerBase.StartHost] (server+client) Starting...");
            ArgModeStartServer();
            ArgModeStartClient();
        }
        
        
        #region Utils
        protected static Dictionary<string, string> GetCommandlineArgs()
        {
            Dictionary<string, string> argDictionary = new();

            string[] args = System.Environment.GetCommandLineArgs();

            for (int i = 0; i < args.Length; ++i)
            {
                string arg = args[i].ToLower();

                if (!arg.StartsWith("-"))
                    continue;
                
                string value = i < args.Length - 1 
                    ? args[i + 1].ToLower() 
                    : null;
                
                value = value?.StartsWith("-") ?? false 
                    ? null 
                    : value;

                argDictionary.Add(arg, value);
            }
            return argDictionary;
        }
        #endregion // Utils
        
        
        void OnDisable()
        {
            // Static vars may persist between Editor play sessions. 
            _sceneArgConsumed = false;
            _modeArgConsumed = false;
        }
    }
}
