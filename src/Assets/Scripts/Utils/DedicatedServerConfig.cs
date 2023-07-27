using System.Collections.Generic;

namespace Fusion.Sample.DedicatedServer.Utils {
  public class DedicatedServerConfig {

    public string SessionName { get; set; }
    public string Region { get; set; }
    public string Lobby { get; set; }
    public ushort Port { get; set; } = 27015;
    public ushort PublicPort { get; set; }
    public string PublicIP { get; set; }
    public Dictionary<string, SessionProperty> SessionProperties { get; private set; } = new Dictionary<string, SessionProperty>();

    private DedicatedServerConfig() { }

    public static DedicatedServerConfig Resolve() {

      var config = new DedicatedServerConfig();

      // Session Name
      if (CommandLineUtils.TryGetArg(out string sessionName, "-session")) { config.SessionName = sessionName; }

      // Custom Region
      if (CommandLineUtils.TryGetArg(out string customRegion, "-region")) { config.Region = customRegion; }

      // Server Lobby
      if (CommandLineUtils.TryGetArg(out string customLobby, "-lobby")) { config.Lobby = customLobby; }

      // Server Port
      if (CommandLineUtils.TryGetArg(out string customPort, "-port", "-PORT") &&
        ushort.TryParse(customPort, out var port)) {
        config.Port = port;
      }

      // Custom Public IP
      if (CommandLineUtils.TryGetArg(out string customPublicIP, "-publicip")) { config.PublicIP = customPublicIP; }
      
      // Custom Public Port
      if (CommandLineUtils.TryGetArg(out string customPublicPort, "-publicport") &&
        ushort.TryParse(customPublicPort, out var publicPort)) {
        config.PublicPort = publicPort;
      }

      // Server Properties
      var argsCustomProps = CommandLineUtils.GetArgumentList("-P");

      foreach (var item in argsCustomProps) {
        var key = item.Item1;
        var value = item.Item2;

        if (int.TryParse(value, out var result)) {
          config.SessionProperties.Add(key, result);
          continue;
        }

        config.SessionProperties.Add(key, value);
      }

      return config;
    }

    public override string ToString() {

      var properties = string.Empty;

      foreach (var item in SessionProperties) {
        properties += $"{item.Value}={item.Value}, ";
      }

      return $"[{nameof(DedicatedServerConfig)}]: " +
        $"{nameof(SessionName)}={SessionName}, " +
        $"{nameof(Region)}={Region}, " +
        $"{nameof(Lobby)}={Lobby}, " +
        $"{nameof(Port)}={Port}, " +
        $"{nameof(PublicIP)}={PublicIP}, " +
        $"{nameof(PublicPort)}={PublicPort}, " +
        $"{nameof(SessionProperties)}={properties}]";
    }
  }
}
