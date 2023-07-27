using System.Collections.Generic;
using UnityEngine;
using Fusion.Sockets;
using System;

namespace Fusion.Sample.DedicatedServer {

  [SimulationBehaviour(Modes = SimulationModes.Server)]
  public class ServerGameController : SimulationBehaviour, INetworkRunnerCallbacks {

    [SerializeField] private NetworkObject _playerPrefab;

    private readonly Dictionary<PlayerRef, NetworkObject> _playerMap = new Dictionary<PlayerRef, NetworkObject>();

    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) {

      if (runner.IsServer && _playerPrefab != null) {

        var pos = UnityEngine.Random.insideUnitSphere * 3;
        pos.y = 1;

        var character = runner.Spawn(_playerPrefab, pos, Quaternion.identity, inputAuthority: player);

        _playerMap[player] = character;

        Log.Info($"Spawn for Player: {player}");
      }
    }

    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) {

      if (_playerMap.TryGetValue(player, out var character)) {
        // Despawn Player
        runner.Despawn(character);

        // Remove player from mapping
        _playerMap.Remove(player);

        Log.Info($"Despawn for Player: {player}");
      }

      if (_playerMap.Count == 0) {
        Log.Info("Last player left, shutdown...");
        // Shutdown Server after the last player leaves
        runner.Shutdown();
      }
    }
    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {
      // Quit application after the Server Shutdown
      Application.Quit(0);
    }

    #region Unused Callbacks
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnDisconnectedFromServer(NetworkRunner runner) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    #endregion
  }
}