// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hathora.Demos.Shared.Scripts.Common
{
    /// <summary>
    /// Commandline helper - run via `-_mode {server|client|host}`.
    /// (!) `-scene` is loaded / awaited before any other cmd.
    /// Unity: Command Line Helper | https://docs-multiplayer.unity3d.com/netcode/current/tutorials/command-line-helper/index.html  
    /// </summary>
    public abstract class HathoraArgHandlerBase : MonoBehaviour
    {
        #region Mock Testing
        [SerializeField, Tooltip("For mock -arg testing within the Editor")]
        private bool mockArgsInEditor;
        protected bool MockArgsInEditor => mockArgsInEditor;
        
        /// <summary>Keys already include the `-` prefix.</summary>
        private static readonly Dictionary<string, string> MOCK_ARGS_DICT = new()
        {
            {"-scene", "HathoraDemoScene-Mirror"}, // "HathoraDemoScene-FishNet" || "HathoraDemoScene-Mirror"
            {"-mode", "server"}, // "server" || "client" || "host
        };

        readonly string MOCK_ARGS_DICT_STR = string.Join(" ", MOCK_ARGS_DICT.Select(kvp => 
            $"{kvp.Key} {kvp.Value}"));
        #endregion // Mock Testing
        
        
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
        
        
        private async void Start() => await InitArgsAsync();

        /// <summary>
        /// (!) Some args like `-scene` and `-mode` are statically consumed only once
        /// (eg: reloading the scene won't apply them).</summary>
        protected virtual async Task InitArgsAsync()
        {
            string logPrefix = $"[HathoraArgHandlerBase.{nameof(InitArgsAsync)}]";
            
            Dictionary<string, string> args = GetCommandlineArgs();

            if (Application.isEditor && mockArgsInEditor)
            {
                Debug.LogWarning($"{logPrefix} (!) Init: <color=yellow>" +
                    $"MOCK_ARG_IN_EDITOR</color>: `{MOCK_ARGS_DICT_STR}`");
               
                args = MOCK_ARGS_DICT; // Override for debugging
            }
            
            string argsStr = string.Join(" ", args.Select(kvp => $"{kvp.Key} {kvp.Value}"));
            Debug.Log($"{logPrefix} Handling args: `{argsStr}`");

            try
            {
                // -scene {string} // Load scene by name
                bool hasSceneArg = args.TryGetValue("-scene", out string sceneName) && !string.IsNullOrEmpty(sceneName); 
                    
                if (!Application.isEditor)
                    Debug.Log($"{logPrefix} Has `-scene` arg? {hasSceneArg}");
                    
                if (hasSceneArg)
                    await InitArgScene(sceneName);
            
                // -----------------
                // -_mode {server|client|host} // Logs and start netcode
                bool hasModeArg = args.TryGetValue("-mode", out string mode) && !string.IsNullOrEmpty(mode);
                
                if (!Application.isEditor)
                    Debug.Log($"{logPrefix} Has `-mode` arg? {hasModeArg}");
                
                if (hasModeArg)
                    InitArgMode(mode);
            }
            catch (Exception e)
            {
                Debug.LogError($"[HathoraArgHandlerBase.InitArgsAsync] Error: {e.Message}");
                throw;
            }
        }

        protected virtual async Task InitArgScene(string _sceneName)
        {
            Debug.Log($"[HathoraArgHandlerBase] InitArgScene: {_sceneName}");

            // Get current scene name
            string currentSceneName = SceneManager.GetActiveScene().name;
            if (currentSceneName == _sceneName)
            {
                _sceneArgConsumed = true;
                return; // We're already in this scene
            }

            if (SceneArgConsumed)
            {
                Debug.LogWarning("[HathoraArgHandlerBase.InitMode] SceneArgConsumed, already");
                return;
            }

            try
            {
                await loadSceneOnceFromArgAsync(_sceneName);
            }
            catch (Exception e)
            {
                Debug.LogError($"[HathoraArgHandlerBase.InitArgScene] Error: {e}");
                throw;
            }
        }
        
        /// <summary>
        /// After using this once, you won't be able to do it again
        /// (to prevent multiple arg handling stack overflows).
        /// 
        /// Mostly used for args or initial scene selection.
        /// </summary>
        /// <param name="_sceneName">CLI Arg passed from `-scene {sceneName}`.</param>
        private static async Task loadSceneOnceFromArgAsync(string _sceneName)
        {
            string logPrefix = $"[HathoraArgHandlerBase.{nameof(loadSceneOnceFromArgAsync)}]";
            Debug.Log($"{logPrefix} sceneName: {_sceneName}");

            if (SceneArgConsumed)
            {
                Debug.LogWarning($"{logPrefix} LoadSceneOnceAsync already consumed! Aborting.");
                return;
            }

            SceneArgConsumed = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(
                _sceneName, 
                LoadSceneMode.Single);

            while (!asyncLoad.isDone)
                await Task.Yield();
        }
        
        /// <summary>
        /// - "server" -> StartServer()
        /// - "client" -> StartClient()
        /// - "host" -> StartHost() // server+client together
        /// </summary>
        protected virtual void InitArgMode(string _mode)
        {
            Debug.Log($"[HathoraArgHandlerBase] InitArgMode: {_mode}");

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
