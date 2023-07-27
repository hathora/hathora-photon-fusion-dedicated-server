// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using Hathora.Cloud.Sdk.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Common.Models
{
    /// <summary>
    /// Hathora SDK model wrapper to add [Serializable] support.
    /// 
    /// This is a wrapper for Hathora SDK's `Room` model.
    /// We'll eventually replace this with a [Serializable] revamp of the model.
    /// </summary>
    [Serializable]
    public class RoomWrapper
    {
        [SerializeField, JsonProperty("appId")]
        private string _appId;
        public string AppId
        {
            get => _appId;
            set => _appId = value;
        }
        
        [SerializeField, JsonProperty("status")]
        private RoomStatus _status = RoomStatus.Destroyed;
        public RoomStatus Status
        {
            get => _status;
            set => _status = value;
        }
        
        [SerializeField, JsonProperty("currentAllocation")]
        private RoomAllocationWrapper _currentAllocationWrapper;
        public RoomAllocation CurrentAllocation
        {
            get => _currentAllocationWrapper?.ToRoomAllocationType();
            set => _currentAllocationWrapper = new RoomAllocationWrapper(value);
        }

        [SerializeField, JsonProperty("allocations")]
        private List<RoomAllocationWrapper> _allocationsWrapper = new();
        public List<RoomAllocation> Allocations
        {
            get => _allocationsWrapper?.ConvertAll(wrapper => wrapper.ToRoomAllocationType());
            set => _allocationsWrapper = value?.ConvertAll(val => 
                new RoomAllocationWrapper(val));
        }

        [SerializeField, JsonProperty("roomId")]
        private string _roomId;
        public string RoomId
        {
            get => _roomId;
            set => _roomId = value;
        }


        public RoomWrapper(Room _room)
        {
            if (_room == null)
                return;
            
            this.AppId = _room.AppId;
            this.Status = _room.Status;
            this.CurrentAllocation = _room.CurrentAllocation;
            this.Allocations = _room.Allocations;
            this.RoomId = _room.RoomId;
        }

        public Room ToRoomType()
        {
            // (!) SDK constructor throws on req'd val == null
            
            Room room = null;
            try
            {
                room = new Room(
                    CurrentAllocation,
                    Status,
                    Allocations,
                    RoomId,
                    AppId
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
            
            return room;
        }
    }
}
