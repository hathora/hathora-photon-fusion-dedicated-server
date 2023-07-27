// Created by dylan@hathora.dev

using System;
using Newtonsoft.Json;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Common.Models
{
    /// <summary>
    /// Generally, most (if not all) SDK models include AppId.
    /// More to certainly be added later.
    /// </summary>
    [Serializable]
    public class SdkWrapperBase : MonoBehaviour
    {
        [SerializeField, JsonProperty("appId")]
        private string _appId;
        public string AppId
        {
            get => _appId;
            set => _appId = value;
        }
        
        
    }
}
