//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEditor;
using EasyGameFramework;

namespace EasyGameFramework.Editor
{
    [CustomEditor(typeof(ConfigComponent))]
    internal sealed class ConfigComponentInspector : GameFrameworkInspector
    {
        // private SerializedProperty _enableLoadConfigUpdateEvent = null;
        // private SerializedProperty _enableLoadConfigDependencyAssetEvent = null;
        private SerializedProperty _cachedBytesSize = null;

        private HelperInfo<ConfigHelperBase> _configHelperInfo = new HelperInfo<ConfigHelperBase>("Config");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ConfigComponent t = (ConfigComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                // EditorGUILayout.PropertyField(_enableLoadConfigUpdateEvent);
                // EditorGUILayout.PropertyField(_enableLoadConfigDependencyAssetEvent);
                _configHelperInfo.Draw();
                EditorGUILayout.PropertyField(_cachedBytesSize);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Config Count", t.Count.ToString());
                EditorGUILayout.LabelField("Cached Bytes Size", t.CachedBytesSize.ToString());
            }

            serializedObject.ApplyModifiedProperties();

            Repaint();
        }

        protected override void OnCompileComplete()
        {
            base.OnCompileComplete();

            RefreshTypeNames();
        }

        private void OnEnable()
        {
            // _enableLoadConfigUpdateEvent = serializedObject.FindProperty("_enableLoadConfigUpdateEvent");
            // _enableLoadConfigDependencyAssetEvent = serializedObject.FindProperty("_enableLoadConfigDependencyAssetEvent");
            _cachedBytesSize = serializedObject.FindProperty("_cachedBytesSize");

            _configHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            _configHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
