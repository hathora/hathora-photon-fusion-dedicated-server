// Created by dylan@hathora.dev

using UnityEngine;

namespace Hathora.Demos.Shared.Scripts.Common
{
    public class DestroyIfUnityEditor : MonoBehaviour
    {
        private void Awake()
        {   
#if UNITY_EDITOR
            Destroy(gameObject);
#endif
        }
    }
}
