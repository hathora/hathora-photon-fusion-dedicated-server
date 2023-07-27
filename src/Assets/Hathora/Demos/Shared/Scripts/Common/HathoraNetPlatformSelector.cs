// Created by dylan@hathora.dev

using System;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Hathora.Demos.Shared.Scripts.Common
{
    /// <summary>
    /// "Select a NetCode Platform": Fishnet | Mirror | NGO
    /// </summary>
    public class HathoraNetPlatformSelector : MonoBehaviour
    {
        [Serializable]
        public enum NetPlatform
        {
            FishNet,
            Mirror,
            NGO, // TODO
        }
        
        public void OnFishnetBtnClick() => 
            OnPlatformSelectBtnClick(NetPlatform.FishNet);
        
        public void OnMirrorBtnClick() => 
            OnPlatformSelectBtnClick(NetPlatform.Mirror);
        
        public void OnNGOBtnClick() =>
            OnPlatformSelectBtnClick(NetPlatform.NGO);

        private void OnPlatformSelectBtnClick(NetPlatform _platform)
        {
            const string logPrefix = "[HathoraNetPlatformSelector] OnPlatformSelectBtnClick:";
            
            switch (_platform)
            {
                case NetPlatform.FishNet:
                    Debug.Log($"{logPrefix} FishNet");
                    _ = LoadSceneOnceFromArgAsync("HathoraDemoScene-FishNet");
                    break;

                case NetPlatform.Mirror:
                    Debug.Log($"{logPrefix} Mirror");
                    _ = LoadSceneOnceFromArgAsync("HathoraDemoScene-Mirror");
                    break;

                case NetPlatform.NGO:
                    Debug.Log($"{logPrefix} NGO");
                    // loadSceneAsync("HathoraDemoScene-UnityNGO");
                    throw new NotImplementedException("TODO");
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(_platform), _platform, null);
            }
        }
        
        /// <summary>
        /// After using this once, you won't be able to do it again
        /// (to prevent multiple arg handling stack overflows).
        /// 
        /// Mostly used for args or initial scene selection.
        /// </summary>
        /// <param name="_sceneName">CLI Arg passed from `-scene {sceneName}`.</param>
        public static async Task LoadSceneOnceFromArgAsync(string _sceneName)
        {
            Debug.Log("[HathoraNetPlatformSelector.LoadSceneOnceFromArgAsync] " +
                $"sceneName: {_sceneName}");

            if (HathoraArgHandlerBase.SceneArgConsumed)
            {
                Debug.LogWarning("[HathoraNetPlatformSelector.LoadSceneAsync] " +
                    "LoadSceneOnceAsync already consumed! Aborting.");
                return;
            }

            HathoraArgHandlerBase.SceneArgConsumed = true;
            AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(
                _sceneName, 
                LoadSceneMode.Single);

            while (!asyncLoad.isDone)
                await Task.Yield();
        }
    }
}
