// Created by dylan@hathora.dev

using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Hathora.Demos.Shared.Scripts.Client.ClientMgr
{
    /// <summary>
    /// Only for use with the Hello World UI. Most component events are
    /// currently handled within component Event click logic (unlike the SDK demo).
    /// </summary>
    public class HathoraClientMgrHelloWorldDemoUi : MonoBehaviour
    {
        #region Vars
        [Header("Hello World Demo")]
        [SerializeField, Tooltip("When we connect as a Client, this is " +
             "the optional host:ip optionally passed along")]
        private TMP_InputField clientConnectInputField;
        public TMP_InputField ClientConnectInputField => clientConnectInputField;

        [SerializeField, Tooltip("WebGL builds should hide this")]
        private Button netStartServerBtn;
        #endregion // Vars

        
        private void Awake()
        {
            #if UNITY_WEBGL
            // TODO: Instead of hiding these btns, we may want to just disable it and show a memo beside it
            Debug.Log("[HathoraClientMgrHelloWorldDemoUi.Awake] Hiding " +
                "netStartHostBtn and server btn for WebGL builds - note that clipboard " +
                "btns will !work unless using a custom type made to work with webgl");
            
            netStartServerBtn.gameObject.SetActive(false);
            #endif
        }
    }
}
