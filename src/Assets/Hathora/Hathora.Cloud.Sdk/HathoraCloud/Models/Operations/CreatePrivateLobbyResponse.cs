
//------------------------------------------------------------------------------
// <auto-generated>
// This code was generated by Speakeasy (https://speakeasyapi.dev). DO NOT EDIT.
//
// Changes to this file may cause incorrect behavior and will be lost when
// the code is regenerated.
// </auto-generated>
//------------------------------------------------------------------------------
#nullable enable
namespace HathoraCloud.Models.Operations
{
    using HathoraCloud.Models.Shared;
    using System;
    using UnityEngine.Networking;
    using UnityEngine;
    
    [Serializable]
    public class CreatePrivateLobbyResponse: IDisposable
    {

        /// <summary>
        /// HTTP response content type for this operation
        /// </summary>
        [SerializeField]
        public string? ContentType { get; set; } = default!;
        

        [SerializeField]
        public string? CreatePrivateLobby400ApplicationJSONString { get; set; }
        

        [SerializeField]
        public string? CreatePrivateLobby401ApplicationJSONString { get; set; }
        

        [SerializeField]
        public string? CreatePrivateLobby404ApplicationJSONString { get; set; }
        

        [SerializeField]
        public string? CreatePrivateLobby422ApplicationJSONString { get; set; }
        

        [SerializeField]
        public string? CreatePrivateLobby429ApplicationJSONString { get; set; }
        

        [SerializeField]
        public string? CreatePrivateLobby500ApplicationJSONString { get; set; }
        

        [SerializeField]
        public Lobby? Lobby { get; set; }
        

        /// <summary>
        /// HTTP response status code for this operation
        /// </summary>
        [SerializeField]
        public int StatusCode { get; set; } = default!;
        

        /// <summary>
        /// Raw HTTP response; suitable for custom response parsing
        /// </summary>
        [SerializeField]
        public UnityWebRequest? RawResponse { get; set; }
        
        public void Dispose() {
            if (RawResponse != null) {
                RawResponse.Dispose();
            }
        }
    }
}