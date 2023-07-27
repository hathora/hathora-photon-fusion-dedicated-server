// Created by dylan@hathora.dev

using System;
using Hathora.Core.Scripts.Runtime.Server.ApiWrapper;

namespace Hathora.Core.Scripts.Runtime.Server.Models
{
    /// <summary>
    /// Server [runtime] API wrapper container. Unlike Clients, we init these
    /// with a new() constructor since they don't inherit from Mono
    /// 
    /// -> Have a new Hathora API to add?
    /// 1. Serialize it here, and add to `HathoraClientMgrBase.InitApis()`
    /// 2. Open scene `HathoraManager` GameObj (not prefab)
    /// 3. Add the new script component to HathoraManager.Hathora{Platform}ClientMgr.ClientApis
    /// 4. Add a new script[] to ClientApis container -> drag the script into the serialized field
    /// </summary>
    [Serializable]
    public struct ServerApiContainer
    {
        public HathoraServerAppApi ServerAppApi { get; set; }
        // public HathoraServerBuildApi ServerBuildApi { get; set; } // Not generally intended for runtime use
        public HathoraServerLobbyApi ServerLobbyApi { get;set; }
        public HathoraServerProcessApi ServerProcessApi { get; set; }
        public HathoraServerRoomApi ServerRoomApi { get; set; }
    }
}
