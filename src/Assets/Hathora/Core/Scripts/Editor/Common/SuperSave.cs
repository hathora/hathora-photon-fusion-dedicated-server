// Created by dylan@imperium42.com

using UnityEditor;
using UnityEngine;

namespace Hathora.Core.Scripts.Editor.Common
{
    /// <summary>
    /// ABOUT:
    /// CTRL+S often doesn't write ScriptableObjects to disk, due to Unity bugs (or lacking feature).
    /// This script replaces CTRL+S with additional save features for scenes/prefabs.
    ///
    /// USAGE:
    /// Drop this in an Editor dir. You may need to resolve a keybinding conflict for CTRL+S to
    /// prioritize this. Else, go to "File -> Super Save All".
    ///
    /// https://forum.unity.com/threads/editorscenemanager-scenesaved-example-no-code-examples-in-the-docs.488861/
    /// </summary>
    static class SuperSave
    {
        // ##########################################################
        // % (ctrl on Windows, cmd on macOS), # (shift), & (alt).
        // ##########################################################

        // ..........................................................................................
        [MenuItem("File/Super Save All %s", isValidateFunction:false, priority:0)] // ctrl+s
        public static void SaveAll()
        {
            // Save scenes + prefab instances
            EditorApplication.ExecuteMenuItem("File/Save");

            // Save dirty ScriptableOjects (.assets)
            AssetDatabase.SaveAssets();
            //EditorSceneManager.SaveOpenScenes();

            Debug.Log("[EditorSuperSave] Done!");
        }

    }
}
