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
        #region Mock Testing
        [SerializeField]
        private bool mockArgsInEditor;
        
        /// <summary>Keys already include the `-` prefix.</summary>
        private static Dictionary<string, string> MOCK_ARGS_DICT = new()
        {
            {"-scene", "HathoraDemoScene-FishNet"}, // "HathoraDemoScene-FishNet" || "HathoraDemoScene-Mirror"
            {"-mode", "server"}, // "server" || "client" || "host
        };
        
        string MOCK_ARGS_DICT_STR = string.Join(" ", MOCK_ARGS_DICT.Select(kvp => 
            $"{kvp.Key} {kvp.Value}"));

        
        protected override async Task InitArgs(Dictionary<string, string> _cmdLineArgsOverrideList = null)
        {
            #if UNITY_EDITOR
            if (mockArgsInEditor)
            {
                Debug.LogWarning($"[HathoraMenuAregHandler] Init: " +
                    $"<color=yellow>MOCK_ARG_IN_EDITOR</color>: `{MOCK_ARGS_DICT_STR}`");
                
                await base.InitArgs(MOCK_ARGS_DICT);
                return;
            }
            #endif // UNITY_EDITOR
                
            base.InitArgs();
        }
        #endregion // Mock Testing

        
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
