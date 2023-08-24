// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Sockets;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Client;
using UnityEngine;

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
    public class PhotonStateMgr : MonoBehaviour, INetworkRunnerCallbacks
    {
        #region vars
        /// <summary>
        /// `New` keyword overrides base Singleton when accessing child directly.
        /// </summary>
        public static PhotonStateMgr Singleton { get; private set; }
        #endregion // vars

        
        #region Init
        /// <summary>Set Singleton instance</summary>
        private void Awake()
        {
            Debug.Log($"[{nameof(PhotonStateMgr)}] Awake");
            setSingleton();
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
            
            (string hostNameOrIp, ushort port) hostPortContainer = splitPortFromHostOrIp(_hostPort);
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
            string hostPort = getHathoraSessionHostPort();
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
        

        #region Photon OnState Callbacks
        public void OnPlayerJoined(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnPlayerLeft(NetworkRunner runner, PlayerRef player)
        {
        }

        public void OnInput(NetworkRunner runner, NetworkInput input)
        {
        }

        public void OnInputMissing(
            NetworkRunner runner,
            PlayerRef player,
            NetworkInput input)
        {
        }

        public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason)
        {
        }

        public void OnConnectedToServer(NetworkRunner runner)
        {
        }

        public void OnDisconnectedFromServer(NetworkRunner runner)
        {
        }

        public void OnConnectRequest(
            NetworkRunner runner,
            NetworkRunnerCallbackArgs.ConnectRequest request,
            byte[] token)
        {
        }

        public void OnConnectFailed(
            NetworkRunner runner,
            NetAddress remoteAddress,
            NetConnectFailedReason reason)
        {
        }

        public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message)
        {
        }

        public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList)
        {
        }

        public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data)
        {
        }

        public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken)
        {
        }

        public void OnReliableDataReceived(
            NetworkRunner runner,
            PlayerRef player,
            ArraySegment<byte> data)
        {
        }

        public void OnSceneLoadDone(NetworkRunner runner)
        {
        }

        public void OnSceneLoadStart(NetworkRunner runner)
        {
        }
        #endregion // Photon OnState Callbacks
        
        
        #region Hathora
        /// <summary>
        /// Get the last queried "host:port" from a Hathora Client session.
        /// - From `HathoraClientSession.ServerConnectionInfo.ExposedPort`.
        /// </summary>
        private string getHathoraSessionHostPort()
        {
            ExposedPort connectInfo = HathoraClientSession.Singleton.ServerConnectionInfo.ExposedPort;

            string hostPort = $"{connectInfo.Host}:{connectInfo.Port}";
            return hostPort;
        }
        #endregion Hathora
        
        
        #region Utils
        /// <summary>
        /// This was likely passed in from the UI to override the default
        /// NetworkManager (often from Standalone Client). Eg:
        /// "1.proxy.hathora.dev:12345" -> "1.proxy.hathora.dev", 12345
        /// </summary>
        /// <param name="_hostPort"></param>
        /// <returns></returns>
        private static (string hostNameOrIp, ushort port) splitPortFromHostOrIp(string _hostPort)
        {
            if (string.IsNullOrEmpty(_hostPort))
                return default;
            
            string[] hostPortArr = _hostPort.Split(':');
            string hostNameOrIp = hostPortArr[0];
            ushort port = ushort.Parse(hostPortArr[1]);
            
            return (hostNameOrIp, port);
        }
        #endregion // Utils

        
        #region Cleanup
        private void OnDestroy()
        {
            // TODO: Unsub to events
        }
        #endregion // Cleanup
    }
}
