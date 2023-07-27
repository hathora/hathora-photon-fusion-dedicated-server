// Created by dylan@hathora.dev

using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace Hathora.Demos.Shared.Scripts.Common
{
    /// <summary>This scene only supports `-scene`</summary>
    public class HathoraMenuArgHandler : HathoraArgHandlerBase
    {
        #region Arg handler overrides
        protected override void ArgModeStartClient()
        {
            // base.ArgModeStartClient();
        }
    
        protected override void ArgModeStartServer()
        {
            // base.ArgModeStartServer();
        }
    
        protected override void ArgModeStartHost()
        {
            // base.ArgModeStartHost();
        }

        protected override void InitArgMemo(string _memoStr)
        {
            // base.InitArgMemo(_memoStr);
        }
        #endregion // Arg handler overrides
    }
}
