// Created by dylan@hathora.dev

using System;
using Hathora.Cloud.Sdk.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Common.Models
{
    /// <summary>
    /// Hathora SDK model wrapper to add [Serializable] support.
    /// 
    /// Set transpport configurations for where the server will listen.
    /// --- 
    /// This is a wrapper for Hathora SDK's `ContainerPort` model.
    /// We'll eventually replace this with a [Serializable] revamp of the model.
    /// </summary>
    [Serializable]
    public class ContainerPortWrapper
    {
        /// <summary>(!) Hathora SDK Enums starts at index 1; not 0: Care of indexes</summary>
        [SerializeField, JsonProperty("transportType")]
        private TransportType _transportType = TransportType.Udp;
       
        /// <summary>(!) Hathora SDK Enums starts at index 1; not 0: Care of indexes</summary>
        public TransportType TransportType
        {
            get => _transportType;
            set => _transportType = value;
        }    
        

        [SerializeField, Range(1024, 65535), JsonProperty("portNumber")]
        private int _portNumber = 7777;
        public int PortNumber
        {
            get => _portNumber;
            set => _portNumber = value;
        }

        
        public ContainerPortWrapper()
        {
        }

        /// <summary>Handle "Host" in child.</summary>
        /// <param name="_exposedPort"></param>
        public ContainerPortWrapper(ExposedPort _exposedPort)
        {
            if (_exposedPort == null)
                return;
            
            this._transportType = _exposedPort.TransportType;
            this._portNumber = (int)_exposedPort.Port;
            // this.Nickname = _exposedPort.Name; // Always "default": See GetTransportNickname()
        }
        
        /// <summary>Handle Nickname in child.</summary>
        /// <param name="_containerPort"></param>
        public ContainerPortWrapper(ContainerPort _containerPort)
        {
            if (_containerPort == null)
                return;
            
            this._transportType = _containerPort.TransportType;
            this._portNumber = _containerPort.Port;
        }
        
        /// <summary>
        /// Override this if you want the name to be custom
        /// </summary>
        /// <returns></returns>
        public virtual string GetTransportNickname() => "default";

        public virtual ContainerPort ToContainerPortType()
        {
            ContainerPort containerPort = null;
            string containerName = this.GetTransportNickname();
            
            try
            {
                containerPort = new ContainerPort(
                    this.TransportType,
                    this.PortNumber,
                    containerName
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }

            return containerPort;
        }

    }
}
