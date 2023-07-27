// Created by dylan@hathora.dev

using Hathora.Core.Scripts.Runtime.Server;
using UnityEditor;

namespace Hathora.Core.Scripts.Editor.Server.ConfigStyle.PostAuth
{
    public class HathoraConfigPostAuthBodyDeployAdvUI : HathoraConfigUIBase
    {
        #region Vars
        // Foldouts
        private bool isAdvancedDeployFoldout;
        #endregion // Vars


        #region Init
        public HathoraConfigPostAuthBodyDeployAdvUI(
            HathoraServerConfig _serverConfig, 
            SerializedObject _serializedConfig)
            : base(_serverConfig, _serializedConfig)
        {
            if (!HathoraConfigUI.ENABLE_BODY_STYLE)
                return;
        }
        #endregion // Init
        
        
        #region UI Draw
        public void Draw()
        {
            if (!IsAuthed)
                return; // You should be calling HathoraConfigPreAuthBodyUI.Draw()

            insertAdvancedDeployFoldout();
            InsertSpace2x();
        }
       
        private void insertAdvancedDeployFoldout()
        {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            
            isAdvancedDeployFoldout = EditorGUILayout.Foldout(
                isAdvancedDeployFoldout, 
                "Advanced");

            if (!isAdvancedDeployFoldout)
            {
                EditorGUILayout.EndVertical(); // End of foldout box skin
                return;
            }
            
            InsertSpace2x();
            
            insertAdvancedDeployFoldoutComponents();
            
            EditorGUILayout.EndVertical(); // End of foldout box skin
        }

        private void insertAdvancedDeployFoldoutComponents()
        {
            BeginFieldIndent();

            // insertTODOHorizGroup(); // TODO

            EndFieldIndent();        }
        #endregion // UI Draw

        
        #region Event Logic
        // TODO
        #endregion // Event Logic
    }
}
