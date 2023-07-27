// Created by dylan@hathora.dev

using System.Threading;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Api;
using Hathora.Cloud.Sdk.Client;
using Hathora.Cloud.Sdk.Model;
using UnityEngine;
using UnityEngine.Assertions;

namespace Hathora.Core.Scripts.Runtime.Server.ApiWrapper
{
    /// <summary>
    /// V2 API Doc | https://hathora.dev/api#tag/LobbyV2
    /// </summary>
    public class HathoraServerLobbyApi : HathoraServerApiWrapperBase
    {
        private readonly LobbyV2Api lobbyApi;
        
        /// <summary>
        /// </summary>
        /// <param name="_hathoraServerConfig"></param>
        /// <param name="_hathoraSdkConfig">
        /// Passed along to base for API calls as `HathoraSdkConfig`; potentially null in child.
        /// </param>
        public HathoraServerLobbyApi( 
            HathoraServerConfig _hathoraServerConfig,
            Configuration _hathoraSdkConfig = null)
            : base(_hathoraServerConfig, _hathoraSdkConfig)
        {
            Debug.Log("[HathoraServerLobbyApi] Initializing API...");
            this.lobbyApi = new LobbyV2Api(base.HathoraSdkConfig);
        }
        
        
        #region Server Lobby Async Hathora SDK Calls
        /// <summary>
        /// When you get the result, check Status for Active.
        /// (!) If !Active, getting the ConnectionInfoV2 will result in !ExposedPort.
        /// </summary>
        /// <param name="_lobbyId"></param>
        /// <param name="_cancelToken"></param>
        /// <returns></returns>
        public async Task<Lobby> GetLobbyInfoAsync(
            string _lobbyId, 
            CancellationToken _cancelToken = default)
        {
            string logPrefix = $"[{nameof(HathoraServerLobbyApi)}.{nameof(GetLobbyInfoAsync)}]";
            Lobby getLobbyInfoResult;

            try
            {
                getLobbyInfoResult = await lobbyApi.GetLobbyInfoAsync(
                    AppId,
                    _lobbyId,
                    _cancelToken);
                
                Assert.IsNotNull(getLobbyInfoResult, $"{logPrefix} !getLobbyInfoResult");
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
                    nameof(HathoraServerLobbyApi),
                    nameof(GetLobbyInfoAsync), 
                    apiErr);
                return null;
            }

            Debug.Log($"{logPrefix} Success: <color=yellow>getLobbyInfoResult: " +
                $"{getLobbyInfoResult.ToJson()}</color>");

            return getLobbyInfoResult;
        }
        #endregion // Server Lobby Async Hathora SDK Calls
    }
}
