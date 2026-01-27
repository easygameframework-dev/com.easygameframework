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
    [CustomEditor(typeof(UIComponent))]
    internal sealed class UIComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty _enableOpenUIFormSuccessEvent = null;
        private SerializedProperty _enableOpenUIFormFailureEvent = null;
        // private SerializedProperty _enableOpenUIFormUpdateEvent = null;
        // private SerializedProperty _enableOpenUIFormDependencyAssetEvent = null;
        private SerializedProperty _enableCloseUIFormCompleteEvent = null;
        private SerializedProperty _instanceAutoReleaseInterval = null;
        private SerializedProperty _instanceCapacity = null;
        private SerializedProperty _instanceExpireTime = null;
        private SerializedProperty _instancePriority = null;
        private SerializedProperty _instanceRoot = null;
        private SerializedProperty _uIGroups = null;

        private HelperInfo<UIFormHelperBase> _uIFormHelperInfo = new HelperInfo<UIFormHelperBase>("UIForm");
        private HelperInfo<UIGroupHelperBase> _uIGroupHelperInfo = new HelperInfo<UIGroupHelperBase>("UIGroup");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            UIComponent t = (UIComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(_enableOpenUIFormSuccessEvent);
                EditorGUILayout.PropertyField(_enableOpenUIFormFailureEvent);
                // EditorGUILayout.PropertyField(_enableOpenUIFormUpdateEvent);
                // EditorGUILayout.PropertyField(_enableOpenUIFormDependencyAssetEvent);
                EditorGUILayout.PropertyField(_enableCloseUIFormCompleteEvent);
            }
            EditorGUI.EndDisabledGroup();

            float instanceAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Instance Auto Release Interval", _instanceAutoReleaseInterval.floatValue);
            if (instanceAutoReleaseInterval != _instanceAutoReleaseInterval.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceAutoReleaseInterval = instanceAutoReleaseInterval;
                }
                else
                {
                    _instanceAutoReleaseInterval.floatValue = instanceAutoReleaseInterval;
                }
            }

            int instanceCapacity = EditorGUILayout.DelayedIntField("Instance Capacity", _instanceCapacity.intValue);
            if (instanceCapacity != _instanceCapacity.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceCapacity = instanceCapacity;
                }
                else
                {
                    _instanceCapacity.intValue = instanceCapacity;
                }
            }

            float instanceExpireTime = EditorGUILayout.DelayedFloatField("Instance Expire Time", _instanceExpireTime.floatValue);
            if (instanceExpireTime != _instanceExpireTime.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstanceExpireTime = instanceExpireTime;
                }
                else
                {
                    _instanceExpireTime.floatValue = instanceExpireTime;
                }
            }

            int instancePriority = EditorGUILayout.DelayedIntField("Instance Priority", _instancePriority.intValue);
            if (instancePriority != _instancePriority.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.InstancePriority = instancePriority;
                }
                else
                {
                    _instancePriority.intValue = instancePriority;
                }
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(_instanceRoot);
                _uIFormHelperInfo.Draw();
                _uIGroupHelperInfo.Draw();
                EditorGUILayout.PropertyField(_uIGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("UI Group Count", t.UIGroupCount.ToString());
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
            _enableOpenUIFormSuccessEvent = serializedObject.FindProperty("_enableOpenUIFormSuccessEvent");
            _enableOpenUIFormFailureEvent = serializedObject.FindProperty("_enableOpenUIFormFailureEvent");
            // _enableOpenUIFormUpdateEvent = serializedObject.FindProperty("_enableOpenUIFormUpdateEvent");
            // _enableOpenUIFormDependencyAssetEvent = serializedObject.FindProperty("_enableOpenUIFormDependencyAssetEvent");
            _enableCloseUIFormCompleteEvent = serializedObject.FindProperty("_enableCloseUIFormCompleteEvent");
            _instanceAutoReleaseInterval = serializedObject.FindProperty("_instanceAutoReleaseInterval");
            _instanceCapacity = serializedObject.FindProperty("_instanceCapacity");
            _instanceExpireTime = serializedObject.FindProperty("_instanceExpireTime");
            _instancePriority = serializedObject.FindProperty("_instancePriority");
            _instanceRoot = serializedObject.FindProperty("_instanceRoot");
            _uIGroups = serializedObject.FindProperty("_uIGroups");

            _uIFormHelperInfo.Init(serializedObject);
            _uIGroupHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            _uIFormHelperInfo.Refresh();
            _uIGroupHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
