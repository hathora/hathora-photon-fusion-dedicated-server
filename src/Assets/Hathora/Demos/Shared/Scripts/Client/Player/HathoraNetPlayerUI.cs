// Created by dylan@hathora.dev
using TMPro;
using UnityEngine;

namespace Hathora.Demos.Shared.Scripts.Client.Player
{
    public class HathoraNetPlayerUI : MonoBehaviour
    {
        [SerializeField]
        private TextMeshProUGUI playerInfoTxt;
        
        private const string HATHORA_VIOLET_COLOR_HEX = "#EEDDFF";
        static string headerBoldColorBegin => $"<b><color={HATHORA_VIOLET_COLOR_HEX}>";
        const string headerBoldColorEnd = "</color></b>";
        
        public void OnConnected(
            string _clientId,
            int _numClientsConnected)
        {
            playerInfoTxt.text =
                $"{headerBoldColorBegin}ClientId:{headerBoldColorEnd} {_clientId}\n" +
                $"{headerBoldColorBegin}NumClients:{headerBoldColorEnd} {_numClientsConnected}";
        }
    }
}
