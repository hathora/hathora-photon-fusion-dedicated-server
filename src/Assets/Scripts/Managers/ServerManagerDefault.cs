using Fusion.Sample.DedicatedServer.Utils;
using System.Threading.Tasks;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Fusion.Sample.DedicatedServer
{

  /// <summary>
  /// Hathora edits:
  /// - Add `|| UNITY_EDITOR` to `#if UNITY_SERVER` to allow optional Editor debugging as server.
  /// - Added server + client wrappers for clarity, since #if wrappers get confusing.
  /// </summary>
  public class ServerManagerDefault : ServerManagerBase
  {
    #region Start Wrapper
    // ########################################################################################
    private async void Start()
    {
      await Task.Yield();
      await OnStart();
    }
    
    protected virtual async Task OnStart()
    {
#if UNITY_SERVER
      await LoadGameAsDedicatedServer();
#else
      LoadMenuAsClient();
#endif
    }
    // ########################################################################################
    #endregion // Start Wrapper


    #region Utils
    /// <summary>Load scene 1 (Menu) as Client</summary>
    protected virtual void LoadMenuAsClient()
    {
      Debug.Log("[ServerManagerDefault] loadMenuAsClient (`1.Menu` scene)");
      SceneManager.LoadScene((int)SceneDefs.MENU, LoadSceneMode.Single);
    }

    /// <summary>Load scene 2 (Game) as Client</summary>
    protected virtual async Task LoadGameAsDedicatedServer()
    {
      // Continue with start the Dedicated Server
      Debug.Log("[ServerManagerDefault] loadGameAsDedicatedServer (`2.Game` scene)");
      Application.targetFrameRate = 30;

      DedicatedServerConfig config = DedicatedServerConfig.Resolve();
      Debug.Log(config);

      // Start a new Runner instance
      NetworkRunner runner = Instantiate(_runnerPrefab);

      // Start the Server
      StartGameResult result = await StartSimulation(runner, config);

      // Check if all went fine
      if (result.Ok) { Log.Debug($"Runner Start DONE"); }
      else
      {
        // Quit the application if startup fails
        Log.Debug($"Error while starting Server: {result.ShutdownReason}");

        // it can be used any error code that can be read by an external application
        // using 0 means all went fine
        Application.Quit(1);
      }
    }
    #endregion // Utils

  }
}