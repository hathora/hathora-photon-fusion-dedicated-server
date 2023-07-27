// dylan@hathora.dev

using System;
using System.Collections.Generic;
using Hathora.Cloud.Sdk.Model;
using Hathora.Core.Scripts.Runtime.Common.Extensions;
using Hathora.Core.Scripts.Runtime.Common.Models;
using UnityEngine;
using UnityEngine.Serialization;

namespace Hathora.Core.Scripts.Runtime.Server.Models
{
    [Serializable]
    public class HathoraDeployAdvancedOpts
    {
        /// <summary>
        /// Perhaps you are curious *what* we're uploading via the
        /// `.hathora` temp dir; or want to edit the Dockerfile?
        /// </summary>
        [SerializeField] 
        private bool _keepTempDir;
        
        /// <summary>
        /// Perhaps you are curious *what* we're uploading via the
        /// `.hathora` temp dir; or want to edit the Dockerfile?
        /// </summary>
        public bool KeepTempDir
        {
            get => _keepTempDir;
            set => _keepTempDir = value;
        }
            
        /// <summary>
        /// In rare cases, you may want to provide multiple (up to 2 more) transports.
        /// Leave the nickname empty and we'll ignore this.
        /// Ensure the port differs from the others.
        /// </summary>
        [SerializeField] 
        private AdditionalContainerPortWrapper additionalTransportInfo1 = new();
        
        /// <summary>
        /// In rare cases, you may want to provide multiple (up to 2 more) transports.
        /// Leave the nickname empty and we'll ignore this.
        /// Ensure the port differs from the others.
        /// </summary>
        public AdditionalContainerPortWrapper AdditionalTransportInfo1
        {
            get => additionalTransportInfo1;
            set => additionalTransportInfo1 = value;
        }
        
        /// <summary>
        /// In rare cases, you may want to provide multiple (up to 2 more) transports.
        /// Leave the nickname empty and we'll ignore this.
        /// Ensure the port differs from the others.
        /// </summary>
        [SerializeField] 
        private AdditionalContainerPortWrapper additionalTransportInfo2 = new();
        
        /// <summary>
        /// In rare cases, you may want to provide multiple (up to 2 more) transports.
        /// Leave the nickname empty and we'll ignore this.
        /// Ensure the port differs from the others.
        /// </summary>
        public AdditionalContainerPortWrapper AdditionalTransportInfo2
        {
            get => additionalTransportInfo2;
            set => additionalTransportInfo2 = value;
        }
        

        /// <summary>Useful for CreateDeploymentAsync() via HathoraDeployOpts.</summary>
        public bool CheckHasAdditionalContainerPorts() =>
            !string.IsNullOrEmpty(additionalTransportInfo1?.TransportNickname) ||
            !string.IsNullOrEmpty(additionalTransportInfo2?.TransportNickname); 

        /// <summary>
        /// Returns a List of Hathora SDK ContainerPort.
        /// </summary>
        /// <returns>Returns an init'd but empty list, if nothing or invalid.</returns>
        public List<ContainerPort> GetExtraContainerPorts()
        {
            if (!CheckHasAdditionalContainerPorts())
                return null;
            
            List<ContainerPort> extraContainerPorts = new();
            
            if (checkIsValidExtraTransportInfo(additionalTransportInfo1))
                extraContainerPorts.AddIfNotNull(ParseToSdkContainerToPort(additionalTransportInfo1));
            
            if (checkIsValidExtraTransportInfo(additionalTransportInfo2))
                extraContainerPorts.AddIfNotNull(ParseToSdkContainerToPort(additionalTransportInfo2));

            return extraContainerPorts;
        }

        private bool checkIsValidExtraTransportInfo(AdditionalContainerPortWrapper containerPortWrapper) =>
            !string.IsNullOrEmpty(containerPortWrapper?.TransportNickname) &&
            containerPortWrapper.PortNumber != 7777;

        public ContainerPort ParseToSdkContainerToPort(AdditionalContainerPortWrapper containerPortWrapper)
        {
            if (containerPortWrapper == null)
                return null;
            
            return new ContainerPort(
                containerPortWrapper.TransportType,
                containerPortWrapper.PortNumber,
                containerPortWrapper.GetTransportNickname());
        }
    }
}
