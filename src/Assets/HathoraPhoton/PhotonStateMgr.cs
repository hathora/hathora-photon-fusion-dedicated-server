// Created by dylan@hathora.dev

using System;
using Fusion;
using Hathora.Demos.Shared.Scripts.Common;
using UnityEngine;
using Assert = UnityEngine.Assertions.Assert;

namespace HathoraPhoton
{
    /// <summary>
    /// Acts as the liason between NetworkManager and HathoraClientMgr.
    /// - This child class tracks FishNet NetworkManager state changes, and:
    ///   * Handles setting the NetworkManager [host|ip]:port.
    ///   * Can talk to Hathora scripts to get cached host:port.
    ///   * Can initialize or stop NetworkManager connections.
    ///   * Tells base to log + trigger OnDone() events other scripts subcribe to.
    /// - Base contains funcs like: StartServer, StartClient, StartHost.
    /// - Base contains events like: OnClientStarted, OnClientStopped.
    /// - Base tracks `ClientState` like: Stopped, Starting, Started.
    /// </summary>
    public class PhotonStateMgr : NetworkMgrStateTracker
    {
        #region vars
        /// <summary>
        /// `New` keyword overrides base Singleton when accessing child directly.
        /// </summary>
        public new static PhotonStateMgr Singleton { get; private set; }
        #endregion // vars

        
        #region Init
        /// <summary>Set Singleton instance</summary>
        protected override void Awake()
        {
            base.Awake(); // Sets base singleton
            setSingleton();
        }

        /// <summary>Subscribe to NetworkManager Client state changes</summary>
        protected override void Start()
        {
            base.Start();
            subToPhotonStateEvents();
        }

        /// <summary>FishNet has a singular event tracker enum.</summary>
        private void subToPhotonStateEvents()
        {
            throw new NotImplementedException("TODO: subToPhotonStateEvents");
        }

        /// <summary>Allow this script to be called from anywhere.</summary>
        private void setSingleton()
        {
            if (Singleton != null)
            {
                Debug.LogError($"[{nameof(PhotonStateMgr)}]**ERR @ " +
                    $"{nameof(setSingleton)}: Destroying dupe");
                
                Destroy(gameObject);
                return;
            }

            Singleton = this;
        }
        #endregion // Init
        
        
        #region NetworkManager Server
        /// <summary>Starts a NetworkManager local Server.</summary>
        public void StartServer()
        {
            throw new NotImplementedException("TODO: StartServer");
        }

        /// <summary>Stops a NetworkManager local Server.</summary>
        public void StopServer()
        {
            throw new NotImplementedException("TODO: StopServer");
        }
        #endregion // NetworkManager Server
        
        
        #region NetworkManager Client
        
        ///<summary>
        /// Connect to the NetworkManager Server as a NetworkManager Client using custom host:ip.
        /// We'll set the host:ip to the NetworkManger -> then call StartClientFromNetworkMgr().
        /// </summary>
        /// <param name="_hostPort">Contains "host:port" - eg: "1.proxy.hathora.dev:12345"</param>
        /// <returns>
        /// startedConnection; to *attempt* the connection (isValid pre-connect vals); we're not connected yet.
        /// </returns>
        public bool StartClient(string _hostPort)
        {
            // Wrong overload?
            if (string.IsNullOrEmpty(_hostPort))
                return StartClient();
            
            string logPrefix = $"[{nameof(PhotonStateMgr)}] {nameof(StartClient)}]"; 
            Debug.Log($"{logPrefix} Start");
            
            (string hostNameOrIp, ushort port) hostPortContainer = SplitPortFromHostOrIp(_hostPort);
            bool hasHost = !string.IsNullOrEmpty(hostPortContainer.hostNameOrIp);
            bool hasPort = hostPortContainer.port > 0;

            // Start FishNet Client via selected Transport
            if (!hasHost)
            {
                Debug.LogError($"{logPrefix} !hasHost (from provided `{_hostPort}`): " +
                    "Instead, using default NetworkSettings config");
            }
            else if (!hasPort)
            {
                Debug.LogError($"{logPrefix} !hasPort (from provided `{_hostPort}`): " +
                    "Instead, using default NetworkSettings config");
            }
            else
            {
                // Set custom host:port 1st
                Debug.Log($"{logPrefix} w/Custom hostPort: " +
                    $"`{hostPortContainer.hostNameOrIp}:{hostPortContainer.port}`");

                throw new NotImplementedException("TODO: Set custom host:port");
            }
            
            return StartClient();
        }
        
        /// <summary>
        /// Connect to the NetworkManager Server as a NetworkManager Client using NetworkManager host:ip.
        /// This will trigger `OnClientConnecting()` related events.
        ///
        /// TRANSPORT VALIDATION:
        /// - WebGL: Asserts for `Bayou` as the NetworkManager's selected transport
        /// - !WebGL: Asserts for `!Bayou` as the NetworkManager's selected transport (such as `Tugboat` UDP)
        /// </summary>
        /// <returns>
        /// startedConnection; to *attempt* the connection (isValid pre-connect vals); we're not connected yet.
        /// </returns>
        public bool StartClient()
        {
            string logPrefix = $"[{nameof(PhotonStateMgr)}.{nameof(StartClient)}";
            Debug.Log($"{logPrefix} Start");
            
            // Validate
            bool isReadyToConnect = validateIsReadyToConnect();
            if (!isReadyToConnect)
                return false; // !startedConnection
            
            throw new NotImplementedException("TODO: Set custom host:port -> log it -> connect as client");
            // // Log "host:port (transport)" -> Connect using NetworkManager settings
            // Debug.Log($"[{logPrefix} Connecting to {GlobalConfigu.}:" +
            //     $"{transport.GetPort()}` via `{transportName}` transport");
            //
            // base.OnClientConnecting(); // => callback @ OnClientConected() || OnStartClientFail()
            // bool startedConnection = InstanceFinder.ClientManager.StartConnection();
            // return startedConnection;
        }
        
        /// <summary>
        /// Starts a NetworkManager Client using Hathora lobby session [last queried] cached host:port.
        /// Connect with info from `HathoraClientSession.ServerConnectionInfo.ExposedPort`,
        /// replacing the NetworkManager host:port.
        /// </summary>
        /// <returns>
        /// startedConnection; to *attempt* the connection (isValid pre-connect vals); we're not connected yet.
        /// </returns>
        public bool StartClientFromHathoraLobbySession()
        {
            string hostPort = GetHathoraSessionHostPort();
            return StartClient(hostPort);
        }

        /// <summary>Starts a NetworkManager Client.</summary>
        public void StopClient()
        {
            throw new NotImplementedException("TODO: StopClient");
        }
            
        
        /// <summary>We're about to connect to a server as a Client - ensure we're ready.</summary>
        /// <returns>isValid</returns>
        private bool validateIsReadyToConnect()
        {
            Debug.Log($"[{nameof(PhotonStateMgr)}] {nameof(validateIsReadyToConnect)}");
            
            throw new NotImplementedException("TODO: validateIsReadyToConnect");
            
            // Success - ready to connect
            return true;
        }
        #endregion // NetworkManager Client
        

        #region Common Utils
        // TODO: Find how to subscribe to the Photon state. Fishnet example below.
        // private void OnClientConnectionState(ClientConnectionStateArgs _state)
        // {
        //     localConnectionState = _state.ConnectionState;
        //     Debug.Log($"[HathoraFishnetClient.OnClientConnectionState] " +
        //         $"New state: {localConnectionState}");
        //     
        //     switch (localConnectionState)
        //     {
        //         case LocalConnectionState.Starting:
        //             base.OnClientStarting();
        //             break;
        //
        //         // onConnectSuccess?
        //         case LocalConnectionState.Started:
        //             base.OnClientStarted();
        //             break;
        //         
        //         case LocalConnectionState.Stopped:
        //             // Failed to connect, or stopped cleanly?
        //             if (base.ClientState == ClientTrackedState.Connecting)
        //                 base.OnStartClientFail(CONNECTION_STOPPED_FRIENDLY_STR);
        //             else
        //                 base.OnClientStopped();
        //             
        //             break;
        //     }
        // }
        #endregion // Common Utils


        private void OnDestroy()
        {
            // TODO: Unsub to events
        }
    }
}
