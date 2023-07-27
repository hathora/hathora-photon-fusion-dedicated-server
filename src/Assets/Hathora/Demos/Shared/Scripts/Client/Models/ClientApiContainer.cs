// Created by dylan@hathora.dev

using System;
using Hathora.Core.Scripts.Runtime.Client.ApiWrapper;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hathora.Demos.Shared.Scripts.Client.Models
{
    /// <summary>
    /// Client API wrapper container to serialize in HathoraManager (in scene).
    /// 
    /// -> Have a new Hathora API to add?
    /// 1. Serialize it here, and add to `HathoraClientMgrBase.InitApis()`
    /// 2. Open scene `HathoraManager` GameObj (not prefab)
    /// 3. Add the new script component to HathoraManager.Hathora{Platform}ClientMgr.ClientApis
    /// 4. Add a new script[] to ClientApis container -> drag the script into the serialized field
    /// </summary>
    [Serializable]
    public struct ClientApiContainer
    {
        [FormerlySerializedAs("clientAuthApiWrapper")]
        [Header("Hathora Client API wrappers")]
        [FormerlySerializedAs("authApi")]
        [SerializeField]
        private HathoraClientAuthApi clientAuthApi;
        public HathoraClientAuthApi ClientAuthApi => clientAuthApi;
        
        [FormerlySerializedAs("clientLobbyApiWrapper")]
        [FormerlySerializedAs("lobbyApi")]
        [SerializeField]
        private HathoraClientLobbyApi clientLobbyApi;
        public HathoraClientLobbyApi ClientLobbyApi => clientLobbyApi;

        [FormerlySerializedAs("clientRoomApiWrapper")]
        [FormerlySerializedAs("roomApi")]
        [SerializeField]
        private HathoraClientRoomApi clientRoomApi;
        public HathoraClientRoomApi ClientRoomApi => clientRoomApi;
    }
}
