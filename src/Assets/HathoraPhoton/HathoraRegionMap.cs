// Created by dylan@hathora.dev

using System.Collections.Generic;
using System.Linq;
using Hathora.Core.Scripts.Runtime.Common.Extensions;
using UnityEngine;
using HathoraRegion = Hathora.Cloud.Sdk.Model.Region;

namespace HathoraPhoton
{
    /// <summary>
    /// To prevent confusion, maps do not override Photon's region whitelist.
    /// Instead, this is more so used for *creating* a game.
    /// </summary>
    public class HathoraRegionMap
    {
        private const HathoraRegion fallbackHathoraRegion = HathoraRegion.WashingtonDC;
        
        #region Region Map Info
        // ###################################
        // HATHORA REGIONS:
        // * 1 // Seattle, WA, USA
        // * 2 // WashingtonDC, USA
        // * 3 // Chicago, IL, USA
        // * 4 // London, England
        // * 5 // Frankfurt, Germany
        // * 6 // Mumbai, India
        // * 7 // Singapore
        // * 8 // Tokyo, Japan
        // * 9 // Sydney, Australia
        // * 10 // SaoPaulo, Brazil
            
        // PHOTON REGIONS:
        // * "asia" // Singapore
        // * "jp" // Tokyo, Japan
        // * "eu" // Amsterdam, Netherlands
        // * "sa" // SaoPaulo, Brazil
        // * "kr" // Seoul, South Korea
        // * "us" // WashingtonDC, USA
        // * "usw" // San José, CA, USA
        // ###################################
        #endregion // Region Map Info
    
        public static int GetHathoraRegionIndexFromPhoton(string _photonRegion) => 
            GetPhotonToHathoraRegionMap()[_photonRegion];

        public static HathoraRegion GetHathoraRegionEnumFromPhoton(string _photonRegion)
        {
            if (string.IsNullOrEmpty(_photonRegion))
            {
                Debug.Log("[HathoraRegionMap] GetHathoraRegionEnumFromPhoton: !_photonRegion; " +
                    $"returning fallback region: {fallbackHathoraRegion}");
                return fallbackHathoraRegion;
            }
            
            return (HathoraRegion)GetPhotonToHathoraRegionMap()[_photonRegion];
        }
        
        /// <summary>
        /// Photon uses implicit strings; Hathora uses 1-based enum.
        /// (!) Photon "asia" and "kr" regions are both mapped to Hathora "Singapore".
        /// </summary>
        public static Dictionary<string, int> GetPhotonToHathoraRegionMap() => new()
        {
            { "us", (int)HathoraRegion.WashingtonDC }, // WashingtonDC, (2) USA // Fallback
            { "usw", (int)HathoraRegion.Seattle }, // San Jose, CA, USA : (1) Seattle, WA, USA
            { "asia", (int)HathoraRegion.Singapore }, // (7) Singapore
            { "jp", (int)HathoraRegion.Tokyo }, // (8) Tokyo, Japan
            { "eu", (int)HathoraRegion.Frankfurt }, // Amsterdam, Netherlands : (5) Frankfurt, Germany
            { "sa", (int)HathoraRegion.SaoPaulo }, // (10) SaoPaulo, Brazil
            { "kr", (int)HathoraRegion.Singapore }, // Seoul, South Korea : (7) Singapore
        };

        /// <summary>
        /// Hathora uses 1-based enum; Photon uses implicit strings
        /// (!) Photon "asia" and "kr" regions are both mapped to Hathora "Singapore".
        /// </summary>
        public static Dictionary<int, string> GetHathoraToPhotonRegionMap()
        {
            // Reverse PhotonToHathoraRegionMap
            return GetPhotonToHathoraRegionMap().ToDictionary(photonToHathoraRegion => 
                photonToHathoraRegion.Value, 
                photonToHathoraRegion => photonToHathoraRegion.Key);
        }
        
        /// <summary>
        /// Eg: "WashingtonDC" -> "Washington DC". Useful for UI.
        /// </summary>
        /// <param name="_hathoraRegionId"></param>
        /// <returns></returns>
        public string GetFriendlyHathoraRegionName(int _hathoraRegionId)
        {
            HathoraRegion region = (HathoraRegion)_hathoraRegionId;
            return region.ToString().SplitPascalCase();
        }

        // /// TODO
        // /// <summary>
        // /// If you split it via SplitPascalCase() via GetFriendlyHathoraRegionName(), we'll revert it.
        // /// </summary>
        // /// <param name="_splitPascalCaseStr"></param>
        // /// <returns></returns>
        // public string GetHathoraRegionByFriendlyStr(string _splitPascalCaseStr)
        // {
        //     
        // }
    }
}
