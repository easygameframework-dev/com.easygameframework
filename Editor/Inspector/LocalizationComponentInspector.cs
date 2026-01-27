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
    [CustomEditor(typeof(LocalizationComponent))]
    internal sealed class LocalizationComponentInspector : GameFrameworkInspector
    {
        // private SerializedProperty _enableLoadDictionaryUpdateEvent = null;
        // private SerializedProperty _enableLoadDictionaryDependencyAssetEvent = null;
        private SerializedProperty _cachedBytesSize = null;

        private HelperInfo<LocalizationHelperBase> _localizationHelperInfo = new HelperInfo<LocalizationHelperBase>("Localization");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            LocalizationComponent t = (LocalizationComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                // EditorGUILayout.PropertyField(_enableLoadDictionaryUpdateEvent);
                // EditorGUILayout.PropertyField(_enableLoadDictionaryDependencyAssetEvent);
                _localizationHelperInfo.Draw();
                EditorGUILayout.PropertyField(_cachedBytesSize);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Language", t.Language.ToString());
                EditorGUILayout.LabelField("System Language", t.SystemLanguage.ToString());
                EditorGUILayout.LabelField("Dictionary Count", t.DictionaryCount.ToString());
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
            // _enableLoadDictionaryUpdateEvent = serializedObject.FindProperty("_enableLoadDictionaryUpdateEvent");
            // _enableLoadDictionaryDependencyAssetEvent = serializedObject.FindProperty("_enableLoadDictionaryDependencyAssetEvent");
            _cachedBytesSize = serializedObject.FindProperty("_cachedBytesSize");

            _localizationHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            _localizationHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
