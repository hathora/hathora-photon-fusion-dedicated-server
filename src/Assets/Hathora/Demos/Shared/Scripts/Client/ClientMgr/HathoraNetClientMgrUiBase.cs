// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Client;
using Hathora.Core.Scripts.Runtime.Client.Config;
using TMPro;
using UnityEngine;

namespace Hathora.Demos.Shared.Scripts.Client.ClientMgr
{
    /// <summary>
    /// Handles the non-Player UI so we can keep the logic separate.
    /// Generally, this is going to be pre-connection UI such as create/join lobbies.
    /// UI OnEvent entry points from Buttons start here.
    /// </summary>
    public abstract class HathoraNetClientMgrUiBase : MonoBehaviour
    {
        #region Serialized Fields
        [SerializeField]
        private HathoraNetClientMgrUiBaseContainer ui;

        public HathoraNetClientMgrUiBaseContainer Ui
        {
            get => ui;
            set => ui = value;
        }
        #endregion // Serialized Fields

        
        // ###################################################################
        // public static HathoraHathoraNetUiBaseBase Singleton { get; protected set; }
        // ###################################################################

        private const float FADE_TXT_DISPLAY_DURATION_SECS = 0.5f;
        private const string HATHORA_VIOLET_COLOR_HEX = "#EEDDFF";
        private static string headerBoldColorBegin => $"<b><color={HATHORA_VIOLET_COLOR_HEX}>";
        private const string headerBoldColorEnd = "</color></b>";

        private HathoraClientMgrBase hathoraClientMgrBase;
        
        protected static HathoraClientSession HathoraClientSession => 
            HathoraClientSession.Singleton;


        #region Init
        private void Awake() => 
            OnAwake();
        
        private void Start() => 
            OnStart();

        protected virtual void OnAwake() =>
            SetSingleton();

        /// <summary>Override + Call InitOnStart</summary>
        protected virtual void OnStart()
        {
            // InitOnStart(hathoraClientBase);
        }

        /// <summary>Call me @ OnStart</summary>
        protected void InitOnStart(HathoraClientMgrBase _hathoraClientMgrBase)
        {
            if (_hathoraClientMgrBase == null)
                throw new ArgumentNullException(nameof(_hathoraClientMgrBase));
            
            this.hathoraClientMgrBase = _hathoraClientMgrBase;
        }
        
        /// <summary>Override this and set your singleton instance</summary>
        protected abstract void SetSingleton();
        #endregion // Init
        
        
        #region UI Interactions
        public virtual void OnStartServerBtnClick() { }
        public virtual void OnStartClientBtnClick() { }
        public virtual void OnStartHostBtnClick() { }
        public virtual void OnStopServerBtnClick() { }
        public virtual void OnStopClientBtnClick() { }
        public virtual void OnStopHostBtnClick() { }

        public void OnAuthLoginBtnClick()
        {
            hathoraClientMgrBase.validateReqs();
                
            SetShowAuthTxt("<color=yellow>Logging in...</color>");
            _ = hathoraClientMgrBase.AuthLoginAsync(); // !await
        }

        public void OnCreateLobbyBtnClick()
        {
            SetShowLobbyTxt("<color=yellow>Creating Lobby...</color>");

            // (!) Region Index starts at 1 (not 0) // TODO: Get from UI
            const Region _region = Region.WashingtonDC;
            
            _ = hathoraClientMgrBase.CreateLobbyAsync(_region); // !await // public lobby
        }

        /// <summary>
        /// The player pressed ENTER || unfocused the ServerConnectionInfo input.
        /// </summary>
        public void OnGetLobbyInfoInputEnd()
        {
            ShowGettingLobbyInfoUi();
            string roomIdInputStr = GetLobbyInfoInputStr();
            ui.getLobbyInfoInput.text = "";

            if (string.IsNullOrEmpty(roomIdInputStr))
            {
                OnAuthedLoggedIn();
                return;
            }
            
            _ = hathoraClientMgrBase.GetLobbyInfoAsync(roomIdInputStr); // !await
        }

        /// <summary>
        /// Btn disabled OnClick via inspector: Restore on done
        /// </summary>
        public async void OnViewLobbiesBtnClick()
        {
            ui.viewLobbiesSeeLogsFadeTxt.text = "<color=yellow>Getting Lobbies...</color>";
            _ = ShowFadeTxtThenFadeAsync(ui.viewLobbiesSeeLogsFadeTxt); // !await

            // TODO: Get region from UI // TODO: Confirm null region returns ALL regions?
            Region? region = null;
            
            try
            {
                await hathoraClientMgrBase.ViewPublicLobbies(region);
            }
            catch (Exception e)
            {
                ui.viewLobbiesBtn.interactable = true;
            }
        }
        
        public void OnCopyLobbyRoomIdBtnClick()
        {
            GUIUtility.systemCopyBuffer = HathoraClientSession.RoomId; // Copy to clipboard
            
            // Show + Fade
            _ = ShowFadeTxtThenFadeAsync(ui.copiedRoomIdFadeTxt); // !await
        }

        /// <summary>
        /// We should only call this if we already have the lobby info (ServerConnectionInfo).
        /// </summary>
        public void OnGetServerInfoBtnClick()
        {
            SetServerInfoTxt("<color=yellow>Getting server connection info...</color>");
            
            // The ServerConnectionInfo should already be cached
            _ = hathoraClientMgrBase.GetActiveConnectionInfo(HathoraClientSession.RoomId); // !await
        }
        
        /// <summary>
        /// Copies as "ip:port"
        /// </summary>
        public void OnCopyServerInfoBtnClick()
        {
            string serverInfo = HathoraClientSession.GetServerInfoIpPort(); // "ip:port"
            GUIUtility.systemCopyBuffer = serverInfo; // Copy to clipboard
            
            // Show + Fade
            _ = ShowFadeTxtThenFadeAsync(ui.copiedServerInfoFadeTxt); // !await
        }

        /// <summary>Component OnClick hides joinLobbyAsClientBtn</summary>
        public virtual void OnJoinLobbyAsClientBtnClick()
        {
            Debug.Log("[HathoraNetUiBase] OnJoinLobbyAsClientBtnClick");

            ui.joinLobbyAsClientBtn.gameObject.SetActive(false);
            ui.joiningLobbyStatusErrTxt.gameObject.SetActive(false);
            
            ui.joiningLobbyStatusTxt.text = "<color=yellow>Joining Lobby...</color>";
            ui.joiningLobbyStatusTxt.gameObject.SetActive(true);
        }

        public void OnJoinLobbyConnectSuccess()
        {
            Debug.Log("[HathoraNetUiBase] OnJoinLobbySuccess");
            
            ui.joiningLobbyStatusTxt.text = "<color=green>Joined Lobby</color>";
            // Player stats should be updated via NetHathoraPlayer.OnStartClient
        }

        public void OnJoinLobbyFailed(string _friendlyErr)
        {
            Debug.Log($"[HathoraNetUiBase] OnJoinLobbyFailed: {_friendlyErr}");

            ui.joiningLobbyStatusTxt.gameObject.SetActive(false);
            ui.joinLobbyAsClientBtn.gameObject.SetActive(true);

            if (string.IsNullOrEmpty(_friendlyErr))
                return;
            
            ui.joiningLobbyStatusErrTxt.text = $"<color=orange>{_friendlyErr}</color>";
            ui.joiningLobbyStatusErrTxt.gameObject.SetActive(true);
        }
        #endregion // UI Interactions
        

        #region Dynamic UI
        public void OnAuthedLoggedIn()
        {
            SetShowAuthTxt("<b>Client Logged In</b> (Anonymously)");
            showInitLobbyUi(true);
        }
        
        public void OnAuthFailed()
        {
            SetShowAuthTxt("<color=orange>Login Failed</color>");
        }
        
        /// <summary>
        /// (!) Don't reset roomIdInputStr.text here
        /// </summary>
        public void ShowGettingLobbyInfoUi()
        {
            // Hide all of lobby EXCEPT the room id text
            SetShowLobbyTxt("<color=yellow>Getting Lobby Info...</color>");
            showInitLobbyUi(false);
        }
        
        /// <summary>Shows arbitrary text at the bottom of the screen</summary>
        /// <param name="memoStr"></param>
        public void SetShowDebugMemoTxt(string memoStr)
        {
            ui.debugMemoTxt.text = memoStr;
            ui.debugMemoTxt.gameObject.SetActive(true);
            Debug.Log($"[HathoraNetUiBase] Debug Memo: '{memoStr}'");
        }

        public void SetShowAuthTxt(string authStr)
        {
            ui.authTxt.text = authStr;
            ui.authTxt.gameObject.SetActive(true);
        }
        
        public void SetShowLobbyTxt(string roomId)
        {
            ui.lobbyRoomIdTxt.text = roomId;
            ui.lobbyRoomIdTxt.gameObject.SetActive(true);
        }

        public void SetShowCreateOrJoinLobbyErrTxt(string friendlyErrStr)
        {
            ui.createOrGetLobbyInfoErrTxt.text = friendlyErrStr;
            ui.createOrGetLobbyInfoErrTxt.gameObject.SetActive(true);
        }
        
        public void SetGetServerInfoErrTxt(string friendlyErrStr)
        {
            ui.getServerInfoErrTxt.text = friendlyErrStr;
            ui.getServerInfoErrTxt.gameObject.SetActive(true);
        }
        
        public void SetServerInfoTxt(string serverInfo)
        {
            ui.getServerInfoTxt.text = serverInfo;
            ui.getServerInfoTxt.gameObject.SetActive(true);
        }

        /// <summary>
        /// This also resets interactable
        /// </summary>
        /// <param name="show"></param>
        protected void showInitLobbyUi(bool show)
        {
            ui.createLobbyBtn.interactable = show;
            ui.getLobbyInfoBtn.interactable = show;
            
            ui.createLobbyBtn.gameObject.SetActive(show);
            ui.getLobbyInfoBtn.gameObject.SetActive(show);
            
            // On or off: If this is resetting, we'll hide it. 
            // This also hides the cancel btn
            ui.lobbyRoomIdTxt.gameObject.SetActive(false); // Behind 'Create Lobby' btn
            ui.getLobbyInfoInput.gameObject.SetActive(false);
            ui.copyLobbyRoomIdBtn.gameObject.SetActive(false);
            ui.createOrGetLobbyInfoErrTxt.gameObject.SetActive(false);
            ui.copiedRoomIdFadeTxt.gameObject.SetActive(false);
            ui.viewLobbiesSeeLogsFadeTxt.gameObject.SetActive(false);
        }

        public void OnCreatedOrJoinedLobbyFail()
        {
            showInitLobbyUi(true);
            SetShowCreateOrJoinLobbyErrTxt("<color=orange>Failed to Get Lobby info - see logs</color>");
        }

        public void OnGetServerInfoSuccess(ConnectionInfoV2 connectionInfo)
        {
            Debug.Log(
                $"[HathoraNetUiBase] OnGetServerInfoSuccess: {HathoraClientSession.GetServerInfoIpPort()} " +
                $"({connectionInfo.ExposedPort.TransportType})");
            
            // ####################
            // ServerInfo:
            // 127.0.0.1:7777 (UDP)
            // ####################
            SetServerInfoTxt($"{headerBoldColorBegin}ServerInfo{headerBoldColorEnd}:\n" +
                $"{connectionInfo.ExposedPort.Host}<color=yellow><b>:</b></color>{connectionInfo.ExposedPort.Port}\n" +
                $"(<color=yellow>{connectionInfo.ExposedPort.TransportType}</color>)");
            
            ui.copyServerInfoBtn.gameObject.SetActive(true);
            ui.joinLobbyAsClientBtn.gameObject.SetActive(true);
        }
        
        public void OnGetServerInfoFail()
        {
            ui.getServerInfoBtn.gameObject.SetActive(true);
            SetGetServerInfoErrTxt("<color=orange>Failed to Get Server Info - see logs</color>");
        }
        
        public void OnCreatedOrJoinedLobby(string _roomId, string _friendlyRegionStr)
        {
            // Hide all init lobby UI except the txt + view lobbies
            showInitLobbyUi(false);
            SetShowLobbyTxt($"{headerBoldColorBegin}RoomId{headerBoldColorEnd}:\n{_roomId}\n\n" +
                $"{headerBoldColorBegin}Region{headerBoldColorEnd}: {_friendlyRegionStr}");

            // We can now show the lobbies and ServerConnectionInfo copy btn
            ui.copyLobbyRoomIdBtn.gameObject.SetActive(true);
            ui.viewLobbiesBtn.gameObject.SetActive(true);
            ui.getServerInfoBtn.gameObject.SetActive(true);
        }

        public string GetLobbyInfoInputStr() =>
            ui.getLobbyInfoInput.text.Trim();
        
        public void OnViewLobbies(List<Lobby> lobbies)
        {
            ui.viewLobbiesSeeLogsFadeTxt.text = "See Logs";
            _ = ShowFadeTxtThenFadeAsync(ui.viewLobbiesSeeLogsFadeTxt); // !await
            
            foreach (Lobby lobby in lobbies)
            {
                Debug.Log($"[NetPlayerUI] OnViewLobbies - lobby found: " +
                    $"RoomId={lobby.RoomId}, CreatedAt={lobby.CreatedAt}, CreatedBy={lobby.CreatedBy}");
            }
            
            // TODO: Create a UI view for these servers
            ui.viewLobbiesBtn.interactable = true;
        }
        
        protected async Task ShowFadeTxtThenFadeAsync(TextMeshProUGUI fadeTxt)
        {
            fadeTxt.gameObject.SetActive(true);
            await Task.Delay((int)(FADE_TXT_DISPLAY_DURATION_SECS * 1000));

            float elapsedTime = 0f;
            Color originalColor = fadeTxt.color;

            while (elapsedTime < FADE_TXT_DISPLAY_DURATION_SECS)
            {
                elapsedTime += Time.deltaTime;
                float alpha = Mathf.Lerp(1f, 0f, elapsedTime / FADE_TXT_DISPLAY_DURATION_SECS);
                fadeTxt.color = new Color(originalColor.r, originalColor.g, originalColor.b, alpha);
                await Task.Yield();
            }

            fadeTxt.gameObject.SetActive(false);
            fadeTxt.color = originalColor;
        }
        
        public void SetInvalidConfig(HathoraClientConfig _config)
        {
            if (ui.authBtn != null)
                ui.authBtn.gameObject.SetActive(false); // Prevent UI overlap
            
            // Core issue
            string netComponentPathFriendlyStr = $" HathoraManager (GameObject)'s " +
                $"{nameof(hathoraClientMgrBase)} component";
            
            if (_config == null)
            {
                ui.authBtn.gameObject.SetActive(false);
                ui.InvalidConfigPnl.SetActive(true);

                throw new Exception($"[{nameof(hathoraClientMgrBase)}] !{nameof(HathoraClientConfig)} - " +
                    $"Serialize one at {netComponentPathFriendlyStr}");
            }
            
            if (!_config.HasAppId)
            {
                ui.InvalidConfigPnl.SetActive(true);

                throw new Exception($"[{nameof(hathoraClientMgrBase)}] !AppId - " +
                    $"Set one at {netComponentPathFriendlyStr} (See top menu `Hathora/Configuration` - your " +
                    "ServerConfig's AppId should match your ClientConfig's AppId)");
            }
            
            bool isTemplate = _config.name.Contains(".template");
            if (!isTemplate)
                return;
            
            ui.authBtn.gameObject.SetActive(false);
            ui.InvalidConfigTemplatePnl.SetActive(true);
                
            throw new Exception("[HathoraNetUiBase.SetInvalidConfig] Error: " +
                "Using template Config! Create a new one via top menu `Hathora/Config Finder`");
        }
        #endregion /Dynamic UI
    }
}
