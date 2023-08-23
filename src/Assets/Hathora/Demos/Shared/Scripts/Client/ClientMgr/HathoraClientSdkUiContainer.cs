// Created by dylan@hathora.dev

using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

namespace Hathora.Demos.Shared.Scripts.Client.ClientMgr
{
    /// <summary>
    /// Contains containers for UI elements for HathoraClientMgrUiBase.
    /// The purpose of this is to be able to set this in a parent GameObj so we
    /// don't have to set it each time for every demo.
    /// </summary>
    public class HathoraClientSdkUiContainer : MonoBehaviour
    {
        #region Serialized Fields
        [FormerlySerializedAs("InvalidConfigTemplatePnl")]
        [Header("Help")]
        [SerializeField]
        private GameObject invalidConfigTemplatePnl;
        public GameObject InvalidConfigTemplatePnl => invalidConfigTemplatePnl;
        
        [FormerlySerializedAs("InvalidConfigPnl")]
        [SerializeField]
        private GameObject invalidConfigPnl;
        public GameObject InvalidConfigPnl => invalidConfigPnl;
        
        [Header("Auth")]
        [SerializeField]
        private Button authBtn;
        public Button AuthBtn => authBtn;
        
        [SerializeField]
        private TextMeshProUGUI authTxt;
        public TextMeshProUGUI AuthTxt => authTxt;

        [Header("Lobby (Before Exists)")]
        [SerializeField]
        private Button createLobbyBtn;
        public Button CreateLobbyBtn => createLobbyBtn;
        
        [SerializeField]
        private Button getLobbyInfoBtn;
        public Button GetLobbyInfoBtn => getLobbyInfoBtn;
        
        [SerializeField]
        private TMP_InputField getLobbyInfoInput;
        public TMP_InputField GetLobbyInfoInput => getLobbyInfoInput;
        
        [Header("Lobby (After Exists)")]
        [SerializeField]
        private TextMeshProUGUI lobbyRoomIdTxt;
        public TextMeshProUGUI LobbyRoomIdTxt => lobbyRoomIdTxt;
        
        [SerializeField]
        private Button viewLobbiesBtn;
        public Button ViewLobbiesBtn => viewLobbiesBtn;
        
        [SerializeField]
        private Button copyLobbyRoomIdBtn;
        public Button CopyLobbyRoomIdBtn => copyLobbyRoomIdBtn;
        
        [SerializeField]
        private TextMeshProUGUI copiedRoomIdFadeTxt;
        public TextMeshProUGUI CopiedRoomIdFadeTxt => copiedRoomIdFadeTxt;
        
        [SerializeField]
        private TextMeshProUGUI createOrGetLobbyInfoErrTxt;
        public TextMeshProUGUI CreateOrGetLobbyInfoErrTxt => createOrGetLobbyInfoErrTxt;
        
        [SerializeField]
        private TextMeshProUGUI viewLobbiesSeeLogsFadeTxt;
        public TextMeshProUGUI ViewLobbiesSeeLogsFadeTxt => viewLobbiesSeeLogsFadeTxt;
        
        [Header("Room (Get Server/Connection Info)")]
        [SerializeField]
        private Button getServerInfoBtn;
        public Button GetServerInfoBtn => getServerInfoBtn;
        
        [SerializeField]
        private TextMeshProUGUI getServerInfoTxt;
        public TextMeshProUGUI GetServerInfoTxt => getServerInfoTxt;
        
        [SerializeField]
        private Button copyServerInfoBtn;
        public Button CopyServerInfoBtn => copyServerInfoBtn;
        
        [SerializeField]
        private TextMeshProUGUI copiedServerInfoFadeTxt;
        public TextMeshProUGUI CopiedServerInfoFadeTxt => copiedServerInfoFadeTxt;
        
        [SerializeField]
        private TextMeshProUGUI getServerInfoErrTxt;
        public TextMeshProUGUI GetServerInfoErrTxt => getServerInfoErrTxt;
        
        [Header("NetCode Transport: Join Lobby [as Client]")]
        [SerializeField]
        private Button joinLobbyAsClientBtn;
        public Button JoinLobbyAsClientBtn => joinLobbyAsClientBtn;
        
        [SerializeField]
        private TextMeshProUGUI joiningLobbyStatusTxt;
        public TextMeshProUGUI JoiningLobbyStatusTxt => joiningLobbyStatusTxt;
        
        [SerializeField, Tooltip("This will show while you still see the Join button returned")]
        private TextMeshProUGUI joiningLobbyStatusErrTxt;
        public TextMeshProUGUI JoiningLobbyStatusErrTxt => joiningLobbyStatusErrTxt;
        #endregion // Serialized Fields
    }
}
