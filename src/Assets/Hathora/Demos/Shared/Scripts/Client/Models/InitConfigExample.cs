// Created by dylan@hathora.dev

using Newtonsoft.Json;

namespace Hathora.Demos.Shared.Scripts.Client.Models
{
    /// <summary>Arbitrary example/demo for how to use `InitialConfig`.</summary>
    public class InitConfigExample
    {
        /// <summary>Arbitrary example.</summary>
        [JsonProperty("gameMode")]
        public int GameMode = 0;

        /// <summary>Returns a stringified json of this class.</summary>
        /// <returns>Escaped version of: `{ "gameMode": 0 }`</returns>
        public override string ToString() =>
            JsonConvert.SerializeObject(this);
    }
}
