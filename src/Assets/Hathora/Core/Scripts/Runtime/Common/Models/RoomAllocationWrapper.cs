// Created by dylan@hathora.dev

using System;
using System.Globalization;
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
    public class RoomAllocationWrapper
    {
        /// <summary>
        /// (!) Originally a nullable DateTime, but Unity's SerializeField
        /// doesn't support nullable types.
        /// </summary>
        [SerializeField, JsonProperty("unscheduledAt")]
        private string _unscheduledAtDateTimeWrapper;
        public DateTime? UnscheduledAt
        { 
            get => DateTime.TryParse(_unscheduledAtDateTimeWrapper, out DateTime parsedDateTime) 
                ? parsedDateTime 
                : DateTime.MinValue;

            set => _unscheduledAtDateTimeWrapper = value.ToString();
        }

        [SerializeField, JsonProperty("scheduledAt")]
        private string _scheduledAtDateTimeWrapper;
        public DateTime ScheduledAt
        { 
            get => DateTime.TryParse(_scheduledAtDateTimeWrapper, out DateTime parsedDateTime) 
                ? parsedDateTime 
                : DateTime.MinValue;

            set => _scheduledAtDateTimeWrapper = value.ToString(CultureInfo.InvariantCulture);
        }

        [SerializeField, JsonProperty("processId")]
        private string _processId;
        public string ProcessId
        { 
            get => _processId;
            set => _processId = value;
        }

        [SerializeField, JsonProperty("roomAllocationId")]
        private string _roomAllocationId;
        public string RoomAllocationId
        { 
            get => _roomAllocationId;
            set => _roomAllocationId = value;
        }

        public RoomAllocationWrapper(RoomAllocation _roomAllocation)
        {
            if (_roomAllocation == null)
                return;
            
            this.UnscheduledAt = _roomAllocation.UnscheduledAt;
            this.ScheduledAt = _roomAllocation.ScheduledAt;
            this.ProcessId = _roomAllocation.ProcessId;
            this.RoomAllocationId = _roomAllocation.RoomAllocationId;
        }

        public RoomAllocation ToRoomAllocationType()
        {
            // (!) SDK constructor throws on req'd val == null
            
            RoomAllocation roomAllocation = null;
            try
            {
                roomAllocation = new RoomAllocation(
                    UnscheduledAt,
                    ScheduledAt,
                    ProcessId,
                    RoomAllocationId
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }
            
            return roomAllocation;
        }
    }
}