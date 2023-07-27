// Created by dylan@hathora.dev

using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Hathora.Demos.Shared.Scripts.Client.ClientMgr
{
    /// <summary>
    /// Contains containers for UI elements for HathoraNetClientMgrUiBase.
    /// The purpose of this is to be able to set this in a parent GameObj so we
    /// don't have to set it each time for every demo.
    /// </summary>
    [Serializable]
    public class HathoraNetClientMgrUiBaseContainer : MonoBehaviour
    {
        #region Serialized Fields
        [FormerlySerializedAs("InvalidConfigPnl")]
        [Header("Help")]
        [SerializeField]
        public GameObject InvalidConfigTemplatePnl;
        
        [SerializeField]
        public GameObject InvalidConfigPnl;
        
        [Header("CLI")]
        [SerializeField]
        public TextMeshProUGUI debugMemoTxt;
        
        [Header("Auth")]
        [SerializeField]
        public Button authBtn;
        [SerializeField]
        public TextMeshProUGUI authTxt;

        [Header("Lobby (Before Exists)")]
        [SerializeField]
        public Button createLobbyBtn;
        [SerializeField]
        public Button getLobbyInfoBtn;
        [SerializeField]
        public TMP_InputField getLobbyInfoInput;

        [Header("Lobby (After Exists)")]
        [SerializeField]
        public TextMeshProUGUI lobbyRoomIdTxt;
        [SerializeField]
        public Button viewLobbiesBtn;
        [SerializeField]
        public Button copyLobbyRoomIdBtn;
        [SerializeField]
        public TextMeshProUGUI copiedRoomIdFadeTxt;
        [SerializeField]
        public TextMeshProUGUI createOrGetLobbyInfoErrTxt;
        [SerializeField]
        public TextMeshProUGUI viewLobbiesSeeLogsFadeTxt;
        
        [Header("Room (Get Server/Connection Info)")]
        [SerializeField]
        public Button getServerInfoBtn;
        [SerializeField]
        public TextMeshProUGUI getServerInfoTxt;
        [SerializeField]
        public Button copyServerInfoBtn;
        [SerializeField]
        public TextMeshProUGUI copiedServerInfoFadeTxt;
        [SerializeField]
        public TextMeshProUGUI getServerInfoErrTxt;
        
        [Header("NetCode Transport: Join Lobby [as Client]")]
        [SerializeField]
        public Button joinLobbyAsClientBtn;
        [SerializeField]
        public TextMeshProUGUI joiningLobbyStatusTxt;
        [SerializeField, Tooltip("This will show while you still see the Join button returned")]
        public TextMeshProUGUI joiningLobbyStatusErrTxt;
        #endregion // Serialized Fields
    }
}
