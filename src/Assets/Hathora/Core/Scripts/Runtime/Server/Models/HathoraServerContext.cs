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
    /// Result model for HathoraServerMgr.GetHathoraServerContextAsync().
    /// - Can get Hathora Process, Room, [Lobby].
    /// - Can quick check if valid via `CheckIsValid`.
    /// - Contains utils to get "host:port" || "ip:port".
    /// </summary>
    public class HathoraServerContext
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
        /// Async since we use Dns to translate the Host to IP.
        /// </summary>
        /// <returns></returns>
        public async Task<(IPAddress ip, ushort port)> GetHathoraServerIpPortAsync()
        {
            string logPrefix = $"[{nameof(HathoraServerContext)}.{nameof(GetHathoraServerIpPortAsync)}]";
            
            (IPAddress ip, ushort port) ipPort;
            
            ExposedPort connectInfo = ProcessInfo?.ExposedPort;

            if (connectInfo == null)
            {
                Debug.LogError($"{logPrefix} !connectInfo from ProcessInfo.ExposedPort");
                return default;
            }
            
            ipPort.port = (ushort)connectInfo.Port;

            try
            {
                ipPort.ip = await HathoraUtils.ConvertHostToIpAddress(connectInfo.Host);
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} ConvertHostToIpAddress => Error: {e}");
                throw;
            }

            return ipPort;
        }
        
        public PickRoomExcludeKeyofRoomAllocations FirstActiveRoomForProcess => 
            ActiveRoomsForProcess?.FirstOrDefault();

        /// <summary>Checks for (Process, Room and Lobby) != null.</summary>
        /// <returns>isValid</returns>
        public bool CheckIsValid(bool _expectingLobby) =>
            ProcessInfo != null &&
            ProcessInfo.StoppingAt == null &&
            FirstActiveRoomForProcess != null &&
            (!_expectingLobby || Lobby != null);

        /// <summary>
        /// You probably want to parse the InitialConfig to your own model.
        /// </summary>
        /// <typeparam name="TInitConfig"></typeparam>
        /// <returns></returns>
        public TInitConfig GetLobbyInitConfig<TInitConfig>()
        {
            string logPrefix = $"[{nameof(HathoraServerContext)}.{nameof(GetLobbyInitConfig)}]";

            object initConfigObj = Lobby?.InitialConfig;
            if (initConfigObj == null)
            {
                Debug.LogError($"{logPrefix} !initConfigObj");
                return default;
            }

            try
            {
                string jsonString = initConfigObj as string;
                
                if (string.IsNullOrEmpty(jsonString))
                {
                    Debug.LogError($"{logPrefix} !jsonString");
                    return default;
                }
                
                TInitConfig initConfigParsed = JsonConvert.DeserializeObject<TInitConfig>(jsonString);
                return initConfigParsed;
            }
            catch (Exception e)
            {
                Debug.LogError($"{logPrefix} Error parsing initConfigObj: {e}");
                throw;
            }
        }
        #endregion // Utils

        
        #region Constructors
        public HathoraServerContext(string _envVarProcessId)
        {
            this.EnvVarProcessId = _envVarProcessId;
        }

        public HathoraServerContext(
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
