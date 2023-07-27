// Created by dylan@hathora.dev

using System.Linq;
using Hathora.Core.Scripts.Runtime.Server;
using UnityEditor;

namespace Hathora.Core.Scripts.Editor.Server
{
    public class HathoraServerAssetPostprocessor : AssetPostprocessor
    {
        /// <summary>
        /// If we add a new ServerConfig, refresh the ServerConfig Finder window
        /// </summary>
        /// <param name="importedAssets"></param>
        /// <param name="deletedAssets"></param>
        /// <param name="movedAssets"></param>
        /// <param name="movedFromAssetPaths"></param>
        /// <returns></returns>
        private static void OnPostprocessAllAssets(
            string[] importedAssets,
            string[] deletedAssets,
            string[] movedAssets,
            string[] movedFromAssetPaths)
        {
            // Check if any of the imported, moved, or deleted assets are of the HathoraServerConfig type
            bool configChanged = importedAssets.Concat(movedAssets).Concat(deletedAssets).Any(
                assetPath => AssetDatabase.GetMainAssetTypeAtPath(assetPath) == typeof(HathoraServerConfig));

            if (configChanged)
            {
                // If a _serverConfig has changed, refresh the _serverConfig list and repaint the window
                HathoraServerConfigFinder.RefreshConfigsAndRepaint();
            }
        }
    }
}
