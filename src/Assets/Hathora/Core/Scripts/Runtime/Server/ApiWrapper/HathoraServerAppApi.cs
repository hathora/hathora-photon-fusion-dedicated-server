// Created by dylan@hathora.dev

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Api;
using Hathora.Cloud.Sdk.Client;
using Hathora.Cloud.Sdk.Model;
using Debug = UnityEngine.Debug;

namespace Hathora.Core.Scripts.Runtime.Server.ApiWrapper
{
    public class HathoraServerAppApi : HathoraServerApiWrapperBase
    {
        private readonly AppV1Api appApi;

        
        /// <summary>
        /// </summary>
        /// <param name="_hathoraServerConfig"></param>
        /// <param name="_hathoraSdkConfig">
        /// Passed along to base for API calls as `HathoraSdkConfig`; potentially null in child.
        /// </param>
        public HathoraServerAppApi(
            HathoraServerConfig _hathoraServerConfig,
            Configuration _hathoraSdkConfig = null)
            : base(_hathoraServerConfig, _hathoraSdkConfig)
        { 
            Debug.Log("[HathoraServerAppApi] Initializing API..."); 
            this.appApi = new AppV1Api(base.HathoraSdkConfig);
        }
        
        
        #region Server App Async Hathora SDK Calls
        /// <summary>
        /// Wrapper for `CreateAppAsync` to upload and app a cloud app to Hathora.
        /// </summary>
        /// <param name="_cancelToken"></param>
        /// <returns>Returns App on success</returns>
        public async Task<List<ApplicationWithDeployment>> GetAppsAsync(
            CancellationToken _cancelToken = default)
        {
            List<ApplicationWithDeployment> getAppsResult;   
            try
            {  
                getAppsResult = await appApi.GetAppsAsync(_cancelToken);
            }
            catch (ApiException apiErr)
            {
                HandleApiException(
                    nameof(HathoraServerAppApi),
                    nameof(GetAppsAsync), 
                    apiErr);
                return null; 
            }

            Debug.Log($"[HathoraServerAppApi.GetAppsAsync] num: '{getAppsResult?.Count}'");

            return getAppsResult;
        }
        #endregion // Server App Async Hathora SDK Calls
    }
}
