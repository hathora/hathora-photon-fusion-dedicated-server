// Created by dylan@hathora.dev

using System.Collections.Generic;
using System.Linq;
using Hathora.Core.Scripts.Runtime.Common.Extensions;
using HathoraCloud.Models.Shared;
using UnityEngine;

namespace HathoraPhoton
{
    /// <summary>
    /// To prevent confusion, maps do not override Photon's region whitelist.
    /// Instead, this is more so useful for *dynamically creating* a game.
    /// </summary>
    public class RegionMap
    {
        private const Region fallbackRegion = Region.WashingtonDC;
        
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
    
        public static int GetRegionIndexFromPhoton(string _photonRegion) => 
            GetPhotonToRegionMap()[_photonRegion];

        public static Region GetRegionEnumFromPhoton(string _photonRegion)
        {
            if (string.IsNullOrEmpty(_photonRegion))
            {
                Debug.Log("[RegionMap] GetRegionEnumFromPhoton: !_photonRegion; " +
                    $"returning fallback region: {fallbackRegion}");
                return fallbackRegion;
            }
            
            return (Region)GetPhotonToRegionMap()[_photonRegion];
        }
        
        /// <summary>
        /// Photon uses implicit strings; Hathora uses 1-based enum.
        /// (!) Photon "asia" and "kr" regions are both mapped to Hathora "Singapore".
        /// </summary>
        public static Dictionary<string, int> GetPhotonToRegionMap() => new()
        {
            { "us", (int)Region.WashingtonDC }, // WashingtonDC, (2) USA // Fallback
            { "usw", (int)Region.Seattle }, // San Jose, CA, USA : (1) Seattle, WA, USA
            { "asia", (int)Region.Singapore }, // (7) Singapore
            { "jp", (int)Region.Tokyo }, // (8) Tokyo, Japan
            { "eu", (int)Region.Frankfurt }, // Amsterdam, Netherlands : (5) Frankfurt, Germany
            { "sa", (int)Region.SaoPaulo }, // (10) SaoPaulo, Brazil
            { "kr", (int)Region.Singapore }, // Seoul, South Korea : (7) Singapore
        };

        /// <summary>
        /// Hathora uses 1-based enum; Photon uses implicit strings
        /// (!) Photon "asia" and "kr" regions are both mapped to Hathora "Singapore".
        /// </summary>
        public static Dictionary<int, string> GetHathoraToPhotonRegionMap()
        {
            // Reverse PhotonToRegionMap
            return GetPhotonToRegionMap().ToDictionary(photonToRegion => 
                photonToRegion.Value, 
                photonToRegion => photonToRegion.Key);
        }
        
        /// <summary>
        /// Eg: "WashingtonDC" -> "Washington DC". Useful for UI.
        /// </summary>
        /// <param name="_RegionId"></param>
        /// <returns></returns>
        public string GetFriendlyRegionName(int _RegionId)
        {
            Region region = (Region)_RegionId;
            return region.ToString().SplitPascalCase();
        }

        // /// TODO
        // /// <summary>
        // /// If you split it via SplitPascalCase() via GetFriendlyRegionName(), we'll revert it.
        // /// </summary>
        // /// <param name="_splitPascalCaseStr"></param>
        // /// <returns></returns>
        // public string GetRegionByFriendlyStr(string _splitPascalCaseStr)
        // {
        //     
        // }
    }
}
