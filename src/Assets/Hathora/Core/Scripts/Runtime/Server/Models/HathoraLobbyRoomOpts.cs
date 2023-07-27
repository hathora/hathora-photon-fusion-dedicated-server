// dylan@hathora.dev

using Hathora.Cloud.Sdk.Model;
using UnityEngine;
using System;
using UnityEngine.Serialization;

namespace Hathora.Core.Scripts.Runtime.Server.Models
{
    [Serializable] 
    public class HathoraLobbyRoomOpts
    {
        #region Hathora Region
        /// <summary>
        /// (!) Hathora SDK Enums starts at index 1; not 0: Care of indexes.
        /// Since this Enum isn't alphabatized, also care if you Sort() the list.
        /// </summary>
        [SerializeField]
        private Region _hathoraRegion = Region.Seattle;
        
        /// <summary>
        /// (!) Hathora SDK Enums starts at index 1; not 0: Care of indexes.
        /// Since this Enum isn't alphabatized, also care if you Sort() the list.
        /// </summary>
        public Region HathoraRegion
        {
            get => (int)_hathoraRegion == 0
                ? Region.Seattle // Fallback for 0-based index
                : _hathoraRegion;
            
            set => _hathoraRegion = value;
        }

        /// <summary>
        /// (!) Hathora SDK Enums starts at index 1; not 0: Care of indexes.
        /// Since this Enum isn't alphabatized, also care if you Sort() the list.
        /// </summary>
        [FormerlySerializedAs("hathoraRegionSelectedIndexUi")]
        [SerializeField]
        private int _hathoraRegionSelectedIndexUi = (int)Region.Seattle;
        
        /// <summary>
        /// (!) Hathora SDK Enums starts at index 1; not 0: Care of indexes.
        /// Since this Enum isn't alphabatized, also care if you Sort() the list.
        /// </summary>
        public int SortedRegionSelectedIndexUi
        {
            get => _hathoraRegionSelectedIndexUi;
            set => _hathoraRegionSelectedIndexUi = value;
        }


        // [SerializeField] // While Rooms last only 5m, don't actually persist this
        private HathoraCachedRoomConnection _lastCreatedRoomConnection;
        public HathoraCachedRoomConnection LastCreatedRoomConnection
        {
            get => _lastCreatedRoomConnection;
            set => _lastCreatedRoomConnection = value;
        }

        /// <summary>
        /// We check if there's a RoomId, and null checking leading up to it.
        /// </summary>
        public bool HasLastCreatedRoomConnection => 
            !string.IsNullOrEmpty(_lastCreatedRoomConnection?.Room?.RoomId ?? string.Empty) && 
            _lastCreatedRoomConnection?.ConnectionInfoV2?.ExposedPort != null;

        /// <summary>
        /// Checks if room has IsError *only*. Returns false if connection is null.
        /// </summary>
        public bool HasLastCreatedRoomConnectionErr => _lastCreatedRoomConnection?.IsError ?? false;
        #endregion // Hathora Region
    }
}
 