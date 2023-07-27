// Created by dylan@hathora.dev

using System.Threading;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Api;
using Hathora.Cloud.Sdk.Client;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Client.Config;
using Hathora.Core.Scripts.Runtime.Client.Models;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Client.ApiWrapper
{
    /// <summary>
    /// High-level API wrapper for the low-level Hathora SDK's Auth API.
    /// * Caches SDK Config and HathoraClientConfig for API use. 
    /// * Try/catches async API calls and [Base] automatically handlles API Exceptions.
    /// * Due to code autogen, the SDK exposes too much: This simplifies and minimally exposes.
    /// * Due to code autogen, the SDK sometimes have nuances: This provides fixes/workarounds.
    /// * Call Init() to pass HathoraClientConfig + Hathora SDK Config (see HathoraClientMgr).
    /// * Does not handle UI (see HathoraClientMgrUi).
    /// * Does not handle Session caching (see HathoraClientSession).
    /// </summary>
    public class HathoraClientAuthApi : HathoraClientApiWrapperBase
    {
        private AuthV1Api authApi;

        
        /// <summary>
        /// </summary>
        /// <param name="_hathoraClientConfig"></param>
        /// <param name="_hathoraSdkConfig">
        /// Passed along to base for API calls as `HathoraSdkConfig`; potentially null in child.
        /// </param>
        public override void Init(
            HathoraClientConfig _hathoraClientConfig, 
            Configuration _hathoraSdkConfig = null)
        {
            Debug.Log("[NetHathoraClientAuthApi] Initializing API...");
            base.Init(_hathoraClientConfig, _hathoraSdkConfig);
            this.authApi = new AuthV1Api(base.HathoraSdkConfig);
        }


        #region Client Auth Async Hathora SDK Calls
        /// <param name="_cancelToken"></param>
        /// <returns>Returns AuthResult on success</returns>
        public async Task<AuthResult> ClientAuthAsync(CancellationToken _cancelToken = default)
        {
            Debug.Log("[HathoraNetClientAuthApi] ClientAuthAsync");
            
            LoginResponse anonLoginResult;
            try
            {
                anonLoginResult = await authApi.LoginAnonymousAsync(
                    HathoraClientConfig.AppId, 
                    _cancelToken);
            }
            catch (ApiException apiException)
            {
                HandleApiException(
                    nameof(HathoraClientAuthApi),
                    nameof(ClientAuthAsync), 
                    apiException);
                return null;
            }

            bool isAuthed = !string.IsNullOrEmpty(anonLoginResult?.Token); 
            
            
#if UNITY_EDITOR
            // For security, we probably only want to log this in the editor
            Debug.Log($"[NetHathoraClientAuthApi] isAuthed: {isAuthed}, " +
                $"<color=yellow>anonLoginResult: {anonLoginResult?.ToJson()}</color>");
#else
            Debug.Log($"[NetHathoraClientAuthApi] isAuthed: {isAuthed}");
#endif
            
            
            return isAuthed
                ? new AuthResult(anonLoginResult.Token)
                : null;

        }
        #endregion // Server Auth Async Hathora SDK Calls
    }
}
