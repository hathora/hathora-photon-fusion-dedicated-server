// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using ExitGames.Client.Photon;
using Fusion;
using Fusion.Sockets;
using Hathora.Core.Scripts.Runtime.Client;
using HathoraCloud.Models.Shared;
using UnityEngine;

namespace HathoraPhoton
{
    /// <summary>
    /// Acts as the liason between NetworkManager and Hathora managers such as HathoraClientMgr.
    /// - This child class tracks NetworkManager state changes, and:
    ///   * Handles setting the NetworkManager [host|ip]:port.
    ///   * Can talk to Hathora scripts to get cached host:port.
    ///   * Can initialize or stop NetworkManager connections.
    ///   * Tells base to log + trigger OnDone() events other scripts subscribe to.
    /// - Base contains functions like: StartServer, StartClient, StartHost.
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
            ConnectionType connectionType = runner.GetPlayerConnectionType(runner.LocalPlayer);
            Debug.Log($"[{nameof(PhotonStateMgr)}] {nameof(OnConnectedToServer)}: " +
                $"connectionType: {connectionType}");
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
