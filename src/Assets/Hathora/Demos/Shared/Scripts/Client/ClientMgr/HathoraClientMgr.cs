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
using Hathora.Demos.Shared.Scripts.Client.Models;
using UnityEngine;
using UnityEngine.Assertions;
using UnityEngine.Serialization;

namespace Hathora.Demos.Shared.Scripts.Client.ClientMgr
{
    /// <summary>
    /// - This is the entry point to call Hathora SDK: Auth, lobby, rooms, etc.
    /// - Opposed to the SDK itself, this gracefully wraps around it with callbacks + events + session.
    /// - Every SDK call caches the result in `hathoraClientSession`.
    /// - If you have a UI script, subscribe to the events above to handle them ^
    /// - To add API scripts: Add to the `ClientApis` serialized field.
    /// - Ready to be inheritted with protected virtual members, should you want to!
    /// 
    /// - Available Events to subscribe to:
    ///     * OnAuthLoginDoneEvent
    ///     * OnGetActiveConnectionInfoFailEvent
    ///     * OnGetActiveConnectionInfoDoneEvent
    ///     * OnGetActivePublicLobbiesDoneEvent
    /// </summary>
    public class HathoraClientMgr : MonoBehaviour
    {
        public static HathoraClientMgr Singleton { get; private set; }
        
        
        #region Serialized Fields
        [Header("(!) Get from Hathora dir; see hover tooltip")]
        [SerializeField, Tooltip("AppId should parity HathoraServerConfig (see top menu Hathora/Configuration")]
        private HathoraClientConfig hathoraClientConfig;
        public HathoraClientConfig HathoraClientConfig => hathoraClientConfig;

        [FormerlySerializedAs("HathoraClientSession")]
        [Header("Session, APIs")]
        [SerializeField, Tooltip("Whenever we use the Hathora Client SDK, we'll cache it in this Session.")]
        private HathoraClientSession hathoraClientSession;
        
        /// <summary>
        /// Whenever we use the Hathora Client SDK, we'll cache it in this Session.
        /// - Reset anew when authenticating.
        /// </summary>
        public HathoraClientSession HathoraClientSession => hathoraClientSession;
        
        [FormerlySerializedAs("ClientApis")]
        [SerializeField]
        private ClientApiContainer clientApis;
        
        /// <summary>Direct access to the APIs, if the mgr !has what you want</summary>
        protected ClientApiContainer ClientApis => clientApis;
        #endregion // Serialized Fields


        #region Public Events
        /// <summary>Event triggers when auth is done (check isSuccess)</summary>
        /// <returns>isSuccess</returns>
        public static event Action<bool> OnAuthLoginDoneEvent;
        
        /// <summary>lobby</summary>
        public static event Action<Lobby> OnCreateLobbyDoneEvent;
        
        /// <summary>connectionInfo:ConnectionInfoV2</summary>
        public static event Action<ConnectionInfoV2> OnGetActiveConnectionInfoDoneEvent;
        
        /// <summary>lobbies:List (sorted by Newest @ top)</summary>
        public static event Action<List<Lobby>> OnGetActivePublicLobbiesDoneEvent;
        #endregion // Public Events
        
        
        #region Init
        protected virtual void Awake() =>
            setSingleton();
        
        protected virtual void Start() =>
            initApis(_hathoraSdkConfig: null); // Base will create this

        /// <summary>
        /// You want other classes to easily be able to access your ClientMgr
        /// </summary>
        /// <summary>
        /// Set a singleton instance - we'll only ever have one serverMgr.
        /// Children probably want to override this and, additionally, add a Child singleton
        /// </summary>
        private void setSingleton()
        {
            if (Singleton != null)
            {
                Debug.LogError($"[{nameof(HathoraClientMgr)}.{nameof(setSingleton)}] " +
                    "Error: Destroying dupe");
                
                Destroy(gameObject);
                return;
            }
            
            Singleton = this;
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
        #endregion // Init
        
        
        #region Interactions from UI
        /// <summary>
        /// Auths anonymously => Creates new hathoraClientSession.
        /// - Resets cache completely on done (not necessarily success)
        /// - Sets `PlayerAuthToken` cache
        /// - Callback @ virtual OnAuthLoginComplete(isSuccess)
        /// </summary>
        public async Task<AuthResult> AuthLoginAsync(CancellationToken _cancelToken = default)
        {
            AuthResult authResult = await clientApis.ClientAuthApi.ClientAuthAsync(_cancelToken);

            hathoraClientSession.InitNetSession(authResult.PlayerAuthToken);
            OnAuthLoginDone(authResult.IsSuccess);

            return authResult;
        }

        /// <summary>
        /// Creates lobby => caches Lobby info @ hathoraClientSession.
        /// - Sets `Lobby` cache on done (not necessarily success)
        /// - Callback @ virtual OnCreateLobbyCompleteAsync(lobby)
        /// - Asserts IsAuthed
        /// </summary>
        /// <param name="_region"></param>
        /// <param name="_visibility"></param>
        /// <param name="_initConfigJsonStr"></param>
        /// <param name="_roomId"></param>
        /// <param name="_cancelToken"></param>
        public async Task<Lobby> CreateLobbyAsync(
            Region _region,
            CreateLobbyRequest.VisibilityEnum _visibility = CreateLobbyRequest.VisibilityEnum.Public,
            string _initConfigJsonStr = "{}",
            string _roomId = null,
            CancellationToken _cancelToken = default)
        {
            Assert.IsTrue(hathoraClientSession.IsAuthed, 
                "expected hathoraClientSession.IsAuthed");

            Lobby lobby = await clientApis.ClientLobbyApi.ClientCreateLobbyAsync(
                hathoraClientSession.PlayerAuthToken,
                _visibility,
                _region,
                _initConfigJsonStr,
                _roomId,
                _cancelToken);
            
            hathoraClientSession.Lobby = lobby;
            OnCreateLobbyDoneAsync(lobby);

            return lobby;
        }

        /// <summary>
        /// Gets lobby info by roomId.
        /// - Asserts IsAuthed
        /// - Sets `Lobby` cache on done (not necessarily success)
        /// - Callback @ virtual OnCreateLobbyCompleteAsync(lobby)
        /// </summary>
        public async Task<Lobby> GetLobbyInfoAsync(
            string _roomId, 
            CancellationToken _cancelToken = default)
        {
            Assert.IsTrue(hathoraClientSession.IsAuthed, 
                "expected hathoraClientSession.IsAuthed");

            Lobby lobby = await clientApis.ClientLobbyApi.ClientGetLobbyInfoAsync(
                _roomId,
                _cancelToken);
        
            hathoraClientSession.Lobby = lobby;
            OnCreateLobbyDoneAsync(lobby);

            return lobby;
        }

        /// <summary>
        /// Gets Public+active lobbies.
        /// - Asserts IsAuthed
        /// - Sets `Lobbies` cache on done (not necessarily success)
        /// - Callback @ virtual OnViewPublicLobbiesComplete(lobbies)
        /// </summary>
        /// <param name="_region">null returns all regions</param>
        /// <param name="_cancelToken"></param>
        public async Task<List<Lobby>> GetActivePublicLobbiesAsync(
            Region? _region = null,
            CancellationToken _cancelToken = default)
        {
            Assert.IsTrue(hathoraClientSession.IsAuthed, 
                "expected hathoraClientSession.IsAuthed");
            
            List<Lobby> lobbies = await clientApis.ClientLobbyApi.ClientListPublicLobbiesAsync(
                _region,
                _cancelToken);

            hathoraClientSession.Lobbies = lobbies;
            OnGetActivePublicLobbiesDone(lobbies);

            return lobbies;
        }
        
        /// <summary>
        /// Gets ip:port (+transport type) info so we can connect the Client
        /// via the selected transport (eg: Fishnet).
        /// - Asserts IsAuthed
        /// - Polls until status is `Active`: May take a bit!
        /// - Sets `ServerConnectionInfo` cache on done (not necessarily success)
        /// - Callback @ virtual OnGetActiveConnectionInfoComplete(connectionInfo)
        /// </summary>
        public async Task<ConnectionInfoV2> GetActiveConnectionInfo(
            string _roomId, 
            CancellationToken _cancelToken = default)
        {
            Assert.IsTrue(hathoraClientSession.IsAuthed, 
                "expected hathoraClientSession.IsAuthed");
            
            ConnectionInfoV2 connectionInfo = null;
            
            try
            {
                connectionInfo = await clientApis.ClientRoomApi.ClientGetConnectionInfoAsync(
                    _roomId,
                    _cancelToken: _cancelToken);
            }
            catch (Exception e)
            {
                Debug.LogError(
                    $"[{nameof(HathoraClientMgr)}.{nameof(GetActiveConnectionInfo)}] " +
                    $"ClientGetConnectionInfoAsync => Error: {e}");

                throw;
            }
            finally
            {
                hathoraClientSession.ServerConnectionInfo = connectionInfo;
                OnGetActiveConnectionInfoDone(connectionInfo);
            }
            
            return connectionInfo;
        }
        #endregion // Interactions from UI
        
        
        #region Virtual callbacks w/Events
        /// <summary>AuthLogin() callback.</summary>
        /// <param name="_isSuccess"></param>
        protected virtual void OnAuthLoginDone(bool _isSuccess) =>
            OnAuthLoginDoneEvent?.Invoke(_isSuccess);
        
        /// <summary>
        /// GetActiveConnectionInfo() done callback (not necessarily successful).
        /// </summary>
        protected virtual void OnGetActiveConnectionInfoDone(ConnectionInfoV2 _connectionInfo) =>
            OnGetActiveConnectionInfoDoneEvent?.Invoke(_connectionInfo);

        /// <summary>GetActivePublicLobbies() callback.</summary>
        /// <param name="_lobbies"></param>
        protected virtual void OnGetActivePublicLobbiesDone(List<Lobby> _lobbies)
        {
            // Sort lobbies by create date -> Pass to UI
            List<Lobby> sortedFromNewestToOldest = _lobbies.OrderByDescending(lobby => 
                lobby.CreatedAt).ToList();
            
            OnGetActivePublicLobbiesDoneEvent?.Invoke(sortedFromNewestToOldest);
        }
        
        /// <summary>
        /// On success, most users will want to call GetActiveConnectionInfo().
        /// </summary>
        /// <param name="_lobby"></param>
        protected virtual void OnCreateLobbyDoneAsync(Lobby _lobby) => 
            OnCreateLobbyDoneEvent?.Invoke(_lobby);
        #endregion // Virtual callbacks w/Events
        
        
        #region Utils
        /// <summary>
        /// We just need HathoraClientConfig serialized to a
        /// scene NetworkManager, with `AppId` set.
        /// - Does not throw, so you can properly handle UI on err.
        /// </summary>
        /// <returns>isValid</returns>
        public bool CheckIsValidToAuth() =>
            hathoraClientConfig != null && hathoraClientConfig.HasAppId;
        #endregion // Utils
    }
}
