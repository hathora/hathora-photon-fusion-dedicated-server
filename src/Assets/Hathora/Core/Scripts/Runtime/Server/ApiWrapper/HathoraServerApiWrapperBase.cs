// Created by dylan@hathora.dev

using Hathora.Cloud.Sdk.Client;
using Hathora.Core.Scripts.Runtime.Common.Models;
using Debug = UnityEngine.Debug;

namespace Hathora.Core.Scripts.Runtime.Server.ApiWrapper
{
    /// <summary>
    /// This allows the API to view UserConfig (eg: AppId), set session and auth tokens.
    /// Server APIs can inherit from this.
    /// Unlike Client API wrappers (since !Mono), we init via Constructor instead of Init().
    /// </summary>
    public abstract class HathoraServerApiWrapperBase : IHathoraApiBase
    {
        public Configuration HathoraSdkConfig { get; set; }
        protected HathoraServerConfig HathoraServerConfig { get; private set; }

        // Shortcuts
        public string AppId
        {
            get {
                if (HathoraServerConfig == null)
                {
                    Debug.LogError("[HathoraServerApiWrapper.AppId.get] !HathoraServerConfig: " +
                        "Did you forget to add init a newly-added API @ HathoraServerMgr.initApis()?" +
                        "**For Non-host Clients (or Servers that don't have runtime Server API calls), you may ignore this**");
                    return null;
                }

                if (HathoraServerConfig.HathoraCoreOpts == null)
                {
                    Debug.LogError("[HathoraServerApiWrapper.AppId.get] HathoraServerConfig exists, " +
                        "but !HathoraServerConfig.HathoraCoreOpts");
                    return null;
                }

                if (string.IsNullOrEmpty(HathoraServerConfig.HathoraCoreOpts.AppId))
                {
                    Debug.LogError("[HathoraServerApiWrapper.AppId.get] " +
                        "!HathoraServerConfig.HathoraCoreOpts.AppId -- " +
                        "Did you configure your HathoraServerConfig?");
                    return null;
                }

                return HathoraServerConfig.HathoraCoreOpts.AppId;
            }
        }
        

        #region Init
        /// <summary>
        /// Server calls use Dev token. Unlike ClientMgr, Server !inherits
        /// from Mono (so we init via constructor).
        /// </summary>
        /// <param name="_hathoraServerConfig">
        /// Find via Unity editor top menu: Hathora >> Find Configs
        /// </param>
        /// <param name="_hathoraSdkConfig">
        /// Passed along to base for API calls as `HathoraSdkConfig`; potentially null in children.
        /// </param>
        protected HathoraServerApiWrapperBase(
            HathoraServerConfig _hathoraServerConfig,
            Configuration _hathoraSdkConfig = null)
        {
            this.HathoraServerConfig = _hathoraServerConfig;
            this.HathoraSdkConfig = _hathoraSdkConfig ?? GenerateSdkConfig();
        }
        
        public Configuration GenerateSdkConfig() => new()
        {
            AccessToken = HathoraServerConfig.HathoraCoreOpts.DevAuthOpts.DevAuthToken,
        };        
        #endregion // Init
        

        public void HandleApiException(
            string _className, 
            string _funcName,
            ApiException _apiException)
        {
            UnityEngine.Debug.LogError($"[{_className}.{_funcName}] API Error: " +
                $"{_apiException.ErrorCode} {_apiException.ErrorContent} | {_apiException.Message}");

            throw _apiException;
        }
    }
}
