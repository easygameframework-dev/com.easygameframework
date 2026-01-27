//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core.Resource;
using UnityEditor;
using UnityEngine;
using EasyGameFramework;

namespace EasyGameFramework.Editor
{
    [CustomEditor(typeof(SceneComponent))]
    internal sealed class SceneComponentInspector : GameFrameworkInspector
    {
        // private SerializedProperty _enableLoadSceneUpdateEvent = null;
        // private SerializedProperty _enableLoadSceneDependencyAssetEvent = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SceneComponent t = (SceneComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                // EditorGUILayout.PropertyField(_enableLoadSceneUpdateEvent);
                // EditorGUILayout.PropertyField(_enableLoadSceneDependencyAssetEvent);
            }
            EditorGUI.EndDisabledGroup();

            serializedObject.ApplyModifiedProperties();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Loaded Scene Asset Names", GetSceneNameString(t.GetLoadedSceneAssetAddresses()));
                EditorGUILayout.LabelField("Loading Scene Asset Names", GetSceneNameString(t.GetLoadingSceneAssetAddresses()));
                EditorGUILayout.LabelField("Unloading Scene Asset Names", GetSceneNameString(t.GetUnloadingSceneAssetAddresses()));
                EditorGUILayout.ObjectField("Main Camera", t.MainCamera, typeof(Camera), true);

                Repaint();
            }
        }

        private void OnEnable()
        {
            // _enableLoadSceneUpdateEvent = serializedObject.FindProperty("_enableLoadSceneUpdateEvent");
            // _enableLoadSceneDependencyAssetEvent = serializedObject.FindProperty("_enableLoadSceneDependencyAssetEvent");
        }

        private string GetSceneNameString(AssetAddress[] sceneAssetAddresses)
        {
            if (sceneAssetAddresses == null || sceneAssetAddresses.Length <= 0)
            {
                return "<Empty>";
            }

            string sceneNameString = string.Empty;
            foreach (AssetAddress sceneAssetAddress in sceneAssetAddresses)
            {
                if (!string.IsNullOrEmpty(sceneNameString))
                {
                    sceneNameString += ", ";
                }

                sceneNameString += ((SceneComponent)target).GetSceneName(sceneAssetAddress);
            }

            return sceneNameString;
        }
    }
}
