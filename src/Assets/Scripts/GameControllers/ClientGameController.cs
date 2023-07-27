using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;
using Fusion.Sockets;
using Fusion.Sample.DedicatedServer.Utils;

namespace Fusion.Sample.DedicatedServer {

  public class ClientGameController : MonoBehaviour, INetworkRunnerCallbacks {

    [SerializeField] private NetworkRunner _runnerPrefab;

    private string _sessionName;
    private string _lobbyName;
    private NetworkRunner _instanceRunner;

    private enum State {
      SelectMode,
      StartClient,
      JoinLobby,
      LobbyJoined,
      Started,
    }

    private State _currentState;
    private List<SessionInfo> _currentSessionList;

    void Awake() {
      Application.targetFrameRate = 60;
    }

    void OnGUI() {
      Rect area = new Rect(10, 90, Screen.width - 20, Screen.height - 100);

      GUILayout.BeginArea(area);

      switch (_currentState) {
        case State.SelectMode: State_SelectMode(); break;
        case State.StartClient: State_StartClient(); break;
        case State.JoinLobby: State_JoinLobby(); break;
        case State.LobbyJoined: State_LobbyJoined(); break;
        case State.Started: State_Started(); break;
      }

      if (_instanceRunner != null && _instanceRunner.IsCloudReady) {
        GUILayout.FlexibleSpace();

        GUILayout.BeginHorizontal();
        {
          GUILayout.FlexibleSpace();

          if (GUILayout.Button("Shutdown", GUILayout.ExpandWidth(false), GUILayout.MinHeight(50), GUILayout.MinWidth(200))) {

            _instanceRunner.Shutdown();
          }
        }
        GUILayout.EndHorizontal();
      }

      GUILayout.EndArea();
    }

    void State_SelectMode() {
      GUILayout.BeginHorizontal();
      GUILayout.Label("Session Name:", GUILayout.ExpandWidth(false));
      _sessionName = GUILayout.TextField(_sessionName)?.Trim();
      GUILayout.EndHorizontal();

      GUILayout.BeginHorizontal();
      GUILayout.Label("Custom Lobby:", GUILayout.ExpandWidth(false));
      _lobbyName = GUILayout.TextField(_lobbyName)?.Trim();
      GUILayout.EndHorizontal();

      if (ExpandButton("Client")) {
        _currentState = State.StartClient;
      }

      if (ExpandButton("Join Lobby")) {
        _currentState = State.JoinLobby;
      }
    }

    async void State_StartClient() {
      _instanceRunner = GetRunner("Client");

      _currentState = State.Started;

      var result = await StartSimulation(_instanceRunner, GameMode.Client, _sessionName);

      if (result.Ok == false) {
        Debug.LogWarning(result.ShutdownReason);

        _currentState = State.SelectMode;
      } else {
        Debug.Log("Done");
      }
    }

    async void State_JoinLobby() {
      _instanceRunner = GetRunner("Client");

      _currentState = State.LobbyJoined;

      var result = await JoinLobby(_instanceRunner);

      if (result.Ok == false) {
        Debug.LogWarning(result.ShutdownReason);

        _currentState = State.SelectMode;
      } else {
        Debug.Log("Done");
      }
    }

    void State_Started() { }

    void State_LobbyJoined() {

      if (_currentSessionList != null && _currentSessionList.Count > 0) {
        GUILayout.BeginVertical();

        foreach (var session in _currentSessionList.ToArray()) {

          GUILayout.BeginHorizontal();

          var props = "";
          foreach (var item in session.Properties) {
            props += $"{item.Key}={item.Value.PropertyValue}, ";
          }

          GUILayout.Label($"Session: {session.Name} ({props})");

          if (GUILayout.Button("Join", GUILayout.ExpandWidth(false), GUILayout.MinWidth(200))) {

            StartSimulation(_instanceRunner, GameMode.Client, session.Name);

            _currentState = State.Started;
          }

          GUILayout.EndHorizontal();
        }

        GUILayout.EndVertical();
      }
    }

    private NetworkRunner GetRunner(string name) {

      var runner = Instantiate(_runnerPrefab);
      runner.name = name;
      runner.ProvideInput = true;
      runner.AddCallbacks(this);

      return runner;
    }

    public Task<StartGameResult> StartSimulation(
        NetworkRunner runner,
        GameMode gameMode,
        string sessionName
      ) {

      return runner.StartGame(new StartGameArgs() {
        SessionName = sessionName,
        GameMode = gameMode,
        SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
        Scene = SceneManager.GetActiveScene().buildIndex,
        DisableClientSessionCreation = true,
      });
    }

    public Task<StartGameResult> JoinLobby(NetworkRunner runner) {
      return runner.JoinSessionLobby(string.IsNullOrEmpty(_lobbyName) ? SessionLobby.ClientServer : SessionLobby.Custom, _lobbyName);
    }

    bool ExpandButton(string text) {
      return GUILayout.Button(text, GUILayout.ExpandWidth(true), GUILayout.ExpandHeight(true));
    }

    // ------------ RUNNER CALLBACKS ------------------------------------------------------------------------------------

    public void OnShutdown(NetworkRunner runner, ShutdownReason shutdownReason) {

      _currentSessionList = null;
      _currentState = State.SelectMode;

      // Reload scene after shutdown

      if (Application.isPlaying) {
        SceneManager.LoadScene((byte)SceneDefs.MENU);
      }
    }

    public void OnDisconnectedFromServer(NetworkRunner runner) {
      runner.Shutdown();
    }

    public void OnConnectFailed(NetworkRunner runner, NetAddress remoteAddress, NetConnectFailedReason reason) {
      runner.Shutdown();
    }

    public void OnSessionListUpdated(NetworkRunner runner, List<SessionInfo> sessionList) {

      Log.Debug($"Received: {sessionList.Count}");

      _currentSessionList = sessionList;
    }

    #region Other callbacks
    public void OnPlayerJoined(NetworkRunner runner, PlayerRef player) { }
    public void OnPlayerLeft(NetworkRunner runner, PlayerRef player) { }
    public void OnInput(NetworkRunner runner, NetworkInput input) { }
    public void OnInputMissing(NetworkRunner runner, PlayerRef player, NetworkInput input) { }
    public void OnConnectedToServer(NetworkRunner runner) { }
    public void OnConnectRequest(NetworkRunner runner, NetworkRunnerCallbackArgs.ConnectRequest request, byte[] token) { }
    public void OnUserSimulationMessage(NetworkRunner runner, SimulationMessagePtr message) { }
    public void OnCustomAuthenticationResponse(NetworkRunner runner, Dictionary<string, object> data) { }
    public void OnReliableDataReceived(NetworkRunner runner, PlayerRef player, ArraySegment<byte> data) { }
    public void OnSceneLoadDone(NetworkRunner runner) { }
    public void OnSceneLoadStart(NetworkRunner runner) { }
    public void OnHostMigration(NetworkRunner runner, HostMigrationToken hostMigrationToken) { }
    #endregion
  }
}