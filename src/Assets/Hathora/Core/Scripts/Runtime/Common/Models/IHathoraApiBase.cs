// Created by dylan@hathora.dev

using Hathora.Cloud.Sdk.Client;

namespace Hathora.Core.Scripts.Runtime.Common.Models
{
    /// <summary>
    /// Common reqs for HathoraServerApiBase || HathoraClientApiBase
    /// </summary>
    public interface IHathoraApiBase
    {
        Configuration HathoraSdkConfig { get; set; }
        string AppId { get; }
        Configuration GenerateSdkConfig();
        void HandleApiException(
            string _className,
            string _funcName,
            ApiException _apiException);
    }
}
