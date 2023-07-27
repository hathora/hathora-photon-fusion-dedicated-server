using Fusion.Sample.DedicatedServer.Utils;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion.Sample.DedicatedServer {

  public class ServerManagerDefault : ServerManagerBase {

    async void Start() {
      await Task.Yield();

#if UNITY_SERVER
      // Continue with start the Dedicated Server
      Application.targetFrameRate = 30;

      var config = DedicatedServerConfig.Resolve();
      Debug.Log(config);

      // Start a new Runner instance
      var runner = Instantiate(_runnerPrefab);

      // Start the Server
      var result = await StartSimulation(runner, config);

      // Check if all went fine
      if (result.Ok) {
        Log.Debug($"Runner Start DONE");
      } else {
        // Quit the application if startup fails
        Log.Debug($"Error while starting Server: {result.ShutdownReason}");

        // it can be used any error code that can be read by an external application
        // using 0 means all went fine
        Application.Quit(1);
      }
#else
      SceneManager.LoadScene((int)SceneDefs.MENU, LoadSceneMode.Single);
#endif
    }
  }
}