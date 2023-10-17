// Created by dylan@hathora.dev

using System;
using System.Threading;
using System.Threading.Tasks;
using HathoraCloud;
using HathoraCloud.Models.Operations;
using HathoraCloud.Models.Shared;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hathora.Core.Scripts.Runtime.Server.ApiWrapper
{
    /// <summary>
    /// Operations to create and manage lobbies.
    /// Lobbies Concept | https://hathora.dev/docs/concepts/hathora-entities#lobby
    /// API Docs | https://hathora.dev/api#tag/LobbyV2
    /// </summary>
    public class HathoraServerLobbyApiWrapper : HathoraServerApiWrapperBase
    {
        protected LobbyV2SDK LobbyApi { get; }
        
        public HathoraServerLobbyApiWrapper(
            HathoraCloudSDK _hathoraSdk,
            HathoraServerConfig _hathoraServerConfig)
            : base(_hathoraSdk, _hathoraServerConfig)
        {
            Debug.Log($"[{nameof(HathoraServerLobbyApiWrapper)}.Constructor] " +
                "Initializing Server API...");
            
            this.LobbyApi = _hathoraSdk.LobbyV2 as LobbyV2SDK;
        }
        
        
        #region Server Lobby Async Hathora SDK Calls
        /// <summary>
        /// When you get the result, check Status for Active.
        /// (!) If !Active, getting the ConnectionInfoV2 will result in !ExposedPort.
        /// </summary>
        /// <param name="_roomId"></param>
        /// <param name="_cancelToken"></param>
        /// <returns></returns>
        public async Task<Lobby> GetLobbyInfoAsync(
            string _roomId, 
            CancellationToken _cancelToken = default)
        {
            string logPrefix = $"[{nameof(HathoraServerLobbyApiWrapper)}.{nameof(GetLobbyInfoAsync)}]";

            // Prepare request
            GetLobbyInfoRequest getLobbyInfoRequest = new()
            {
                RoomId = _roomId,
            };
            
            // Get response async =>
            GetLobbyInfoResponse getLobbyInfoResult = null;

            try
            {
                getLobbyInfoResult = await LobbyApi.GetLobbyInfoAsync(getLobbyInfoRequest);
                Assert.IsNotNull(getLobbyInfoResult, $"{logPrefix} !getLobbyInfoResult");
            }
            catch (TaskCanceledException)
            {
                // The user explicitly cancelled, or the Task timed out
                Debug.Log($"{logPrefix} Task cancelled");
                return null;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} {nameof(LobbyApi.GetLobbyInfoAsync)} => Error: {e.Message}");
                return null; // fail
            }

            // Process result
            Debug.Log($"{logPrefix} Success: <color=yellow>{nameof(getLobbyInfoResult.Lobby)}: " +
                $"{ToJson(getLobbyInfoResult.Lobby)}</color>");

            getLobbyInfoResult.RawResponse?.Dispose(); // Prevent mem leaks
            return getLobbyInfoResult.Lobby;
        }
        #endregion // Server Lobby Async Hathora SDK Calls
    }
}
