// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Client;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Client;
using Hathora.Core.Scripts.Runtime.Client.Config;
using Hathora.Core.Scripts.Runtime.Client.Models;
using Hathora.Core.Scripts.Runtime.Common.Extensions;
using Hathora.Demos.Shared.Scripts.Client.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hathora.Demos.Shared.Scripts.Client.ClientMgr
{
    /// <summary>
    /// High-level Hathora Client API wrapper manager (Singleton).
    /// - Entry point to call Hathora SDK: Auth, lobby, rooms, etc.
    /// - Entry point for HathoraClientConfig and SDK Config, such as AppId.
    /// - Inits all Client API wrappers with Config + AppId.
    /// - Validates the minimum req's to use the SDK/API.
    /// - Caches API results to Session to reference again later.
    /// - Performs before:after common tasks after API calls. 
    /// - Handles any nuances of the SDK, exposing only what we need.
    /// - Wraps async calls in try/catch so we may gracefully update UI on errs.
    /// - Spawns BEFORE the player, or even connected to the network.
    /// - To add API scripts: Add to the `ClientApis` base serialized field.
    /// </summary>
    public abstract class HathoraClientMgrBase : MonoBehaviour
    {
        #region Serialized Fields
        [FormerlySerializedAs("HathoraClientConfig")]
        [Header("(!) Get from Hathora dir; see hover tooltip")]
        [SerializeField, Tooltip("AppId should parity HathoraServerConfig (see top menu Hathora/Configuration")]
        private HathoraClientConfig hathoraClientConfig;
        protected HathoraClientConfig HathoraClientConfig => hathoraClientConfig;

        [FormerlySerializedAs("HathoraClientSession")]
        [Header("Session, APIs")]
        [SerializeField]
        private HathoraClientSession hathoraClientSession;
        public HathoraClientSession HathoraClientSession => hathoraClientSession;
        
        [FormerlySerializedAs("ClientApis")]
        [SerializeField]
        private ClientApiContainer clientApis;
        protected ClientApiContainer ClientApis => clientApis;
        #endregion // Serialized Fields

        private bool hasUi => netClientMgrUiBase != null;

        
        // public static Hathora{X}Client Singleton { get; private set; } // TODO: Implement me in child
        
        /// <summary>Updates this on state changes</summary>
        protected bool IsConnecting { get; set; }
        
        private HathoraNetClientMgrUiBase netClientMgrUiBase { get; set; }

        
        #region Init
        private void Awake() => OnAwake();
        private void Start() => OnStart();

        /// <summary>setSingleton()</summary>
        protected abstract void OnAwake();

        /// <summary>Override OnStart and call this before anything.</summary>
        /// <param name="_netClientMgrUiBase"></param>
        protected virtual void InitOnStart(HathoraNetClientMgrUiBase _netClientMgrUiBase)
        {
            netClientMgrUiBase = _netClientMgrUiBase;
        }

        protected virtual void OnStart()
        {
            validateReqs();
            initApis(_hathoraSdkConfig: null); // Base will create this
        }
        
        /// <summary>
        /// Init all Client API wrappers. Uses serialized HathoraClientConfig
        /// </summary>
        /// <param name="_hathoraSdkConfig">We'll automatically create this, if empty</param>
        private void initApis(Configuration _hathoraSdkConfig = null)
        {
            if (clientApis.ClientAuthApi != null)
                clientApis.ClientAuthApi.Init(hathoraClientConfig, _hathoraSdkConfig);
            
            if (clientApis.ClientLobbyApi != null)
                clientApis.ClientLobbyApi.Init(hathoraClientConfig, _hathoraSdkConfig);

            if (clientApis.ClientRoomApi != null)
                clientApis.ClientRoomApi.Init(hathoraClientConfig, _hathoraSdkConfig);
        }

        public virtual void validateReqs()
        {
            // Are we using any Client Config at all?
            bool hasConfig = hathoraClientConfig != null;
            bool hasAppId = hathoraClientConfig.HasAppId;
            bool hasNoAppIdButHasUiInstance = !hasAppId && hasUi;
            
            if (!hasConfig || hasNoAppIdButHasUiInstance)
                netClientMgrUiBase.SetInvalidConfig(hathoraClientConfig);
        }

        // // TODO: implement me in child class:
        // protected virtual void setSingleton()
        // {
        //     if (Singleton != null)
        //     {
        //         Debug.LogError("[HathoraClientBase]**ERR @ setSingleton: Destroying dupe");
        //         Destroy(gameObject);
        //         return;
        //     }
        //     
        //     Singleton = this;
        // }
        #endregion // Init
        
        
        #region Interactions from UI
        
        
        #region Interactions from UI -> Required Overrides
        public abstract Task<bool> ConnectAsClient();
        public abstract Task StartServer();
        public abstract Task StartClient();
        public abstract Task StartHost();
        public abstract Task StopHost();
        public abstract Task StopServer();
        public abstract Task StopClient(); 
        #endregion // Interactions from UI -> Required Overrides
       
        
        #region Interactions from UI -> Optional overrides
        /// <summary>If !success, call OnConnectFailed().</summary>
        /// <returns>isValid</returns>
        protected virtual bool ValidateServerConfigConnectionInfo()
        {
            // Validate host:port connection info
            if (!hathoraClientSession.CheckIsValidServerConnectionInfo())
            {
                OnConnectFailed("Invalid ServerConnectionInfo");
                return false; // !success
            }
            
            return true; // success
        }

        /// <summary>Sets `IsConnecting` + logs ip:port (transport).</summary>
        /// <param name="_transportName"></param>
        protected virtual void SetConnectingState(string _transportName)
        {
            IsConnecting = true;

            Debug.Log("[HathoraClientBase.SetConnectingState] Connecting to: " + 
                $"{hathoraClientSession.GetServerInfoIpPort()} via " +
                $"NetworkManager.{_transportName} transport");
        }
        #endregion // Interactions from UI -> Optional overrides

        
        /// <summary>
        /// Auths anonymously => Creates new hathoraClientSession.
        /// </summary>
        public async Task<AuthResult> AuthLoginAsync(CancellationToken _cancelToken = default)
        {
            AuthResult result;
            try
            {
                result = await clientApis.ClientAuthApi.ClientAuthAsync(_cancelToken);
            }
            catch
            {
                OnAuthLoginComplete(_isSuccess:false);
                return null;
            }
           
            hathoraClientSession.InitNetSession(result.PlayerAuthToken);
            OnAuthLoginComplete(result.IsSuccess);

            return result;
        }

        /// <summary>
        /// Creates lobby => caches Lobby info @ hathoraClientSession
        /// </summary>
        /// <param name="_region"></param>
        /// <param name="_visibility"></param>
        /// <param name="_initConfigJsonStr"></param>
        /// <param name="roomId">Leaving empty creates a randomly-generated short Id (recommended)</param>
        /// <param name="_cancelToken"></param>
        public async Task<Lobby> CreateLobbyAsync(
            Region _region,
            CreateLobbyRequest.VisibilityEnum _visibility = CreateLobbyRequest.VisibilityEnum.Public,
            string _initConfigJsonStr = "{}",
            string roomId = null,
            CancellationToken _cancelToken = default)
        {
            Lobby lobby;
            try
            {
                lobby = await clientApis.ClientLobbyApi.ClientCreateLobbyAsync(
                    hathoraClientSession.PlayerAuthToken,
                    _visibility,
                    _region,
                    _initConfigJsonStr,
                    roomId,
                    _cancelToken);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                OnCreateOrJoinLobbyCompleteAsync(null);
                return null;
            }
            
            hathoraClientSession.Lobby = lobby;
            OnCreateOrJoinLobbyCompleteAsync(lobby);

            return lobby;
        }

        /// <summary>
        /// Gets lobby info, if you arleady know the roomId.
        /// (!) Creating a lobby will automatically return the lobbyInfo (along with the roomId).
        /// </summary>
        public async Task<Lobby> GetLobbyInfoAsync(
            string _roomId, 
            CancellationToken _cancelToken = default)
        {
            Lobby lobby;
            try
            {
                lobby = await clientApis.ClientLobbyApi.ClientGetLobbyInfoAsync(
                    _roomId,
                    _cancelToken);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                OnCreateOrJoinLobbyCompleteAsync(null);
                return null;
            }

            hathoraClientSession.Lobby = lobby;
            OnCreateOrJoinLobbyCompleteAsync(lobby);

            return lobby;
        }

        /// <summary>Public lobbies only.</summary>
        /// <param name="_region">
        /// TODO (to confirm): null region returns *all* region lobbies?
        /// </param>
        /// <param name="_cancelToken"></param>
        public async Task<List<Lobby>> ViewPublicLobbies(
            Region? _region = null,
            CancellationToken _cancelToken = default)
        {
            List<Lobby> lobbies;
            try
            {
                lobbies = await clientApis.ClientLobbyApi.ClientListPublicLobbiesAsync(
                    _region,
                    _cancelToken);
            }
            catch (Exception e)
            {
                Debug.LogWarning(e.Message);
                throw new NotImplementedException("TODO: Get lobbies err handling UI");
            }

            hathoraClientSession.Lobbies = lobbies;
            OnViewPublicLobbiesComplete(lobbies);

            return lobbies;
        }
        
        /// <summary>
        /// Gets ip:port (+transport type) info so we can connect the Client via the selected transport (eg: Fishnet).
        /// AKA "GetServerInfo" (from UI). Polls until status is `Active`: May take a bit!
        /// </summary>
        public async Task<ConnectionInfoV2> GetActiveConnectionInfo(
            string _roomId, 
            CancellationToken _cancelToken = default)
        {
            ConnectionInfoV2 connectionInfo;
            try
            {
                connectionInfo = await clientApis.ClientRoomApi.ClientGetConnectionInfoAsync(
                    _roomId, 
                    _cancelToken: _cancelToken);
            }
            catch (Exception e)
            {
                Debug.LogError($"[HathoraClientBase] OnCreateOrJoinLobbyCompleteAsync: {e.Message}");
                if (hasUi)
                    netClientMgrUiBase.OnGetServerInfoFail();
                return null; // fail
            }
            
            hathoraClientSession.ServerConnectionInfo = connectionInfo;
            OnGetActiveConnectionInfoComplete(connectionInfo);

            return connectionInfo;
        }
        #endregion // Interactions from UI
        
        
        #region Callbacks
        protected virtual void OnConnectFailed(string _friendlyReason)
        {
            IsConnecting = false;
            
            if (hasUi)
                netClientMgrUiBase.OnJoinLobbyFailed(_friendlyReason);
        }
        
        protected virtual void OnConnectSuccess()
        {
            IsConnecting = false;
            
            if (hasUi)
                netClientMgrUiBase.OnJoinLobbyConnectSuccess();
        }
        
        protected virtual void OnGetActiveConnectionInfoFail()
        {
            if (hasUi)
                netClientMgrUiBase.OnGetServerInfoFail();
        }
        
        /// <summary>AKA OnGetServerInfoSuccess - mostly UI</summary>
        protected virtual void OnGetActiveConnectionInfoComplete(ConnectionInfoV2 _connectionInfo)
        {
            if (netClientMgrUiBase == null)
                return;

            if (string.IsNullOrEmpty(_connectionInfo?.ExposedPort?.Host))
            {
                netClientMgrUiBase.OnGetServerInfoFail();
                return;
            }
            
            netClientMgrUiBase.OnGetServerInfoSuccess(_connectionInfo);
        }
        
        protected virtual void OnAuthLoginComplete(bool _isSuccess)
        {
            if (netClientMgrUiBase == null)
                return;

            if (!_isSuccess)
            {
                netClientMgrUiBase.OnAuthFailed();
                return;
            }

            netClientMgrUiBase.OnAuthedLoggedIn();
        }

        protected virtual void OnViewPublicLobbiesComplete(List<Lobby> _lobbies)
        {
            int numLobbiesFound = _lobbies?.Count ?? 0;
            Debug.Log("[NetHathoraPlayer] OnViewPublicLobbiesComplete: " +
                $"# Lobbies found: {numLobbiesFound}");

            // UI >>
            if (netClientMgrUiBase == null)
                return;

            if (_lobbies == null || numLobbiesFound == 0)
                throw new NotImplementedException("TODO: !Lobbies handling");

            List<Lobby> sortedLobbies = _lobbies.OrderBy(lobby => lobby.CreatedAt).ToList();
            netClientMgrUiBase.OnViewLobbies(sortedLobbies);
        }
        
        /// <summary>
        /// On success, most users will want to call GetActiveConnectionInfo().
        /// </summary>
        /// <param name="_lobby"></param>
        protected virtual void OnCreateOrJoinLobbyCompleteAsync(Lobby _lobby)
        {
            if (netClientMgrUiBase == null)
                return;

                // UI >>
            if (string.IsNullOrEmpty(_lobby?.RoomId))
            {
                netClientMgrUiBase.OnCreatedOrJoinedLobbyFail();
                return;
            }
            
            // Success >> We may not have a UI
            if (netClientMgrUiBase == null)
                return;

            string friendlyRegion = _lobby.Region.ToString().SplitPascalCase();
            netClientMgrUiBase.OnCreatedOrJoinedLobby(
                _lobby.RoomId, 
                friendlyRegion);
        }
        #endregion // Callbacks
    }
}
