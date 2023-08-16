// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Client;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Common.Utils;
using Hathora.Core.Scripts.Runtime.Server.ApiWrapper;
using Hathora.Core.Scripts.Runtime.Server.Models;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Server
{
    /// <summary>
    /// Inits and centralizes all Hathora Server [runtime] API wrappers.
    /// (!) If you are inheritting this, child should likely have setSingleton().
    ///     (CTRL+F here for template)
    /// 
    /// Unlike HathoraClientMgrBase, we don't need a parent since Server is lower-level
    /// than Client (eg: No UI, Session or net code specific to a platform).
    /// TODO: If this gets more complex, make it an abstract base class; parity with Client.
    /// </summary>
    public abstract class HathoraServerMgrBase : MonoBehaviour
    {
        // ######################################################################
        // TODO: Add a `HathoraCommonMgr` for shared code; mv StartHost() here
        // ######################################################################
        
        #region Vars
        /// <summary>Set null/empty to !fake a procId in the Editor</summary>
        [SerializeField, Tooltip("When in the Editor, we'll get this Hathora ProcessInfo " +
             "as if deployed on Hathora; useful for debugging")]
        private string debugEditorMockProcId;
        protected string DebugEditorMockProcId => debugEditorMockProcId;
        
        [Header("(!) Top menu: Hathora/ServerConfigFinder")]
        [SerializeField]
        private HathoraServerConfig hathoraServerConfig;
        public HathoraServerConfig HathoraServerConfig
        {
            get {
				#if !UNITY_SERVER && !UNITY_EDITOR
				Debug.LogError("[HathoraServerMgr] (!) Tried to get hathoraServerConfig " +
                    "from Server when NOT a <server || editor>");
				return null;
				#endif // !UNITY_SERVER && !UNITY_EDITOR

                if (hathoraServerConfig == null)
                {
                    Debug.LogError("[HathoraServerMgr.hathoraServerConfig.get] HathoraServerMgr exists, " +
                        "but !HathoraServerConfig -- Did you forget to serialize a config into your scene?");
                }

                return hathoraServerConfig;
            }
        }
        
        [Header("API Wrappers for Hathora SDK - See ServerApiContainer.cs")]
        [SerializeField]
        private ServerApiContainer serverApis;
        
        /// <summary>
        /// Get the Hathora Server SDK API wrappers for all wrapped Server APIs.
        /// (!) There may be high-level variants of the calls here; check 1st!
        /// </summary>
        public ServerApiContainer ServerApis => serverApis;

        // /// <summary>TODO: Mv to child</summary>
        // public static HathoraServerMgrBase Singleton { get; private set; }

        /// <summary>
        /// (!) This is set async on Awake; check for null.
        /// For the public accessor, `see GetSystemHathoraProcessAsync()`.
        /// </summary>
        private volatile Process cachedDeployedHathoraProcess;
        protected Process CachedDeployedHathoraProcess => cachedDeployedHathoraProcess;
        
        /// <summary>Set @ Awake, and only if deployed on Hathora</summary>
        private string serverDeployedProcessId;
        protected string ServerDeployedProcessId => serverDeployedProcessId;
        
        private bool hasServerDeployedProcessId =>
            !string.IsNullOrEmpty(serverDeployedProcessId);
        public bool HasServerDeployedProcessId => hasServerDeployedProcessId;
        #endregion // Vars

        
        #region Init
        protected virtual void Awake()
        {
#if !UNITY_SERVER && !UNITY_EDITOR
            Debug.Log("(!) [HathoraServerMgrBase.Awake] Destroying - not a server");
            Destroy(this);
            return;
#endif
            

            Debug.Log("[HathoraServerMgrBase] Awake");
            SetSingleton();

            // Unlike Client calls, we can init immediately @ Awake
            InitApis(_hathoraSdkConfig: null); // Base will create this

            
#if (UNITY_EDITOR)
            serverDeployedProcessId = getServerDeployedProcessId(debugEditorMockProcId);
#else
            serverDeployedProcessId = getServerDeployedProcessId();
#endif
            
            _ = GetHathoraProcessFromEnvVarAsync(); // !await
        }
        
        /// <summary>If we were not server || editor, we'd already be destroyed @ Awake</summary>
        protected virtual void Start()
        {
        }

        /// <param name="_overrideProcIdVal">Mock a val for testing within the Editor</param>
        protected virtual string getServerDeployedProcessId(string _overrideProcIdVal = null)
        {
            if (!string.IsNullOrEmpty(_overrideProcIdVal))
            {
                Debug.Log("[HathoraServerMgrBase.getServerDeployedProcessId] (!) Overriding " +
                    $"HATHORA_PROCESS_ID with mock val: `{_overrideProcIdVal}`");

                return _overrideProcIdVal;
            }
            return Environment.GetEnvironmentVariable("HATHORA_PROCESS_ID");
        }

        /// <summary>Set a singleton instance - well only ever have one serverMgr</summary>
        protected abstract void SetSingleton();
        // {
        //     if (Singleton != null)
        //     {
        //         Debug.LogError("[HathoraServerMgrBase.setSingleton] Error: " +
        //             "setSingleton: Destroying dupe");
        //         
        //         Destroy(gameObject);
        //         return;
        //     }
        //     
        //     Singleton = this;
        // }

        protected virtual bool ValidateReqs()
        {
            string logPrefix = $"[HathoraServerMgrBase{nameof(ValidateReqs)}]";
            
            if (hathoraServerConfig == null)
            {
#if UNITY_SERVER
                Debug.LogError($"{logPrefix} !HathoraServerConfig: " +
                    $"Serialize to {gameObject.name}.{nameof(HathoraServerMgrBase)} (if you want " +
                    "server runtime calls from Server standalone || Editor)");
                return false;
#elif UNITY_EDITOR
                Debug.Log($"<color=orange>(!)</color> {logPrefix} !HathoraServerConfig: Np in Editor, " +
                    "but if you want server runtime calls when you build as UNITY_SERVER, " +
                    $"serialize {gameObject.name}.{nameof(HathoraServerMgrBase)}");

#else
                // We're probably a Client - just silently stop this. Clients don't have a dev key.
#endif
                return false;
            }

            return true;
        }
        
        /// <summary>
        /// Call this when you 1st know well run Server runtime events.
        /// Init all Server [runtime] API wrappers. Passes serialized HathoraServerConfig.
        /// (!) Unlike ClientMgr that are Mono-derived, we init via Constructor instead of Init().
        /// </summary>
        /// <param name="_hathoraSdkConfig">We'll automatically create this, if empty</param>
        protected virtual void InitApis(Configuration _hathoraSdkConfig = null)
        {
            if (!ValidateReqs())
                return;
            
            serverApis.ServerAppApi = new HathoraServerAppApi(hathoraServerConfig, _hathoraSdkConfig);
            serverApis.ServerLobbyApi = new HathoraServerLobbyApi(hathoraServerConfig, _hathoraSdkConfig);
            serverApis.ServerProcessApi = new HathoraServerProcessApi(hathoraServerConfig, _hathoraSdkConfig);
            serverApis.ServerRoomApi = new HathoraServerRoomApi(hathoraServerConfig, _hathoraSdkConfig);
        }
        
        /// <summary>
        /// Gets the Server process info by a special env var that's
        /// *always* included (automatically) in Hathora deployments.
        /// You probably want to call this @ Awake, then get cached ver later @ GetCachedHathoraProcessAsync()
        /// </summary>
        protected virtual async Task GetHathoraProcessFromEnvVarAsync()
        {
            Debug.Log($"[HathoraServerMgrBase.getHathoraProcessAsync] " +
                $"HATHORA_PROCESS_ID: `{serverDeployedProcessId}`");
            
            bool hasHathoraProcId = !string.IsNullOrEmpty(serverDeployedProcessId);
            if (!hasHathoraProcId)
                return;
            
            this.cachedDeployedHathoraProcess = await serverApis.ServerProcessApi
                .GetProcessInfoAsync(serverDeployedProcessId);
        }
        #endregion // Init
        
        
        /// <summary>
        /// systemHathoraProcess tries to set async @ Awake, but it could still take some time.
        /// We'll await until != null for 5s before timing out.
        /// We initially set this @ Awake via getHathoraProcessFromEnvVarAsync.
        /// TODO: Accept custom cancelToken
        /// </summary>
        /// <returns></returns>
        public async Task<Process> GetCachedHathoraProcessAsync()
        {
            if (hathoraServerConfig == null || !hasServerDeployedProcessId)
                return null;

            // If we already have a cached Process, return it now ->
            if (cachedDeployedHathoraProcess != null)
                return cachedDeployedHathoraProcess;
            
            // ------------
            // Await up to 5s to become !null =>
            CancellationTokenSource cancellationTokenSource = new(TimeSpan.FromSeconds(5));
            await HathoraTaskUtils.WaitUntil(() => 
                cachedDeployedHathoraProcess != null, 
                _cancelToken: cancellationTokenSource.Token);

            while (cachedDeployedHathoraProcess == null)
            {
                if (cancellationTokenSource.IsCancellationRequested)
                    throw new TimeoutException($"[HathoraServerMgrBase.{nameof(GetCachedHathoraProcessAsync)}] Timed out");

                await Task.Delay(
                    TimeSpan.FromMilliseconds(100), 
                    cancellationTokenSource.Token);
            }

            return cachedDeployedHathoraProcess;
        }        
        
        
        #region Chained API calls
        /// <summary>
        /// Servers deployed in Hathora will have a special env var containing the ProcessId.
        /// From this, we can get Process, Room and Lobby info.
        /// - Note the GetLobbyInitConfig() call: Parse this `object` to your own model.
        /// - Throws if !Process || !Room
        /// - Does NOT throw if !Lobby (assert exists, if execting it).
        /// </summary>
        /// <param name="_cancelToken"></param>
        /// <returns></returns>
        public async Task<HathoraGetDeployInfoResult> ServerGetDeployedInfoAsync(
            CancellationToken _cancelToken = default)
        {
            string logPrefix = $"[[HathoraServerMgrBase.{nameof(ServerGetDeployedInfoAsync)}]";
            Debug.Log($"{logPrefix} ServerGetDeployedInfoAsync");

            HathoraGetDeployInfoResult getDeployInfoResult = new(serverDeployedProcessId);
            
            // ----------------
            // Get Process from env var "HATHORA_PROCESS_ID" => We probably cached this, already, @ )
            // We await => just in case we called this early, to prevent race conditions
            Process processInfo = await GetCachedHathoraProcessAsync();
            string procId = processInfo.ProcessId;
            if (string.IsNullOrEmpty(procId) || _cancelToken.IsCancellationRequested)
                return null;
            
            getDeployInfoResult.ProcessInfo = processInfo;

            // ----------------
            // Get all active Rooms by ProcessId =>
            List<PickRoomExcludeKeyofRoomAllocations> activeRooms =
                await ServerApis.ServerRoomApi.GetActiveRoomsForProcessAsync(procId, _cancelToken);

            // Get 1st Room -> validate
            PickRoomExcludeKeyofRoomAllocations firstActiveRoom = activeRooms?.FirstOrDefault();
            if (firstActiveRoom == null || _cancelToken.IsCancellationRequested)
            {
                Debug.LogError(_cancelToken.IsCancellationRequested ? "Cancelled" : "!firstActiveRoom");
                return null;
            }

            getDeployInfoResult.ActiveRoomsForProcess = activeRooms;
			
            // ----------------
            // We have Room info, but we need Lobby: Get from RoomId =>
            Lobby lobby = await ServerApis.ServerLobbyApi.GetLobbyInfoAsync(
                firstActiveRoom.RoomId,
                _cancelToken);
            
            if (_cancelToken.IsCancellationRequested)
            {
                Debug.LogError($"{logPrefix} Cancelled");
                return null;
            }
            
            if (lobby == null)
                Debug.LogError($"{logPrefix} !lobby, but continuing");

            getDeployInfoResult.Lobby = lobby;

            // Done
            return getDeployInfoResult;
        }
        #endregion // Chained API calls
    }
}
