// Created by dylan@hathora.dev

using System;
using System.Collections;
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
using UnityEngine.Assertions;

namespace Hathora.Core.Scripts.Runtime.Server
{
    /// <summary>
    /// Inits and centralizes all Hathora Server [runtime] API wrappers.
    /// - This is the entry point to call Hathora SDK: Auth, process, rooms, etc.
    /// - Opposed to the SDK itself, this gracefully wraps around it with callbacks + events.
    /// - Ready to be inheritted with protected virtual members, should you want to!
    /// </summary>
    public class HathoraServerMgr : MonoBehaviour
    {
        #region Vars
        public static HathoraServerMgr Singleton { get; private set; }
        
        /// <summary>Set null/empty to !fake a procId in the Editor</summary>
        [SerializeField, Tooltip("When in the Editor, we'll get this Hathora ConnectionInfo " +
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
        
        private ServerApiContainer serverApis;
        
        /// <summary>
        /// Get the Hathora Server SDK API wrappers for all wrapped Server APIs.
        /// (!) There may be high-level variants of the calls here; check 1st!
        /// </summary>
        public ServerApiContainer ServerApis => serverApis;

        /// <summary>
        /// (!) This is set async on Awake; check for null.
        /// - Publicly get via awaiting _GetServerContext()
        /// </summary>
        private volatile HathoraServerContext serverContext;
        
        
        #region Vars -> Deployed Env Vars // TODO: Mv to HathoraDeployedEnvVars
        // ####################################################
        // https://hathora.dev/docs/guides/access-env-variables
        // ####################################################
        
        /// <summary>Set @ Awake from "HATHORA_PROCESS_ID", and only if deployed on Hathora</summary>
        public string HathoraProcessIdEnvVar { get; private set; }
        
        /// <summary>Set @ Awake from "HATHORA_REGION", and only if deployed on Hathora</summary>
        public string HathoraRegionEnvVar { get; private set; }
        
        /// <summary>Set @ Awake from "HATHORA_IP", and only if deployed on Hathora</summary>
        public string HathoraPublicIpAddressEnvVar { get; private set; }

        /// <summary>Set @ Awake from "HATHORA_PORT", and only if deployed on Hathora</summary>
        public ushort HathoraPublicPortEnvVar { get; private set; }
        
        /// <summary>
        /// Set @ Awake from "HATHORA_APP_SECRET", and only if deployed on Hathora.
        /// Should match HathoraServerConfig's secret.
        /// </summary>
        private string hathoraAppSecretEnvVar;
        
        /// <summary>
        /// This will only be true if we're deployed on Hathora, by verifying
        /// a special env var ("HATHORA_PROCESS_ID").
        /// </summary>
        public bool IsDeployedOnHathora =>
            !string.IsNullOrEmpty(HathoraProcessIdEnvVar);
        #endregion Vars -> // Deployed Env Vars

        
        public static event Action<HathoraServerContext> OnInitializedEvent;
        #endregion // Vars

        
        #region Init
        /// <summary>If we were not server || editor, we'd already be destroyed @ Awake</summary>
        protected virtual void Start()
        {
            // Ideally, this would be placed in Awake, but spammy server logs often bury this result.
            // Subscribe to `OnInitializedEvent` to prevent race conditions.
            _ = GetHathoraServerContextAsync(_expectingLobby: false); // !await; sets `HathoraServerContext ServerContext` ^
        }

        /// <param name="_overrideProcIdVal">Mock a val for testing within the Editor</param>
        protected virtual string getServerDeployedProcessId(string _overrideProcIdVal = null)
        {
            if (!string.IsNullOrEmpty(_overrideProcIdVal))
            {
                Debug.Log($"[{nameof(HathoraServerMgr)}.{nameof(getServerDeployedProcessId)}] " +
                    $"(!) Overriding HATHORA_PROCESS_ID with mock val: `{_overrideProcIdVal}`");

                return _overrideProcIdVal;
            }
            
            return HathoraUtils.GetEnvVar("HATHORA_PROCESS_ID");
        }

        protected virtual async void Awake()
        {
#if !UNITY_SERVER && !UNITY_EDITOR
            Debug.Log("(!) [HathoraServerMgr.Awake] Destroying - not a server");
            Destroy(this);
            return;
#endif
            
            Debug.Log($"[{nameof(HathoraServerMgr)}] Awake");
            setSingleton();
            
#if (UNITY_EDITOR)
            // Optional mocked ID for debugging: Create a Room manually in Hathora console => paste ProcessId @ debugEditorMockProcId
            HathoraProcessIdEnvVar = getServerDeployedProcessId(debugEditorMockProcId); // Special, since it can be overridden for mock tests
#else
            setDeployedEnvVars();
#endif
            
            // Unlike Client calls, we can init immediately @ Awake
            InitApis(_hathoraSdkConfig: null); // Base will create this
        }

        private void setDeployedEnvVars()
        {
            HathoraProcessIdEnvVar = HathoraUtils.GetEnvVar("HATHORA_PROCESS_ID");
            HathoraRegionEnvVar = HathoraUtils.GetEnvVar("HATHORA_REGION");
            HathoraPublicIpAddressEnvVar = HathoraUtils.GetEnvVar("HATHORA_IP");
            
            ushort.TryParse(HathoraUtils.GetEnvVar("HATHORA_PORT"), out ushort _hathoraPublicPortEnvVar);
            this.HathoraPublicPortEnvVar = _hathoraPublicPortEnvVar;
            
            hathoraAppSecretEnvVar = HathoraUtils.GetEnvVar("HATHORA_APP_SECRET"); // Should match HathoraServerConfig's secret
        }

        /// <summary>
        /// Set a singleton instance - we'll only ever have one serverMgr.
        /// Children probably want to override this and, additionally, add a Child singleton
        /// </summary>
        private void setSingleton()
        {
            if (Singleton != null)
            {
                Debug.LogError($"[{nameof(HathoraServerMgr)}.{nameof(setSingleton)}] " +
                    "Error: Destroying dupe");
                
                Destroy(gameObject);
                return;
            }
            
            Singleton = this;
        }

        protected virtual bool ValidateReqs()
        {
            string logPrefix = $"[{nameof(HathoraServerMgr)}.{nameof(ValidateReqs)}]";
            
            if (hathoraServerConfig == null)
            {
#if UNITY_SERVER
                Debug.LogError($"{logPrefix} !HathoraServerConfig: " +
                    $"Serialize to {gameObject.name}.{nameof(HathoraServerMgr)} (if you want " +
                    "server runtime calls from Server standalone || Editor)");
                return false;
#elif UNITY_EDITOR
                Debug.Log($"<color=orange>(!)</color> {logPrefix} !HathoraServerConfig: Np in Editor, " +
                    "but if you want server runtime calls when you build as UNITY_SERVER, " +
                    $"serialize {gameObject.name}.{nameof(HathoraServerMgr)}");

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
        #endregion // Init
        
        
        #region ServerContext Getters
        /// <summary>
        /// Set @ Awake async, chained through 3 API calls - async to prevent race conditions.
        /// - If UNITY_SERVER: While !null, delay 0.1s until !null
        /// - If !UNITY_SERVER: Return cached null - this is only for deployed Hathora servers
        /// - Timeout after 10s
        /// </summary>
        /// <returns></returns>
        public async Task<HathoraServerContext> GetCachedServerContextAsync(
            CancellationToken _cancelToken = default)
        {
#if !UNITY_SERVER
            bool isMockTesting = !string.IsNullOrEmpty(debugEditorMockProcId);
            if (!isMockTesting)
                return null; // For headless servers deployed on Hathora only (and !mock testing)
#endif
            
            using CancellationTokenSource cts = new();
            cts.CancelAfter(TimeSpan.FromSeconds(10));
            
            if (serverContext != null)
                return serverContext;

            // We're probably still gathering the data => await for up to 10s
            string logPrefix = $"[{nameof(HathoraServerMgr)}.{nameof(GetCachedServerContextAsync)}]";
            Debug.Log($"{logPrefix} <color=orange>(!)</color> serverContext == null: " +
                "Awaiting up to 10s for val set async");
            
            return await waitForServerContextAsync(cts.Token);
        }
        
        /// <summary>
        /// [Coroutine alternative to async/await] Set @ Awake async, chained through 3 API calls -
        /// Async to prevent race conditions.
        /// - If UNITY_SERVER: While !null, delay 0.1s until !null
        /// - If !UNITY_SERVER: Return null - this is only for deployed Hathora servers
        /// - Timeout after 10s
        /// </summary>
        /// <returns></returns>
        public IEnumerator GetCachedServerContextCoroutine(Action<HathoraServerContext> _callback)
        {
            Task<HathoraServerContext> task = GetCachedServerContextAsync();

            // Wait until the task is completed
            while (!task.IsCompleted)
                yield return null; // Wait for the next frame

            // Handle any exceptions that were thrown by the task
            if (task.IsFaulted)
            {
                string logPrefix = $"[{nameof(HathoraServerMgr)}.{nameof(GetCachedServerContextCoroutine)}]";
                Debug.LogError($"{logPrefix} An error occurred while getting the server context: {task.Exception}");
            }
            else
            {
                // Retrieve the result and invoke the callback
                HathoraServerContext result = task.Result;
                _callback?.Invoke(result);
            }
        }

        private async Task<HathoraServerContext> waitForServerContextAsync(CancellationToken _cancelToken)
        {
            while (serverContext == null)
            {
                if (_cancelToken.IsCancellationRequested)
                {
                    string logPrefix = $"[{nameof(HathoraServerMgr)}.{nameof(GetCachedServerContextCoroutine)}]";
                    Debug.LogError($"{logPrefix} Timed out after 10s");
                    return null;
                }
                
                await Task.Delay(TimeSpan.FromSeconds(0.1), _cancelToken);
            }

            return serverContext;
        }
        #endregion // ServerContext Getters
        
        
        #region Chained API calls outside Init
        /// <summary>
        /// If Deployed (not localhost), get HathoraServerContext: { Room, Process, [Lobby], utils }.
        /// - (!) If cached info is ok, instead call `GetCachedHathoraServerContextAsync()`.
        /// - Servers deployed in Hathora will have a special env var containing the ProcessId (HATHORA_PROCESS_ID).
        /// - If env var exists, we're deployed in Hathora.
        /// - Note the result GetLobbyInitConfig() util: Parse this `object` to your own model.
        /// - Calls automatically @ Awake => triggers `OnInitializedEvent` on success.
        /// - Caches locally @ serverContext; public get via GetCachedServerContextAsync().
        /// - Logs a verbose summary on success.
        /// </summary>
        /// <param name="_expectingLobby">Throws if !Lobby; be extra sure to try/catch this, if true</param>
        /// <param name="_cancelToken"></param>
        /// <returns>Triggers `OnInitializedEvent` event on success</returns>
        public async Task<HathoraServerContext> GetHathoraServerContextAsync(
            bool _expectingLobby,
            CancellationToken _cancelToken = default)
        {
            string logPrefix = $"[{nameof(HathoraServerMgr)}.{nameof(GetHathoraServerContextAsync)}]";
            Debug.Log($"{logPrefix} Start");
            
            // Delay just 1 frame so the logs are closer to the bottom [Hathora Console workaround for max 1k logs viewed]
            await Task.Yield();
            
            if (!IsDeployedOnHathora)
            {
                #if UNITY_SERVER && !UNITY_EDITOR
                Debug.LogError($"{logPrefix} !serverDeployedProcessId; ensure: " +
                    "(1) HathoraServerConfig is serialized to a scene HathoraServerMgr, " +
                    "(2) We're deployed to Hathora (if testing locally, ignore this err)");
                #endif // UNITY_SERVER
                
                return null;
            }
            
            // ----------------
            // Log projections from env vars only set on deployed Hathora server | https://hathora.dev/docs/guides/access-env-variables 
            Debug.Log($"{logPrefix} Gathering verbose server context that should match similarly to deployed env vars:\n" +
                $"{nameof(HathoraProcessIdEnvVar)}={HathoraProcessIdEnvVar},\n" +
                // $"{nameof(hathoraAppSecretEnvVar)}={hathoraAppSecretEnvVar}\n"); // Uncomment to log your secret key (!recommended)
                $"{nameof(HathoraRegionEnvVar)}={HathoraRegionEnvVar}\n" +
                $"{nameof(HathoraPublicIpAddressEnvVar)}={HathoraPublicIpAddressEnvVar}\n" +
                $"{nameof(HathoraPublicPortEnvVar)}={HathoraPublicPortEnvVar}\n");
            
            // ----------------
            // Get Process from env var "HATHORA_PROCESS_ID" => Cached already @ Awake.
            // (!) Even with active Rooms, the Process itself may still be initializing!
            // (!) Don't trust the ExposedPort here -- trust the Room's ConnectionInfo `Active` status (that relates to Process), instead.
            // (!) Don't trust the Room's `Active` status; unrelated to a Process status.
            Process initializingProcess = await ServerApis.ServerProcessApi.GetProcessInfoAsync(
                HathoraProcessIdEnvVar, 
                _returnNullOnStoppedProcess: true,
                _cancelToken);
            
            string procId = initializingProcess?.ProcessId;
            
            if (_cancelToken.IsCancellationRequested)
            {
                Debug.LogError($"{logPrefix} Cancelled while getting Process - " +
                    "`OnInitialized` event will !trigger");
                return null;
            }
            if (string.IsNullOrEmpty(procId))
            {
                string errMsg = $"{logPrefix} !Process";

                // Are we debugging in the Editor? Add +info
                bool isMockDebuggingInEditor = UnityEngine.Application.isEditor && 
                    !string.IsNullOrEmpty(debugEditorMockProcId);
                if (isMockDebuggingInEditor)
                    errMsg += " <b>Since isEditor && debugEditorMockProcId, `your HathoraServerMgr.debugEditorMockProcId` " +
                        "is likely stale.</b> Create a new Room -> Update your `debugEditorMockProcId` -> try again.";
                
                Debug.LogError(errMsg);
                return null;
            }
            
            // ----------------
            // Get all active Rooms by ProcessId => There should be at least 1 
            List<PickRoomExcludeKeyofRoomAllocations> activeRooms =
                await ServerApis.ServerRoomApi.GetActiveRoomsForProcessAsync(procId, _cancelToken);

            if (activeRooms.Count == 0)
            {
                Debug.LogError($"{logPrefix} !activeRooms");
                return null;
            }
            
            // TODO: If we ever want to iterate *all* Rooms (rather than just 1st), loop here
            PickRoomExcludeKeyofRoomAllocations firstRoom = activeRooms.FirstOrDefault();
            RoomServerContext tempFirstRoomServerContext = await getRoomServerContextAsync(
                firstRoom, 
                _expectingLobby,
                _cancelToken);

            // Validate
            Assert.IsNotNull(tempFirstRoomServerContext);

            // ----------------
            // Combine all the data
            HathoraServerContext tempServerContext = new(
                HathoraProcessIdEnvVar,
                initializingProcess,
                activeRooms,
                tempFirstRoomServerContext.ConnectionInfo,
                tempFirstRoomServerContext.Lobby);
            
            // Validate integrity
            if (!tempServerContext.CheckIsValidServerContext(_expectingLobby))
            {
                Debug.LogError($"{logPrefix} !IsValidServerContext (at final integrity check)");
                return null;
            }
            
            // ----------------
            // Done - log server summary
            string summaryLogs = await tempServerContext.GetDebugSummary();
            Debug.Log($"{logPrefix} Done. <b>{nameof(HathoraServerContext)}:</b> {summaryLogs}");
            
            // Cache context -> trigger event -> return
            this.serverContext = tempServerContext;
            OnInitializedEvent?.Invoke(tempServerContext);
            return tempServerContext;
        }

        /// <summary>
        /// Called from GetServerContextAsync():
        /// - Validate, get ConnectionInfo, [get Lobby]
        /// - We generally pass the 1st Room (although it could be *any* Room)
        /// </summary>
        /// <param name="_roomInfo"></param>
        /// <param name="_throwErrIfNoLobby"></param>
        /// <param name="_cancelToken"></param>
        /// <returns>roomServerContext</returns>
        private async Task<RoomServerContext> getRoomServerContextAsync(
            PickRoomExcludeKeyofRoomAllocations _roomInfo,
            bool _throwErrIfNoLobby,
            CancellationToken _cancelToken)
        {
            string logPrefix = $"[{nameof(HathoraServerMgr)}.{nameof(getRoomServerContextAsync)}]";
            Assert.IsNotNull(_roomInfo, $"{logPrefix} Expected PickRoomExcludeKeyofRoomAllocations `room`");
            
            // Validate
            if (_cancelToken.IsCancellationRequested)
            {
                Debug.LogError($"{logPrefix} Cancelled while getting Room - " +
                    "`OnInitialized` event will !trigger");
                return null;
            }
                
            // ----------------
            // We have Room info, but the Process itself may not yet be active:  Poll for connection info `Active` status from RoomId.
            // - We can't simply check `Room` status since the `Process` may still be initializing. 
            // - Even if it's already active now (processInfo.ExposedPort != null), this extra info may still be useful to cache.
            ConnectionInfoV2 roomConnectionInfo = await serverApis.ServerRoomApi.PollConnectionInfoUntilActiveAsync(
                _roomInfo.RoomId, 
                _cancelToken);
            
            if (_cancelToken.IsCancellationRequested)
            {
                Debug.LogError($"{logPrefix} Cancelled while getting ConnectionInfo - " +
                    "`OnInitialized` event will !trigger");
                return null;
            }
            if (roomConnectionInfo == null)
            {
                Debug.LogError($"{logPrefix} !roomConnectionInfo (after polling for `Active` status) - " +
                    "`OnInitialized` event will !trigger");
                return null;
            }
            
            // ----------------
            // We have 1st Room + Connection info, but we *may* need Lobby: Get from RoomId =>
            Lobby roomLobby = null;
            try
            {
                // Try catch since we may not have a Lobby, which could be ok
                roomLobby = await ServerApis.ServerLobbyApi.GetLobbyInfoAsync(
                    _roomInfo.RoomId,
                    _cancelToken);
            }
            catch (Exception e)
            {
                // Should 404 if !Lobby, returning null
                if (_throwErrIfNoLobby)
                {
                    Debug.LogError($"Error: {e}");
                    throw;
                }
                
                Debug.Log($"{logPrefix} <b>!Lobby, but likely expected</b> " +
                    "(since !_expectingLobby) - continuing...");
            }

            if (_cancelToken.IsCancellationRequested)
            {
                Debug.LogError("Cancelled while getting optional Lobby");
                return null;
            }

            RoomServerContext roomServerContext = new(
                _roomInfo,
                roomConnectionInfo,
                roomLobby);

            return roomServerContext;
        }
        #endregion // Chained API calls outside Init
    }
}
