// Created by dylan@hathora.d.evv

using Hathora.Core.Scripts.Runtime.Server;
using UnityEngine;

namespace HathoraPhoton
{
    /// <summary>
    /// Implements HathoraServerMgrBase (Monobehaviour) for Photon.
    /// </summary>
    public class HathoraServerMgr : HathoraServerMgrBase
    {
        public static HathoraServerMgr Singleton { get; private set; }
    
        
        protected override void SetSingleton()
        {
            if (Singleton != null)
            {
                Debug.LogError("[HathoraClientPhotonMgr]**ERR @ setSingleton: Destroying dupe");
                Destroy(gameObject);
                return;
            }
            
            Singleton = this;
        }
    }
}
