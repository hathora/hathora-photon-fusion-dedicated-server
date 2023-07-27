// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Api;
using Hathora.Cloud.Sdk.Client;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Server.Models;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hathora.Core.Scripts.Runtime.Server.ApiWrapper
{
    public class HathoraServerRoomApi : HathoraServerApiWrapperBase
    {
        private readonly RoomV2Api roomApi;
        private HathoraLobbyRoomOpts roomOpts => HathoraServerConfig.HathoraLobbyRoomOpts;
        public bool IsPollingForActiveConnInfo { get; private set; }
        
        /// <summary>
        /// </summary>
        /// <param name="_hathoraServerConfig"></param>
        /// <param name="_hathoraSdkConfig">
        /// Passed along to base for API calls as `HathoraSdkConfig`; potentially null in child.
        /// </param>
        public HathoraServerRoomApi( 
            HathoraServerConfig _hathoraServerConfig,
            Configuration _hathoraSdkConfig = null)
            : base(_hathoraServerConfig, _hathoraSdkConfig)
        {
            Debug.Log("[HathoraServerRoomApi] Initializing API...");
            this.roomApi = new RoomV2Api(base.HathoraSdkConfig);
        }
        
        
        #region Server Room Async Hathora SDK Calls
        /// <summary>
        /// This is a HIGH-LEVEL func that does multiple things:
        /// </summary>
        /// 
        /// <list type="number">
        /// <item>Wrapper for `CreateRoomAwaitActiveAsync` to create a new room in Hathora</item>
        /// <item>Poll GetConnectionInfoV2Async(roomId): takes ~5s, until Status is `Active`</item>
        /// <item>Once Active Status, get Room info</item>
        /// </list>
        /// 
        /// <param name="_customCreateRoomId"></param>
        /// <param name="_cancelToken"></param>
        /// <returns>both Room + [ACTIVE]ConnectionInfoV2 (ValueTuple) on success</returns>
        public async Task<(Room room, ConnectionInfoV2 connInfo)> CreateRoomAwaitActiveAsync(
            string _customCreateRoomId = null,
            CancellationToken _cancelToken = default)
        {
            // (1/3) Create Room
            string newlyCreatedRoomId = null;
            try
            {
                newlyCreatedRoomId = await CreateRoomAsync(_customCreateRoomId, _cancelToken);
            }
            catch (TaskCanceledException e)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"[HathoraServerRoomApi.CreateRoomAwaitActiveAsync] " +
                    $"Error => CreateRoomAsync: {e}");
                throw;
            }
            
            // ----------
            // (2/3) Poll until `Active` connection Status (or timeout)
            ConnectionInfoV2 activeConnectionInfo = null;

            try
            {
                activeConnectionInfo = await PollConnectionInfoUntilActiveAsync(
                    newlyCreatedRoomId,
                    _cancelToken);

                Assert.IsTrue(
                    activeConnectionInfo?.Status == ConnectionInfoV2.StatusEnum.Active,
                    "activeConnectionInfo !Active status (expected Active since room is Active");
            }
            catch (TaskCanceledException e)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError("[HathoraServerRoomApi.CreateRoomAwaitActiveAsync] " +
                    $"Error => PollConnectionInfoUntilActiveAsync: {e.Message} " +
                    "(Check console.hathora.dev logs for +info)");
                throw;
            }
            
            // ----------
            // (3/3) Once Connection Status is `Active`, get Room info
            Room activeRoom = null;
            try
            {
                activeRoom = await GetRoomInfoAsync(
                    newlyCreatedRoomId, 
                    _cancelToken);
                
                Assert.IsTrue(activeRoom?.Status == RoomStatus.Active, 
                    "activeRoom !Active status");
            }
            catch (TaskCanceledException e)
            {
                throw;
            }
            catch (Exception e)
            {
                Debug.LogError($"[HathoraServerRoomApi.CreateRoomAwaitActiveAsync] " +
                    $"Error => GetRoomInfoAsync: {e.Message} " +
                    "(Check console.hathora.dev logs for +info)");
                throw;
            }

            // ----------
            // Success
            Debug.Log($"[HathoraServerRoomApi.CreateRoomAwaitActiveAsync] Success: " +
                $"<color=yellow>activeConnectionInfo: {activeConnectionInfo.ToJson()}</color>");

            return (activeRoom, activeConnectionInfo);
        }

        /// <summary>
        /// Wrapper for `CreateRoomAwaitActiveAsync` to create a new room in Hathora.
        /// </summary>
        /// <param name="_customCreateRoomId"></param>
        /// <param name="_cancelToken"></param>
        /// <returns></returns>
        private async Task<string> CreateRoomAsync(
            string _customCreateRoomId = null,
            CancellationToken _cancelToken = default)
        {
            // Prep request data
            CreateRoomRequest createRoomReq = null;
            try
            {
                createRoomReq = new CreateRoomRequest(roomOpts.HathoraRegion);
                
                Debug.Log("[HathoraServerRoom.CreateRoomAwaitActiveAsync] " +
                    $"<color=yellow>roomConfig: {createRoomReq.ToJson()}</color>");
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }

            // Request call async =>
            ConnectionInfoV2 createRoomResultWithNullPort;
            try
            {
                // BUG: ExposedPort prop will always be null here; prop should be removed for CreateRoom.
                // To get the ExposedPort, we need to poll until Room Status is Active
                createRoomResultWithNullPort = await roomApi.CreateRoomAsync(
                    AppId,
                    createRoomReq,
                    _customCreateRoomId,
                    _cancelToken);
            }
            catch (TaskCanceledException)
            {
                // The user explicitly cancelled, or the Task timed out
                Debug.Log("[HathoraServerRoomApi.CreateRoomAsync] Task cancelled");
                return null;
            }
            catch (ApiException apiErr)
            {
                // HTTP err from Hathora Cloud
                HandleApiException(
                    nameof(HathoraServerRoomApi),
                    nameof(CreateRoomAwaitActiveAsync), 
                    apiErr);
                return null;
            }
            
            Debug.Log($"[HathoraServerRoomApi.CreateRoomAsync] Success: <color=yellow>" +
                $"createRoomResultWithNullPort: {createRoomResultWithNullPort.ToJson()}</color>");

            // Everything else in this result object is currently irrelevant except the RoomId
            return createRoomResultWithNullPort.RoomId;
        }
        
        /// <summary>
        /// When you get the result, check Status for Active.
        /// (!) If !Active, getting the ConnectionInfoV2 will result in !ExposedPort.
        /// </summary>
        /// <param name="_roomId"></param>
        /// <param name="_cancelToken"></param>
        /// <returns></returns>
        public async Task<Room> GetRoomInfoAsync(
            string _roomId, 
            CancellationToken _cancelToken = default)
        {
            Room getRoomInfoResult;

            try
            {
                getRoomInfoResult = await roomApi.GetRoomInfoAsync(
                    AppId,
                    _roomId,
                    _cancelToken);
            }
            catch (TaskCanceledException)
            {
                // The user explicitly cancelled, or the Task timed out
                Debug.Log("[HathoraServerRoomApi.GetRoomInfoAsync] Task cancelled");
                return null;
            }
            catch (ApiException apiErr)
            {
                // HTTP err from Hathora Cloud
                HandleApiException(
                    nameof(HathoraServerRoomApi),
                    nameof(GetRoomInfoAsync), 
                    apiErr);
                return null;
            }

            Debug.Log($"[HathoraServerRoomApi] Success: " +
                $"<color=yellow>getRoomInfoResult: {getRoomInfoResult.ToJson()}</color>");

            return getRoomInfoResult;
        }
        
        /// <summary>
        /// Get all active rooms for a given process using `appId + `processId`.
        /// API Doc | https://hathora.dev/api#tag/RoomV2/operation/GetActiveRoomsForProcess 
        /// </summary>
        /// <param name="_processId">
        /// System generated unique identifier to a runtime instance of your game server.
        /// Example: "cbfcddd2-0006-43ae-996c-995fff7bed2e"
        /// </param>
        /// <param name="_cancelToken"></param>
        /// <returns></returns>
        public async Task<List<PickRoomExcludeKeyofRoomAllocations>> GetActiveRoomsForProcessAsync(
            string _processId, 
            CancellationToken _cancelToken = default)
        {
            string logPrefix = $"[HathoraServerRoomApi].{nameof(GetActiveRoomsForProcessAsync)}]";
            List<PickRoomExcludeKeyofRoomAllocations> getActiveRoomsResultList = null;

            try
            {
                getActiveRoomsResultList = await roomApi.GetActiveRoomsForProcessAsync(
                    AppId,
                    _processId,
                    _cancelToken);
            }
            catch (TaskCanceledException)
            {
                // The user explicitly cancelled, or the Task timed out
                Debug.Log($"{logPrefix} Task cancelled");
                return null;
            }
            catch (ApiException apiErr)
            {
                // HTTP err from Hathora Cloud
                HandleApiException(
                    nameof(HathoraServerRoomApi),
                    nameof(GetRoomInfoAsync), 
                    apiErr);
                return null;
            }

            Debug.Log($"{logPrefix} Success: <color=yellow>" +
                $"getActiveRoomsResultList count: {getActiveRoomsResultList.Count}</color>");

            if (getActiveRoomsResultList.Count > 0)
            {
                Debug.Log($"{logPrefix} Success: <color=yellow>" +
                    $"getActiveRoomsResultList[0]: {getActiveRoomsResultList[0].ToJson()}</color>");
            }

            return getActiveRoomsResultList;
        }

        /// <summary>
        /// (!) If the Room you created has a Status if !Active, the
        /// Result `ExposedPort` prop here will be null.
        /// </summary>
        /// <param name="_roomId"></param>
        /// <param name="_cancelToken"></param>
        /// <returns></returns>
        public async Task<ConnectionInfoV2> GetConnectionInfoAsync(
            string _roomId, 
            CancellationToken _cancelToken = default)
        {
            ConnectionInfoV2 getConnectionInfoResult;

            try
            {
                getConnectionInfoResult = await roomApi.GetConnectionInfoAsync(
                    AppId,
                    _roomId,
                    _cancelToken);
            }
            catch (TaskCanceledException)
            {
                // The user explicitly cancelled, or the Task timed out
                Debug.Log("[HathoraServerRoomApi.GetConnectionInfoAsync] Task cancelled");
                return null;
            }
            catch (ApiException apiErr)
            {
                // HTTP err from Hathora Cloud
                HandleApiException(
                    nameof(HathoraServerRoomApi),
                    nameof(GetConnectionInfoAsync), 
                    apiErr);
                return null;
            }
            
            bool isActiveWithExposedPort = 
                getConnectionInfoResult.Status == ConnectionInfoV2.StatusEnum.Active && 
                getConnectionInfoResult.ExposedPort != null;

            Debug.Log($"[HathoraServerRoomApi] Success: " +
                $"isActiveWithExposedPort? {isActiveWithExposedPort}, " +
                $"<color=yellow>getConnectionInfoResult: {getConnectionInfoResult.ToJson()}</color>");

            return getConnectionInfoResult;
        }

        // ------------------------------------------
        // Utils >>
        
        /// <summary>ETA 5 seconds; poll once/second.</summary>
        /// <param name="_roomId"></param>
        /// <param name="_cancelToken"></param>
        /// <returns></returns>
        public async Task<ConnectionInfoV2> PollConnectionInfoUntilActiveAsync(
            string _roomId,
            CancellationToken _cancelToken = default)
        {
            ConnectionInfoV2 connectionInfo = null;
            int attemptNum = 0;
            IsPollingForActiveConnInfo = true;
            
            while (connectionInfo is not { Status: ConnectionInfoV2.StatusEnum.Active })
            {
                // Check for cancel + await 1s
                if (_cancelToken.IsCancellationRequested)
                {
                    IsPollingForActiveConnInfo = false;
                    _cancelToken.ThrowIfCancellationRequested();
                }
                
                await Task.Delay(TimeSpan.FromSeconds(1), _cancelToken);

                // ---------------
                // Try again
                attemptNum++;
                Debug.Log("[HathoraConfigPostAuthBodyRoomUI.PollConnectionInfoUntilActiveAsync] " +
                    $"Attempt #{attemptNum} ...");
                
                connectionInfo = await GetConnectionInfoAsync(
                    _roomId,
                    _cancelToken: _cancelToken);
                
                // ---------------
                if (connectionInfo?.Status == ConnectionInfoV2.StatusEnum.Active)
                    break; // Success
                
                // ---------------
                // Still !Active -- log, then try again
                Debug.LogWarning("[HathoraConfigPostAuthBodyRoomUI.PollConnectionInfoUntilActiveAsync] " +
                    "Room !Active (yet) - attempting to poll again ...");
            }
            
            
            Debug.Log($"[HathoraConfigPostAuthBodyRoomUI.PollConnectionInfoUntilActiveAsync] " +
                $"Success: <color=yellow>{nameof(connectionInfo)}: {connectionInfo.ToJson()}</color>");

            IsPollingForActiveConnInfo = false;
            return connectionInfo;
        }
        #endregion // Server Room Async Hathora SDK Calls
    }
}
