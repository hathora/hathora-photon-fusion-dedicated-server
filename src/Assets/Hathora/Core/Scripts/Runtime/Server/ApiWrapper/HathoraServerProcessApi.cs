// Created by dylan@hathora.dev

using System.Threading;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Api;
using Hathora.Cloud.Sdk.Client;
using Hathora.Cloud.Sdk.Model;
using Debug = UnityEngine.Debug;

namespace Hathora.Core.Scripts.Runtime.Server.ApiWrapper
{
    /// <summary>
    /// Operations to get data on active and stopped processes.
    /// API Docs | https://hathora.dev/api#tag/ProcessesV1 
    /// </summary>
    public class HathoraServerProcessApi : HathoraServerApiWrapperBase
    {
        private ProcessesV1Api processesApi;

        /// <summary>
        /// </summary>
        /// <param name="_hathoraServerConfig"></param>
        /// <param name="_hathoraSdkConfig">
        /// Passed along to base for API calls as `HathoraSdkConfig`; potentially null in child.
        /// </param>
        public HathoraServerProcessApi(
            HathoraServerConfig _hathoraServerConfig,
            Configuration _hathoraSdkConfig = null)
            : base(_hathoraServerConfig, _hathoraSdkConfig)
        { 
            Debug.Log("[HathoraServerProcessApi] Initializing API..."); 
            this.processesApi = new ProcessesV1Api(base.HathoraSdkConfig);
        }
        
        
        #region Server Process Async Hathora SDK Calls
        /// <summary>
        /// `GetProcessInfo` wrapper: Get details for an existing process using appId and processId.
        /// API Doc | https://hathora.dev/api#tag/ProcessesV1/operation/GetProcessInfo 
        /// </summary>
        /// <param name="_processId">
        /// The process running the Room; find it in web console or GetRunningProcesses().
        /// </param>
        /// <param name="_cancelToken"></param>
        /// <returns>Returns Process on success</returns>
        public async Task<Process> GetProcessInfoAsync(
            string _processId,
            CancellationToken _cancelToken = default)
        {
            const string logPrefix = "[HathoraServerProcessApi.GetProcessInfoAsync]";
            Debug.Log($"{logPrefix} <color=yellow>processId: {_processId}</color>");
            
            Process getProcessInfoResult = null;
            try
            {  
                getProcessInfoResult = await processesApi.GetProcessInfoAsync(
                    AppId,
                    _processId,
                    _cancelToken);
            }
            catch (ApiException apiErr)
            {
                HandleApiException(
                    nameof(HathoraServerProcessApi),
                    nameof(GetProcessInfoAsync), 
                    apiErr);
                return null; 
            }

            Debug.Log($"{logPrefix} Success: <color=yellow>" +
                $"getProcessInfoResult: {getProcessInfoResult.ToJson()}</color>");

            return getProcessInfoResult;
        }
        #endregion // Server Process Async Hathora SDK Calls
    }
}
