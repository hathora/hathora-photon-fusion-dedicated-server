// dylan@hathora.dev

using System;
using System.Collections.Generic;
using System.Text;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Common.Models;
using Hathora.Core.Scripts.Runtime.Common.Utils;
using UnityEngine;

namespace Hathora.Core.Scripts.Runtime.Server.Models
{
    [Serializable]
    public class HathoraDeployOpts
    {
        /// <summary>Default: 1. How many rooms do you want to support per server?</summary>
        [SerializeField]
        private int _roomsPerProcess = 1;

        /// <summary>Default: 1. How many rooms do you want to support per server?</summary>
        public int RoomsPerProcess
        {
            get => _roomsPerProcess;
            set => _roomsPerProcess = value;
        }

        /// <summary>(!) Hathora SDK Enums starts at index 1; not 0: Care of indexes</summary>
        [SerializeField]
        private int _planNameSelectedIndex = (int)PlanName.Tiny;

        /// <summary>(!) Hathora SDK Enums starts at index 1; not 0: Care of indexes</summary>
        public int PlanNameSelectedIndex
        {
            get => _planNameSelectedIndex;
            set => _planNameSelectedIndex = value;
        }

        /// <summary>(!) Hathora SDK Enums starts at index 1; not 0: Care of indexes</summary>
        public PlanName SelectedPlanName => 
            (PlanName)_planNameSelectedIndex;
        
        
        /// <summary>Default: Tiny. Billing Option: You only get charged for active rooms.</summary>
        [SerializeField]
        private ContainerPortWrapper _containerPortWrapper = new();

        /// <summary>Default: Tiny. Billing Option: You only get charged for active rooms.</summary>
        public ContainerPortWrapper ContainerPortWrapper
        {
            get => _containerPortWrapper;
            set => _containerPortWrapper = value;
        }
        

        /// <summary>(!) Hathora SDK Enums starts at index 1; not 0: Care of indexes</summary>
        public TransportType SelectedTransportType => 
            (TransportType)_transportTypeSelectedIndex;
        
        /// <summary>(!) Hathora SDK Enums starts at index 1; not 0: Care of indexes</summary>
        [SerializeField]
        private int _transportTypeSelectedIndex = (int)TransportType.Udp;
       
        /// <summary>(!) Hathora SDK Enums starts at index 1; not 0: Care of indexes</summary>
        public int TransportTypeSelectedIndex
        {
            get => _transportTypeSelectedIndex;
            set => _transportTypeSelectedIndex = value;
        }
        

        /// <summary>(!) Like an `.env` file, these are all strings.</summary>
        [SerializeField]
        private List<HathoraEnvVars> _envVars = new();

        /// <summary>(!) Like an `.env` file, these are all strings.</summary>
        public List<HathoraEnvVars> EnvVars
        {
            get => _envVars;
            set => _envVars = value;
        }

        /// <summary>You probably don't need to touch these, unless debugging</summary>
        [SerializeField]
        private HathoraDeployAdvancedOpts _advancedDeployOpts = new();

        /// <summary>You probably don't need to touch these, unless debugging</summary>
        public HathoraDeployAdvancedOpts AdvancedDeployOpts
        {
            get => _advancedDeployOpts;
            set => _advancedDeployOpts = value;
        }

        /// <summary>If you deployed something, we set the cached result</summary>
        // [SerializeField] // TODO: Make serializable. For now, this won't persist between Unity sessions.
        private DeploymentWrapper _lastDeployment;
        
        /// <summary>If you deployed something, we set the cached result</summary>
        public DeploymentWrapper LastDeployment
        {
            get => _lastDeployment;
            set => _lastDeployment = value;
        }
        
        private StringBuilder _lastDeployLogsStrb = new();
        public StringBuilder LastDeployLogsStrb
        {
            get => _lastDeployLogsStrb; 
            set => _lastDeployLogsStrb = value;
        }
        public bool HasLastDeployLogsStrb => 
            LastDeployLogsStrb?.Length > 0;
    }
}
