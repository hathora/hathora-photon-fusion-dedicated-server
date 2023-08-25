// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Threading.Tasks;
using Hathora.Cloud.Sdk.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Server.Models
{
    /// <summary>
    /// Result model for HathoraServerMgr.GetHathoraServerContextAsync().
    /// - Can get Hathora Process, Room, [Lobby].
    /// - Can quick check if valid via `CheckIsValidActiveRoom`.
    /// - Contains utils to get "host:port" || "ip:port".
    /// </summary>
    public class HathoraServerContext
    {
        #region Vars
        public string EnvVarProcessId { get; private set; }
        public Process ProcessInfo { get; set; }
        public List<PickRoomExcludeKeyofRoomAllocations> ActiveRoomsForProcess { get; set; }
        
        /// <summary>
        /// Contains the Room + ConnectionInfo (host/ip/port) of the 1st active Room.
        /// - For info other than the 1st, iterate ActiveRoomsForProcess through HathoraServerMgr Room api.
        /// </summary>
        private RoomServerContext FirstRoomServerContext { get; set; }
        #endregion // Vars
        

        #region Utils
        /// <summary>
        /// Return debug log info:
        /// - IsValid, FirstRoomServerContext { IsValid, ConnectionInfo, RoomInfo, hostPort, ipPort, [Lobby] }.
        /// - Async to get IP info (uses async DNS namespace).
        /// </summary>
        /// <returns></returns>
        public async Task<string> GetDebugSummary()
        {
            string firstRoomServerContextDebugSummary = await FirstRoomServerContext.GetDebugSummary();
            
            return "\n--------------------------\n" +
                $"ConnectionInfo: `{ProcessInfo.ToJson() ?? "null"}`,\n" +
                "--------------------------\n" +
                $"FirstRoomServerContext: `{firstRoomServerContextDebugSummary}`,\n" +
                "--------------------------\n";
        }
        
        /// <summary>
        /// Checks for:
        /// - Valid ProcessInfo
        /// - At least 1 valid + Active Room
        /// - [Optionally, checks for a Lobby, if expecting one]
        /// </summary>
        /// <returns>isValid</returns>
        public bool CheckIsValidServerContext(bool _expectingLobby) =>
            ProcessInfo != null &&
            FirstRoomServerContext != null &&
            FirstRoomServerContext.CheckIsValidActiveRoom(_expectingLobby);
        #endregion // Utils

        
        #region Constructors
        public HathoraServerContext(string _envVarProcessId)
        {
            this.EnvVarProcessId = _envVarProcessId;
        }

        /// <summary>
        /// Set at HathoraServerMgr.GetHathoraServerContextAsync().
        /// </summary>
        /// <param name="_envVarProcessId"></param>
        /// <param name="_processInfo"></param>
        /// <param name="_activeRoomsForProcess"></param>
        /// <param name="_firstRoomConnectionInfo"></param>
        /// <param name="_firstRoomLobby"></param>
        public HathoraServerContext(
            string _envVarProcessId,
            Process _processInfo,
            List<PickRoomExcludeKeyofRoomAllocations> _activeRoomsForProcess,
            ConnectionInfoV2 _firstRoomConnectionInfo,
            Lobby _firstRoomLobby)
        {
            this.EnvVarProcessId = _envVarProcessId;
            this.ProcessInfo = _processInfo;
            this.ActiveRoomsForProcess = _activeRoomsForProcess;

            PickRoomExcludeKeyofRoomAllocations firstRoom = ActiveRoomsForProcess.FirstOrDefault();
            this.FirstRoomServerContext = new RoomServerContext(
                firstRoom, 
                _firstRoomConnectionInfo,
                _lobby: _firstRoomLobby);
        }
        #endregion // Constructors
    }
}
