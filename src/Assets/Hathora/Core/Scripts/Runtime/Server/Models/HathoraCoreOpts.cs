// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Linq;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Common.Models;
using Hathora.Core.Scripts.Runtime.Common.Utils;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Server.Models
{
    [Serializable]
    public class HathoraCoreOpts
    {
        /// <summary>Get from your Hathora dashboard.</summary>
        [SerializeField]
        private string _appId;

        [SerializeField]
        private int _existingAppsSelectedIndex = -1;

        /// <summary>Get from your Hathora dashboard</summary>
        public string AppId => ExistingAppsWithDeployment != null && ExistingAppsWithDeployment.Count > 0 && _existingAppsSelectedIndex > -1 && _existingAppsSelectedIndex < ExistingAppsWithDeployment.Count
            ? ExistingAppsWithDeployment?[_existingAppsSelectedIndex]?.AppId
            : null;
        
        public bool HasAppId => !string.IsNullOrEmpty(AppId);
        
        public int ExistingAppsSelectedIndex
        {
            get => _existingAppsSelectedIndex;
            set => _existingAppsSelectedIndex = value;
        }

        /// <summary>Ported from `ApplicationWithDeployment`</summary>
        [SerializeField]
        private List<ApplicationWithDeploymentWrapper> _existingAppsWithDeploymentWrapper = new();
        
        public List<ApplicationWithDeploymentWrapper> ExistingAppsWithDeploymentWrapper
        {
            get => _existingAppsWithDeploymentWrapper;
            set => _existingAppsWithDeploymentWrapper = value;
        }

        /// <summary>Ported from `ApplicationWithDeployment`</summary>
        public List<ApplicationWithDeployment> ExistingAppsWithDeployment
        {
            get {
                if (this._existingAppsWithDeploymentWrapper == null)
                    return new List<ApplicationWithDeployment>();
                
                // Parse from `ApplicationWithDeploymentWrapper` to `ApplicationWithDeployment` 
                List<ApplicationWithDeployment> appsWithDeployment = this._existingAppsWithDeploymentWrapper
                    .Select(app => app.ToApplicationWithDeploymentType())
                    .ToList();

                return appsWithDeployment;
            }
            
            set  
            {
                if (value == null)
                    return;
                
                List<ApplicationWithDeploymentWrapper> parsedList = _existingAppsWithDeploymentWrapper = value
                    .Select(app => new ApplicationWithDeploymentWrapper(app))
                    .ToList();

                this._existingAppsWithDeploymentWrapper = parsedList;
            }
        }

        /// <summary>Cached from App API.</summary>
        /// <param name="_prependDummyIndex0Str">
        /// (!) Hathora SDK Enums starts at index 1; not 0: Care of indexes.
        /// </param>
        public List<string> GetExistingAppNames(string _prependDummyIndex0Str)
        {
            if (_existingAppsWithDeploymentWrapper == null)
                return new List<string>();

            IEnumerable<string> enumerable = _existingAppsWithDeploymentWrapper?
                .Select(app => app.AppName);

            if (!string.IsNullOrEmpty(_prependDummyIndex0Str))
            {
                if (HathoraUtils.SDK_ENUM_STARTING_INDEX == 0)
 #pragma warning disable CS0162 // Don't spam logs for `Unreachable code detected`
                {
                    Debug.LogWarning("HathoraUtils.SDK_ENUM_STARTING_INDEX == 0, " +
                        "but you are using a _prependDummyIndex0Str: Intentional?");
                }    
 #pragma warning restore CS0162 // Don't spam logs for `Unreachable code detected`
                
                enumerable = enumerable.Prepend(_prependDummyIndex0Str);
            }

            return enumerable?.ToList();
        }

        /// <summary>Doc | https://hathora.dev/docs/guides/generate-admin-token</summary>
#if UNITY_SERVER || UNITY_EDITOR
        [SerializeField, Tooltip("Set earlier from log in button")]
        private HathoraDevAuthTokenOpts _devAuthOpts = new();
#endif
        
        /// <summary>Doc | https://hathora.dev/docs/guides/generate-admin-token</summary>
        public HathoraDevAuthTokenOpts DevAuthOpts
        {
            get {
#if UNITY_SERVER || UNITY_EDITOR
                return _devAuthOpts;
#endif
                
                // Client >>
                return null;
            }
#if UNITY_SERVER || UNITY_EDITOR
            set => _devAuthOpts = value;
#endif
        }
    }
}
