// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Api;
using Hathora.Cloud.Sdk.Client;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Common.Utils;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Server.ApiWrapper
{
    public class HathoraServerBuildApi : HathoraServerApiWrapperBase
    {
        private readonly BuildV1Api buildApi;
        private volatile bool uploading;

        /// <summary>
        /// </summary>
        /// <param name="_hathoraServerConfig"></param>
        /// <param name="_hathoraSdkConfig">
        /// Set in base as `HathoraSdkConfig`; potentially null in child.
        /// </param>
        public HathoraServerBuildApi(
            HathoraServerConfig _hathoraServerConfig,
            Configuration _hathoraSdkConfig = null) 
            : base(_hathoraServerConfig, _hathoraSdkConfig)
        {
            Debug.Log("[HathoraServerBuildApi] Initializing API...");
            this.buildApi = new BuildV1Api(base.HathoraSdkConfig);
        }
        
        
        
        #region Server Build Async Hathora SDK Calls
        /// <summary>
        /// Wrapper for `CreateBuildAsync` to request an cloud build (_tarball upload).
        /// </summary>
        /// <param name="_cancelToken"></param>
        /// <returns>Returns Build on success >> Pass this info to RunCloudBuildAsync()</returns>
        public async Task<Build> CreateBuildAsync(CancellationToken _cancelToken = default)
        {
            Build createCloudBuildResult;
            
            try
            {
                createCloudBuildResult = await buildApi.CreateBuildAsync(
                    HathoraServerConfig.HathoraCoreOpts.AppId,
                    _cancelToken);
            }
            catch (ApiException apiException)
            {
                HandleApiException(
                    nameof(HathoraServerBuildApi),
                    nameof(CreateBuildAsync), 
                    apiException);
                return null;
            }

            Debug.Log($"[HathoraServerRoomApi.CreateBuildAsync] Success: " +
                $"<color=yellow>createCloudBuildResult: {createCloudBuildResult.ToJson()}</color>");

            return createCloudBuildResult;
        }

        /// <summary>
        /// Wrapper for `RunBuildAsync` to upload the _tarball after calling CreateBuildAsync().
        /// (!) Temporarily sets the Timeout to 15min (900k ms) to allow for large builds.
        /// (!) After this is done, you probably want to call GetBuildInfoAsync().
        /// </summary>
        /// <param name="_buildId"></param>
        /// <param name="_pathToTarGzBuildFile">Ensure path is normalized</param>
        /// <param name="_cancelToken"></param>
        /// <returns>Returns streamLogs (List of chunks) on success</returns>
        public async Task<List<string>> RunCloudBuildAsync(
            double _buildId, 
            string _pathToTarGzBuildFile,
            CancellationToken _cancelToken = default)
        {
            byte[] cloudRunBuildResultLogsStream = null;

            #region Timeout Workaround
            // Temporarily sets the Timeout to 15min (900k ms) to allow for large builds.
            // Since Timeout has no setter, we need to temporarily make a new api instance.
            Configuration highTimeoutConfig = HathoraUtils.DeepCopy(base.HathoraSdkConfig);
            highTimeoutConfig.Timeout = (int)TimeSpan.FromMinutes(15).TotalMilliseconds;

            BuildV1Api highTimeoutBuildApi = new(highTimeoutConfig);
            #endregion // Timeout Workaround
         
            uploading = true;

            try
            {
                _ = startProgressNoticeAsync(); // !await

                await using FileStream fileStream = new(
                    _pathToTarGzBuildFile,
                    FileMode.Open,
                    FileAccess.Read);

                // (!) Using the `highTimeoutBuildApi` workaround instance here
                cloudRunBuildResultLogsStream = await highTimeoutBuildApi.RunBuildAsync(
                    HathoraServerConfig.HathoraCoreOpts.AppId,
                    _buildId,
                    fileStream,
                    _cancelToken);
            }
            catch (TaskCanceledException)
            {
                Debug.Log("[HathoraServerBuildApi.RunCloudBuildAsync] Task Cancelled || timed out");
            }
            catch (ApiException apiException)
            {
                HandleApiException(
                    nameof(HathoraServerBuildApi),
                    nameof(RunCloudBuildAsync),
                    apiException);

                return null;
            }
            finally
            {
                uploading = false;
            }

            Debug.Log($"[HathoraServerBuildApi.RunCloudBuildAsync] Done - " +
                "to know if success, call buildApi.RunBuild");

            // (!) Unity, by default, truncates logs to 1k chars (including callstack).
            string encodedLogs = Encoding.UTF8.GetString(cloudRunBuildResultLogsStream);
            List<string> logChunks = onRunCloudBuildDone(encodedLogs);
            
            return logChunks;  // streamLogs 
        }

        private async Task startProgressNoticeAsync()
        {
            TimeSpan delayTimespan = TimeSpan.FromSeconds(5);
            StringBuilder sb = new("...");
            
            while (uploading)
            {
                Debug.Log($"[HathoraServerBuild] Uploading {sb}");
                
                await Task.Delay(delayTimespan);
                sb.Append(".");
            }
        }

        /// <summary>
        /// DONE - not necessarily success. Log stream every 500 lines
        /// (!) Unity, by default, truncates logs to 1k chars (including callstack).
        /// </summary>
        /// <param name="_cloudRunBuildResultLogsStr"></param>
        /// <returns>List of log chunks</returns>
        private static List<string> onRunCloudBuildDone(string _cloudRunBuildResultLogsStr)
        {
            // Split string into lines
            string[] linesArr = _cloudRunBuildResultLogsStr.Split(new[] 
                { "\r\n", "\r", "\n" }, StringSplitOptions.None);
            List<string> lines = new (linesArr);

            // Group lines into chunks of 500
            const int chunkSize = 500;
            for (int i = 0; i < lines.Count; i += chunkSize)
            {
                IEnumerable<string> chunk = lines.Skip(i).Take(chunkSize);
                string chunkStr = string.Join("\n", chunk);
                Debug.Log($"[HathoraServerBuildApi.onRunCloudBuildDone] result == chunk starting at line {i}: " +
                    $"\n<color=yellow>{chunkStr}</color>");
            }

            return lines;
        }

        /// <summary>
        /// Wrapper for `RunBuildAsync` to upload the _tarball after calling 
        /// </summary>
        /// <param name="_buildId"></param>
        /// <param name="_cancelToken"></param>
        /// <returns>Returns byte[] on success</returns>
        public async Task<Build> GetBuildInfoAsync(
            double _buildId,
            CancellationToken _cancelToken)
        {
            Build getBuildInfoResult;
            
            try
            {
                getBuildInfoResult = await buildApi.GetBuildInfoAsync(
                    HathoraServerConfig.HathoraCoreOpts.AppId,
                    _buildId,
                    _cancelToken);
            }
            catch (ApiException apiException)
            {
                HandleApiException(
                    nameof(HathoraServerBuildApi),
                    nameof(GetBuildInfoAsync), 
                    apiException);
                return null;
            }

            bool isSuccess = getBuildInfoResult is { Status: Build.StatusEnum.Succeeded };
            Debug.Log($"[HathoraServerRoomApi.GetBuildInfoAsync] Success? {isSuccess}, " +
                $"<color=yellow>createCloudBuildResult: {getBuildInfoResult.ToJson()}</color>");

            return getBuildInfoResult;
        }
        #endregion // Server Build Async Hathora SDK Calls
    }
}
