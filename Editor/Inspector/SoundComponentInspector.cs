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
    [CustomEditor(typeof(SoundComponent))]
    internal sealed class SoundComponentInspector : GameFrameworkInspector
    {
        // private SerializedProperty _enablePlaySoundUpdateEvent = null;
        // private SerializedProperty _enablePlaySoundDependencyAssetEvent = null;
        private SerializedProperty _instanceRoot = null;
        private SerializedProperty _audioMixer = null;
        private SerializedProperty _soundGroups = null;

        private HelperInfo<SoundHelperBase> _soundHelperInfo = new HelperInfo<SoundHelperBase>("Sound");
        private HelperInfo<SoundGroupHelperBase> _soundGroupHelperInfo = new HelperInfo<SoundGroupHelperBase>("SoundGroup");
        private HelperInfo<SoundAgentHelperBase> _soundAgentHelperInfo = new HelperInfo<SoundAgentHelperBase>("SoundAgent");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            SoundComponent t = (SoundComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                // EditorGUILayout.PropertyField(_enablePlaySoundUpdateEvent);
                // EditorGUILayout.PropertyField(_enablePlaySoundDependencyAssetEvent);
                EditorGUILayout.PropertyField(_instanceRoot);
                EditorGUILayout.PropertyField(_audioMixer);
                _soundHelperInfo.Draw();
                _soundGroupHelperInfo.Draw();
                _soundAgentHelperInfo.Draw();
                EditorGUILayout.PropertyField(_soundGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Sound Group Count", t.SoundGroupCount.ToString());
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
            // _enablePlaySoundUpdateEvent = serializedObject.FindProperty("_enablePlaySoundUpdateEvent");
            // _enablePlaySoundDependencyAssetEvent = serializedObject.FindProperty("_enablePlaySoundDependencyAssetEvent");
            _instanceRoot = serializedObject.FindProperty("_instanceRoot");
            _audioMixer = serializedObject.FindProperty("_audioMixer");
            _soundGroups = serializedObject.FindProperty("_soundGroups");

            _soundHelperInfo.Init(serializedObject);
            _soundGroupHelperInfo.Init(serializedObject);
            _soundAgentHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            _soundHelperInfo.Refresh();
            _soundGroupHelperInfo.Refresh();
            _soundAgentHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
