using UnityEngine;
using System.Net;
using System.Threading.Tasks;
using System.Collections.Generic;
using Fusion.Sockets;
using Fusion.Photon.Realtime;
using Fusion.Sample.DedicatedServer.Utils;
using Hathora.Core.Scripts.Runtime.Server;

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
      ushort containerPort = 0,
      string customLobby = null,
      string customRegion = null,
      string customPublicIP = null,
      ushort customPublicPort = 0
    )
    {
      string logPrefix = $"[{nameof(ServerManagerBase)}.{nameof(StartSimulation)}]";
        
      // Build Custom Photon Config
      var photonSettings = PhotonAppSettings.Instance.AppSettings.GetCopy();

      if (string.IsNullOrEmpty(customRegion) == false) {
        photonSettings.FixedRegion = customRegion.ToLower();
      }

      // Build Custom External Addr
      NetAddress? externalAddr = null;

      // Parse custom public IP
      if (string.IsNullOrEmpty(customPublicIP) == false && customPublicPort > 0) {
        if (IPAddress.TryParse(customPublicIP, out var _)) {
          Log.Info($"{logPrefix} Preparing to parse ip:port from env vars: `{customPublicIP}:{customPublicPort}`");
          externalAddr = NetAddress.CreateFromIpPort(customPublicIP, customPublicPort);
        } else {
          Log.Error($"{logPrefix} Unable to parse 'Custom Public IP' - " +
              "we may run as a relay instead of a direct connection");
        }
      }
      
      Log.Info($"{logPrefix} {nameof(externalAddr)} == `{externalAddr}` | " +
          $"{nameof(containerPort)} == {containerPort}");

      // Start Runner
      return runner.StartGame(new StartGameArgs() {
        SessionName = SessionName,
        GameMode = GameMode.Server,
        SceneManager = runner.gameObject.AddComponent<NetworkSceneManagerDefault>(),
        Scene = (int)SceneDefs.GAME,
        SessionProperties = customProps,
        
        #region >> Important for Hathora Deployment >>
          
        Address = NetAddress.Any(containerPort), // Default == 7777
        CustomPublicAddress = externalAddr,
        
        #endregion // << Important for Hathora Deployment <<

        CustomLobbyName = customLobby,
        CustomPhotonAppSettings = photonSettings,
      });
    }
  }
}