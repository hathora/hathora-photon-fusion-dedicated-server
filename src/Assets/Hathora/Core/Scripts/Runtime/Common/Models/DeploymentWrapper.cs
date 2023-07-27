// Created by dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Globalization;
using Hathora.Cloud.Sdk.Model;
using Newtonsoft.Json;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Common.Models
{
    /// <summary>
    /// Hathora SDK model wrapper to add [Serializable] support.
    /// 
    /// This is a wrapper for Hathora SDK's `Deployment` model.
    /// We'll eventually replace this with a [Serializable] revamp of the model.
    /// </summary>
    [Serializable]
    public class DeploymentWrapper
    {
        [SerializeField, JsonProperty("planName")]
        private PlanName _planName;
        public PlanName PlanName 
        { 
            get => _planName;
            set => _planName = value;
        }
        
        [SerializeField, JsonProperty("transportType")]
        private TransportType _transportType;
        public TransportType TransportType 
        { 
            get => _transportType;
            set => _transportType = value;
        }
        
        [SerializeField, JsonProperty("env")] // TODO
        private List<DeploymentEnvInner> _env;
        public List<DeploymentEnvInner> Env
        {
            get => new List<DeploymentEnvInner>();
            // get => _env;
            
            set => _env = value;

        }
        
        [SerializeField, JsonProperty("roomsPerProcess")]
        private double _roomsPerProcess;
        public double RoomsPerProcess 
        { 
            get => _roomsPerProcess;
            set => _roomsPerProcess = value;
        }
        
        
        /// <summary>Parses from List of ContainerPort</summary>
        [SerializeField, JsonProperty("additionalContainerPorts")]
        private List<AdditionalContainerPortWrapper> _additionalContainerPorts;

        /// <summary>Parses from List of ContainerPort</summary>
        public List<ContainerPort> AdditionalContainerPorts
        {
            get => _additionalContainerPorts?.ConvertAll(wrapper => wrapper.ToContainerPortType());
            set => _additionalContainerPorts = value?.ConvertAll(val => 
                new AdditionalContainerPortWrapper(val));
        }
        

        /// <summary>Parses from ContainerPort</summary>
        [SerializeField, JsonProperty("defaultContainerPort")]
        private ContainerPortWrapper _defaultContainerPortWrapper = new();
        public ContainerPort DefaultContainerPort 
        { 
            get => _defaultContainerPortWrapper?.ToContainerPortType();
            set => _defaultContainerPortWrapper = new ContainerPortWrapper(value);
        }
        
        
        /// <summary>Serialized from `DateTime`.</summary>
        [SerializeField, JsonProperty("createdAt")]
        private string _createdAtWrapper;
        
        /// <summary>Serialized from `DateTime`.</summary>
        public DateTime CreatedAt
        {
            get => DateTime.TryParse(_createdAtWrapper, out DateTime parsedDateTime) 
                ? parsedDateTime 
                : DateTime.MinValue;
            
            set => _createdAtWrapper = value.ToString(CultureInfo.InvariantCulture);
        }
        
        
        [SerializeField, JsonProperty("createdBy")]
        private string _createdBy;
        public string CreatedBy
        {
            get => _createdBy ?? "";
            set => _createdBy = value;
        }
        
        [SerializeField, JsonProperty("requestedMemoryMB")]
        private double _requestedMemoryMB;
        public double RequestedMemoryMB 
        { 
            get => _requestedMemoryMB;
            set => _requestedMemoryMB = value;
        }
        
        [SerializeField, JsonProperty("requestedCPU")]
        private double _requestedCPU;
        public double RequestedCPU 
        { 
            get => _requestedCPU;
            set => _requestedCPU = value;
        }
        
        [SerializeField, JsonProperty("deploymentId")]
        private double _deploymentId;
        public double DeploymentId 
        { 
            get => _deploymentId;
            set => _deploymentId = value;
        }
        
        [SerializeField, JsonProperty("buildId")]
        private double _buildId;
        public double BuildId 
        { 
            get => _buildId;
            set => _buildId = value;
        }
        
        [SerializeField, JsonProperty("appId")]
        private string _appId;

        public string AppId 
        { 
            get => _appId;
            set => _appId = value;
        }
        
        
        public DeploymentWrapper(Deployment _deployment)
        {
            if (_deployment == null)
                return;

            this.PlanName = _deployment.PlanName;
            this.TransportType = _deployment.TransportType;
            this.RoomsPerProcess = _deployment.RoomsPerProcess;
            this._defaultContainerPortWrapper = new ContainerPortWrapper(_deployment.DefaultContainerPort);
            this.AdditionalContainerPorts = _deployment.AdditionalContainerPorts;
            this.CreatedAt = _deployment.CreatedAt;
            this.CreatedBy = _deployment.CreatedBy;
            this.RequestedMemoryMB = _deployment.RequestedMemoryMB;
            this.RequestedCPU = _deployment.RequestedCPU;
            this.DeploymentId = _deployment.DeploymentId;
            this.BuildId = _deployment.BuildId;
            this.AppId = _deployment.AppId;
            // Env = _deployment.Env;
        }

        public Deployment ToDeploymentType()
        {
            // (!) SDK constructor throws on req'd val == null
            
            Deployment deployment = null;
            try
            {
                deployment = new(
                    env: Env,
                    createdAt: this.CreatedAt,
                    createdBy: this.CreatedBy,
                    planName: this.PlanName,
                    transportType: this.TransportType,
                    roomsPerProcess: this.RoomsPerProcess,
                    additionalContainerPorts: this.AdditionalContainerPorts,
                    defaultContainerPort: this.DefaultContainerPort,
                    requestedMemoryMB: this.RequestedMemoryMB,
                    requestedCPU: this.RequestedCPU,
                    deploymentId: this.DeploymentId,
                    buildId: this.BuildId,
                    appId: this.AppId // Env = this.Env,
                );
            }
            catch (Exception e)
            {
                Debug.LogError($"Error: {e}");
                throw;
            }

            return deployment;
        }
    }
}
