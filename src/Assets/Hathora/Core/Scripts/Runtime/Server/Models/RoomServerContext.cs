// Created by dylan@hathora.dev

using System;
using System.Net;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Common.Utils;
using Newtonsoft.Json;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Server.Models
{
    /// <summary>
    /// Contains the core info you would want from HathoraServerContext in relation to a Room. 
    /// </summary>
    public class RoomServerContext
    {
        #region Core Vars
        /// <summary>
        /// Contains the Room info of the Process' active Room, set at Constructor.
        /// </summary>
        /// </summary>
        public PickRoomExcludeKeyofRoomAllocations RoomInfo { get; }
        
        /// <summary>
        /// Contains the host:ip info of RoomInfo, set at Constructor.
        /// - For ConnectionInfo other than the 1st Room, go through the HathoraServerMgr Room api.
        /// - Unlike the ConnectionInfo.ExposedPort that's specific to the Docker Container Process, this is specific to the Room.
        /// </summary>
        public ConnectionInfoV2 ConnectionInfo { get; }

        /// <summary>
        /// Optional - you may not have a Lobby unless it was explicitly created (most often by Clients).
        /// </summary>
        public Lobby Lobby { get; }
        #endregion // Core Vars


        #region Utils
        public bool HasPort => ConnectionInfo?.ExposedPort?.Port > 0;
        
        /// <summary>
        /// Return host:port sync (opposed to GetHathoraServerIpPort async).
        /// </summary>
        /// <returns></returns>
        public (string host, ushort port) GetHostPort()
        {
            ExposedPort connectInfo = ConnectionInfo?.ExposedPort;

            if (connectInfo == null)
                return default;

            ushort port = (ushort)connectInfo.Port;
            return (connectInfo.Host, port);
        }
        
        /// <summary>
        /// Async since we use Dns to translate the Host to IP.
        /// </summary>
        /// <returns></returns>
        public async Task<(IPAddress ip, ushort port)> GetConnectionInfoIpPortAsync()
        {
            string logPrefix = $"[{nameof(HathoraServerContext)}.{nameof(GetConnectionInfoIpPortAsync)}]";
            
            (IPAddress ip, ushort port) ipPort;
            
            ExposedPort connectInfo = ConnectionInfo?.ExposedPort;

            if (connectInfo == null)
            {
                Debug.LogError($"{logPrefix} !connectInfo from ConnectionInfo.ExposedPort");
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
        
        /// <summary>
        /// Checks for:
        /// - Valid Room,
        /// - Active ConnectionInfo with Port
        /// - [Optionally, checks for a Lobby]
        /// </summary>
        /// <returns>isValid</returns>
        public bool CheckIsValidActiveRoom(bool _expectingLobby) =>
            ConnectionInfo is
            {
                ExposedPort: not null, 
                Status: ConnectionInfoV2.StatusEnum.Active,
            } && 
            RoomInfo?.Status == RoomStatus.Active &&
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
        
        /// <summary>
        /// Return debug log info:
        /// - IsValid, ConnectionInfo, RoomInfo, [Lobby], hostPort, ipPort
        /// - Async to get IP info (uses async DNS namespace).
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetDebugSummary()
        {
            (IPAddress ip, ushort port) ipPort = await GetConnectionInfoIpPortAsync();
            string ipPortStr = $"{ipPort.ip}:{ipPort.port}";

            (string host, ushort port) hostPort = GetHostPort();
            string hostPortStr = $"{hostPort.host}:{hostPort.port}";

            return "\n==========================\n" +
                $"IsValid: `{CheckIsValidActiveRoom(_expectingLobby: Lobby != null)}`,\n" +
                "==========================\n" +
                $"Lobby: `{Lobby?.ToJson() ?? "null"}`,\n" +
                "==========================\n" +
                $"hostPort: `{hostPortStr}`,\n" +
                "==========================\n" +
                $"ipPort: `{ipPortStr}`,\n" +
                "==========================\n";
        }
        #endregion // Utils
        
        
        #region Constructors
        public RoomServerContext()
        {
        }

        /// <summary>
        /// Lobby is optional, but potentially important if expecting it.
        /// </summary>
        /// <param name="_roomInfo"></param>
        /// <param name="_connectionInfo"></param>
        /// <param name="_lobby">[Optional] Explicitly pass `null` if !Lobby.</param>
        public RoomServerContext(
            PickRoomExcludeKeyofRoomAllocations _roomInfo,
            ConnectionInfoV2 _connectionInfo,
            Lobby _lobby)
        {
            this.RoomInfo = _roomInfo;
            this.ConnectionInfo = _connectionInfo;
            this.Lobby = _lobby;
        }
        #endregion // Constructors
    }
}
