// Created by dylan@hathora.dev

using System.Collections.Generic;
using System.Linq;
using Hathora.Core.Scripts.Editor.Common;
using Hathora.Core.Scripts.Runtime.Server;
using UnityEditor;
using UnityEngine;

namespace Hathora.Core.Scripts.Editor.Server
{
    [InitializeOnLoad]
    public class HathoraServerConfigFinder : EditorWindow
    {
        public static HathoraServerConfigFinder Instance { get; private set; }

        private const string ShowOnStartupKey = "HathoraServerConfigFinder.ShowOnStartup";
        private static List<HathoraServerConfig> serverConfigs;
        private static Vector2 scrollPos;
        private static GUIStyle richCenterTxtLabelStyle;
        private static GUIStyle richSmLeftTxtLabelStyle;

        
        #region Init
        /// <summary>Just run once on startup</summary>
        static HathoraServerConfigFinder()
        {
            EditorApplication.delayCall += ShowWindowOnStartup;
        }
        
        /// <summary>
        /// As soon as we see the window, we find all the configs and select the latest (if any).
        /// </summary>
        private void OnEnable()
        {
            findAllHathoraServerConfigs();
            AssetDatabase.importPackageCompleted += OnImportPackageCompleted;
        }
        
        private void OnDisable()
        {
            AssetDatabase.importPackageCompleted -= OnImportPackageCompleted;
        }

        /// <summary>
        /// We're looking for new _serverConfig files while the window is open
        /// As soon as we close the window, we unsubscribe from the event.
        /// </summary>
        /// <param name="_packagename"></param>
        private void OnImportPackageCompleted(string _packagename)
        {
            findAllHathoraServerConfigs();
            Repaint();
        }

        private static void InitStyles()
        {
            richCenterTxtLabelStyle ??= new GUIStyle(EditorStyles.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleCenter,
                wordWrap = true,
            };
            
            richSmLeftTxtLabelStyle ??= new GUIStyle(EditorStyles.label)
            {
                richText = true,
                alignment = TextAnchor.MiddleLeft,
                wordWrap = true,
                fontSize = 11,
            };
        }

        private static void ShowWindowOnStartup()
        {
            if (EditorPrefs.GetBool(ShowOnStartupKey, true))
                ShowWindowSelect1stFound();
        }

        [MenuItem("Hathora/ServerConfig Finder On Startup/Enable", false, -1001)]
        public static void EnableConfigFinderOnStartup()
        {
            EditorPrefs.SetBool(ShowOnStartupKey, true);
        }

        [MenuItem("Hathora/ServerConfig Finder On Startup/Disable", false, -1000)]
        public static void DisableConfigFinderOnStartup()
        {
            EditorPrefs.SetBool(ShowOnStartupKey, false);
        }
        #endregion // Init
        
        
        public static void RefreshConfigsAndRepaint()
        {
            if (Instance == null)
                return;
            
            Instance.findAllHathoraServerConfigs();
            Instance.Repaint();
        }
        

        [MenuItem("Hathora/Configuration (Server) _%#h", priority = -1000)] // Ctrl + Shift + H
        public static void ShowWindowSelect1stFound()
        {
            ShowWindowOnly();
            
            // Select the 1st one found
            selectHathoraServerConfig(serverConfigs[0]);
        }
        
        /// <summary>
        /// Does not select 1st found, unlike ShowWindowSelect1stFound().
        /// </summary>
        public static void ShowWindowOnly()
        {
            HathoraServerConfigFinder window = GetWindow<HathoraServerConfigFinder>(
                "Hathora Server Config Finder");
            
            window.minSize = new Vector2(x: 350, y: 255);
            window.maxSize = new Vector2(x: 600, y: 500);
            
            // Set the Instance property
            Instance = window;
            
            // Select the 1st one found
            selectHathoraServerConfig(serverConfigs[0]);
        }

        [MenuItem("Hathora/ServerConfig Finder On Startup/Enable", true)]
        public static bool ValidateEnableConfigFinderOnStartup()
        {
            return !EditorPrefs.GetBool(ShowOnStartupKey, true);
        }

        [MenuItem("Hathora/ServerConfig Finder On Startup/Disable", true)]
        public static bool ValidateDisableConfigFinderOnStartup()
        {
            return EditorPrefs.GetBool(ShowOnStartupKey, true);
        }

        private void OnGUI()
        {
            InitStyles();
            
            HathoraEditorUtils.InsertBanner(
                _includeVerticalGroup: false,
                _wrapperExtension: 24f); // Place banner @ top
            EditorGUILayout.Space(4f);
            HathoraEditorUtils.InsertHathoraSloganLbl();
            EditorGUILayout.EndVertical();
            
            insertDescrLbl();
            EditorGUILayout.Space(20f);

            scrollPos = EditorGUILayout.BeginScrollView(scrollPos);
    
            // Check if any serverConfigs are null with LINQ: If they are, findAllHathoraServerConfigs() to refresh cache.
            if (serverConfigs != null && serverConfigs.Any(config => config == null))
                findAllHathoraServerConfigs();

            insertRowForEachConfigFound();

            EditorGUILayout.Space(10f);
            insertNewConfigBtn();
            
            EditorGUILayout.Space(5);
        }

        private void insertDescrLbl()
        {
            string lblContent = "Your Hathora Configuration Files make integration " +
                "with Hathora Cloud seamless. They store your developer token and appId, " +
                "and will make it easy to configure, build and deploy your game server.";
            
            GUILayout.Label(lblContent, richSmLeftTxtLabelStyle);
        }

        private void insertNewConfigBtn()
        {
            // Add the "New ServerConfig" button
            if (GUILayout.Button("New ServerConfig", 
                    GUILayout.Height(20), 
                    GUILayout.MinWidth(215),
                    GUILayout.ExpandWidth(false)))
            {
                createAndSelectNewConfig();
            }
        }

        private void insertRowForEachConfigFound()
        {
            foreach (HathoraServerConfig config in serverConfigs)
            {
                EditorGUILayout.BeginVertical(GUI.skin.box);
                EditorGUILayout.BeginHorizontal();
                
                GUILayout.Label(config.name, GUILayout.ExpandWidth(true));

                if (GUILayout.Button("Select", GUILayout.Width(100)))
                    selectHathoraServerConfig(config);

                EditorGUILayout.EndHorizontal();
                EditorGUILayout.EndVertical();
                EditorGUILayout.Space(5);
            }

            EditorGUILayout.EndScrollView();
        }

        private void createAndSelectNewConfig()
        {
            string basePath = "Assets/Hathora";
            string fullPath = $"{basePath}/HathoraServerConfig.asset";

            // Create Hathora directory if it doesn't exist
            if (!AssetDatabase.IsValidFolder(basePath))
                AssetDatabase.CreateFolder("Assets", "Hathora");

            // Create a new instance of HathoraServerConfig
            HathoraServerConfig newConfig = ScriptableObject.CreateInstance<HathoraServerConfig>();

            // Save the new instance as an asset
            AssetDatabase.CreateAsset(newConfig, AssetDatabase.GenerateUniqueAssetPath(fullPath));

            // Refresh the AssetDatabase to ensure the new asset is properly saved and recognized
            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            // Select the newly created HathoraServerConfig in the Project tab
            selectHathoraServerConfig(newConfig);

            // Refresh the list of configs
            findAllHathoraServerConfigs();        
        }

        private void findAllHathoraServerConfigs()
        {
            string[] guids = AssetDatabase.FindAssets("t:Hathora.Scripts.Net.Server.HathoraServerConfig");
            serverConfigs = guids?.Select(guid => AssetDatabase.LoadAssetAtPath<HathoraServerConfig>(
                AssetDatabase.GUIDToAssetPath(guid))).ToList();
        }

        private static void selectHathoraServerConfig(HathoraServerConfig config)
        {
            EditorGUIUtility.PingObject(config);
            Selection.activeObject = config;
        }
    }
}
