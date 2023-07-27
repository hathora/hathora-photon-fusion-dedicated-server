// Created by dylan@hathora.dev

using System;
using Hathora.Cloud.Sdk.Model;

namespace Hathora.Core.Scripts.Runtime.Common.Models
{
    /// <summary>
    /// Hathora SDK model wrapper to add [Serializable] support.
    /// 
    /// This is a wrapper for Hathora SDK's `ApplicationWithDeployment` model.
    /// We'll eventually replace this with a [Serializable] revamp of the model.
    /// </summary>
    [Serializable]
    public class ApplicationAuthConfigurationWrapper
    {
        // [SerializeField, JsonProperty("_google")] // TODO
        // private ApplicationAuthConfigurationGoogleWrapper _googleWrapper;
        // public ApplicationAuthConfigurationGoogle Google 
        // { 
        //     get => _google; 
        //     set => _google = value;
        // }
        
        
        // /// <summary>"Construct a type with a set of properties K of type T" --from SDK</summary>
        // [SerializeField, JsonProperty("nickname")] // TODO
        // private string _nicknameWrapper;
        //
        // /// <summary>"Construct a type with a set of properties K of type T" --from SDK</summary>
        // public System.Object Nickname // TODO: What's expected in this object, if not a string? 
        // { 
        //     get => _nicknameWrapper; 
        //     set => _nicknameWrapper = value;
        // }
        
        
        // /// <summary>"Construct a type with a set of properties K of type T" --from SDK</summary>
        // [SerializeField, JsonProperty("anonymous")]
        // private System.Object _anonymous;
        //
        // /// <summary>"Construct a type with a set of properties K of type T" --from SDK</summary>
        // public System.Object Anonymous 
        // { 
        //     get => _anonymous; 
        //     set => _anonymous = value;
        // }
        

        public ApplicationAuthConfigurationWrapper(ApplicationAuthConfiguration _appAuthConfig)
        {
            if (_appAuthConfig == null)
                return;

            // this.Google = _appAuthConfig.Google; // TODO
            // this.Nickname = _appAuthConfig.Nickname; // TODO
            // this.Anonymous = _appAuthConfig.Anonymous; // TODO
        }

        public ApplicationAuthConfiguration ToApplicationAuthConfigurationType()
        {
            ApplicationAuthConfiguration appAuthConfig = null;
            try
            {
                appAuthConfig = new()
                {
                    // Optional >>
                    // Google = this.Google, // TODO
                    // Nickname = this.Nickname, // TODO
                    // Anonymous = this.Anonymous, // TODO
                };
            }
            catch (Exception e)
            {
                UnityEngine.Debug.LogError($"Error: {e}");
                throw;
            }

            return appAuthConfig;
        }
    }
}
