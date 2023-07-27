// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Common.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Server.Models
{
    /// <summary>
    /// Result model for HathoraServerLobbyApi.ServerGetDeployedInfoAsync().
    /// </summary>
    public class HathoraGetDeployInfoResult
    {
        #region Vars
        public string EnvVarProcessId { get; private set; }
        public Process ProcessInfo { get; set; }
        public Lobby Lobby { get; set; }
        public List<PickRoomExcludeKeyofRoomAllocations> ActiveRoomsForProcess { get; set; }
        #endregion // Vars
        

        #region Utils
        public bool HasPort => ProcessInfo?.ExposedPort?.Port > 0;
        
        /// <summary>
        /// Return host:port sync (opposed to GetHathoraServerIpPort async).
        /// </summary>
        /// <returns></returns>
        public (string host, ushort port) GetHathoraServerHostPort()
        {
            ExposedPort connectInfo = ProcessInfo?.ExposedPort;

            if (connectInfo == null)
                return default;

            ushort port = (ushort)connectInfo.Port;
            return (connectInfo.Host, port);
        }
        
        /// <summary>
        /// Gets host:port from ProcessInfo.ExposedPort, then converts host to IP.
        /// Async since we use Dns to translate the Host to IP.
        /// </summary>
        /// <returns></returns>
        public async Task<(IPAddress ip, ushort port)> GetHathoraServerIpPortAsync()
        {
            (IPAddress ip, ushort port) ipPort;
            
            ExposedPort connectInfo = ProcessInfo?.ExposedPort;

            if (connectInfo == null)
            {
                UnityEngine.Debug.LogError("[HathoraGetDeployInfoResult.GetHathoraServerIpPortAsync] " +
                    "!connectInfo from ProcessInfo.ExposedPort");
                return default;
            }

            ipPort.ip = await HathoraUtils.ConvertHostToIpAddress(connectInfo.Host);
            ipPort.port = (ushort)connectInfo.Port;

            return ipPort;
        }
        
        public PickRoomExcludeKeyofRoomAllocations FirstActiveRoomForProcess => 
            ActiveRoomsForProcess?.FirstOrDefault();
        
        /// <summary>Checks for (Process, Room and Lobby) != null.</summary>
        /// <returns>isValid</returns>
        public bool CheckIsValid() => 
            ProcessInfo != null && 
            Lobby != null && 
            FirstActiveRoomForProcess != null;

        /// <summary>
        /// You probably want to parse the InitialConfig to your own model.
        /// Forwards Lobby to Hathora util
        /// </summary>
        /// <typeparam name="TInitConfig"></typeparam>
        /// <returns></returns>
        public TInitConfig GetLobbyInitConfig<TInitConfig>() =>
            HathoraUtils.GetLobbyInitConfig<TInitConfig>(this.Lobby);
        #endregion // Utils

        
        #region Constructors
        public HathoraGetDeployInfoResult(string _envVarProcessId)
        {
            this.EnvVarProcessId = _envVarProcessId;
        }

        public HathoraGetDeployInfoResult(
            string _envVarProcessId,
            Process _processInfo,
            List<PickRoomExcludeKeyofRoomAllocations> _activeRoomsForProcess,
            Lobby _lobby)
        {
            this.EnvVarProcessId = _envVarProcessId;
            this.ProcessInfo = _processInfo;
            this.ActiveRoomsForProcess = _activeRoomsForProcess;
            this.Lobby = _lobby;
        }
        #endregion // Constructors
    }
}
