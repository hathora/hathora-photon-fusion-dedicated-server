using System.Threading.Tasks;
using Hathora.Core.Scripts.Runtime.Client.Models;
using Hathora.Demos.Shared.Scripts.Client.ClientMgr;
using UnityEngine;

namespace HathoraPhoton
{
    public class HathoraPhotonClientMgr : HathoraClientMgrBase
    {
        public static HathoraPhotonClientMgr Singleton { get; private set; }
       
        
        #region Base implementation
        protected override void OnAwake()
        {
            setSingleton();
        }

        protected override void OnStart()
        {
            base.OnStart();
            
#if UNITY_EDITOR || !UNITY_SERVER
            // We should only auto-login if we're not a standalone server
            _ = ConnectAsClient(); // Login passively; !await
#endif
        }

        public override async Task<bool> ConnectAsClient()
        {
            Debug.Log("[HathoraPhotonClientMgr] ConnectAsClient");
            
            AuthResult authResult = await ClientApis.ClientAuthApi.ClientAuthAsync();
            HathoraClientSession.InitNetSession(authResult?.PlayerAuthToken);
            
            return HathoraClientSession.IsAuthed;
        }

        public override Task StartServer()
        {
            throw new System.NotImplementedException();
        }

        public override Task StartClient()
        {
            throw new System.NotImplementedException();
        }
        
        public override Task StartHost()
        {
            throw new System.NotImplementedException();
        }

        public override Task StopHost()
        {
            throw new System.NotImplementedException();
        }

        public override Task StopServer()
        {
            throw new System.NotImplementedException();
        }

        public override Task StopClient()
        {
            throw new System.NotImplementedException();
        }
        #endregion // Base implementation
        
        
        protected virtual void setSingleton()
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
