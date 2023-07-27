using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Fusion.Sockets;
using Fusion.Photon.Realtime;
using Fusion.Sample.DedicatedServer.Utils;

namespace Fusion.Sample.DedicatedServer {

  public abstract class ServerManagerBase : MonoBehaviour {

    /// <summary>
    /// Network Runner Prefab used to Spawn a new Runner used by the Server
    /// </summary>
    [SerializeField] protected NetworkRunner _runnerPrefab;

    protected Task<StartGameResult> StartSimulation(NetworkRunner runner, DedicatedServerConfig serverConfig) => StartSimulation(
      runner,
      serverConfig.SessionName,
      serverConfig.SessionProperties,
      serverConfig.Port,
      serverConfig.Lobby,
      serverConfig.Region,
      serverConfig.PublicIP,
      serverConfig.PublicPort
    );

    protected Task<StartGameResult> StartSimulation(
      NetworkRunner runner,
      string SessionName,
      Dictionary<string, SessionProperty> customProps = null,
      ushort port = 0,
      string customLobby = null,
      string customRegion = null,
      string customPublicIP = null,
      ushort customPublicPort = 0
    ) {

      // Build Custom Photon Config
      var photonSettings = PhotonAppSettings.Instance.AppSettings.GetCopy();

      if (string.IsNullOrEmpty(customRegion) == false) {
        photonSettings.FixedRegion = customRegion.ToLower();
      }

      // Build Custom External Addr
      NetAddress? externalAddr = null;

      if (string.IsNullOrEmpty(customPublicIP) == false && customPublicPort > 0) {
        if (IPAddress.TryParse(customPublicIP, out var _)) {
          externalAddr = NetAddress.CreateFromIpPort(customPublicIP, customPublicPort);
        } else {
          Log.Warn("Unable to parse 'Custom Public IP'");
        }
      }

      // Start Runner
      return runner.StartGame(new StartGameArgs() {
        SessionName = SessionName,
        GameMode = GameMode.Server,
        SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
        Scene = (int)SceneDefs.GAME,
        SessionProperties = customProps,
        Address = NetAddress.Any(port),
        CustomPublicAddress = externalAddr,
        CustomLobbyName = customLobby,
        CustomPhotonAppSettings = photonSettings,
      });
    }
  }
}