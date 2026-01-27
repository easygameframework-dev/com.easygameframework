//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using System;
using System.IO;
using System.Reflection;
using System.Text;
using UnityEditor;
using UnityEngine;
using EasyGameFramework;

namespace EasyGameFramework.Editor
{
    [CustomEditor(typeof(ResourceComponent))]
    internal sealed class ResourceComponentInspector : GameFrameworkInspector
    {
        private static readonly string[] _resourceModeNames = new string[]
        {
            "EditorSimulateMode (编辑器下的模拟模式)",
            "OfflinePlayMode (单机模式)",
            "HostPlayMode (联机运行模式)",
            "WebPlayMode (WebGL运行模式)"
        };

        private static readonly string[] _verifyLevelNames = new string[]
        {
            "Low (验证文件存在)",
            "Middle (验证文件大小)",
            "High (验证文件大小和CRC)"
        };

        private SerializedProperty _playMode = null;
        private SerializedProperty _defaultPackageName = null;
        // private SerializedProperty _updatableWhilePlaying = null;
        private SerializedProperty _fileVerifyLevel = null;
        private SerializedProperty _milliseconds = null;
        // private SerializedProperty _readWritePathType = null;
        private SerializedProperty _minUnloadUnusedAssetsInterval = null;
        private SerializedProperty _maxUnloadUnusedAssetsInterval = null;
        private SerializedProperty _useSystemUnloadUnusedAssets = null;
        private SerializedProperty _assetAutoReleaseInterval = null;
        private SerializedProperty _assetCapacity = null;
        private SerializedProperty _assetExpireTime = null;
        private SerializedProperty _assetPriority = null;
        private SerializedProperty _downloadingMaxNum = null;
        private SerializedProperty _failedTryAgain = null;
        private SerializedProperty _loadResourceAgentHelper = null;
        private SerializedProperty _resourceHelper = null;

        private int _resourceModeIndex = 0;
        private int _verifyIndex = 0;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            ResourceComponent t = (ResourceComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
                {
                    EditorGUILayout.EnumPopup("Resource Mode", t.PlayMode);
                    EditorGUILayout.EnumPopup("VerifyLevel", t.FileVerifyLevel);
                }
                else
                {
                    int selectedIndex = EditorGUILayout.Popup("Resource Mode", _resourceModeIndex, _resourceModeNames);
                    if (selectedIndex != _resourceModeIndex)
                    {
                        _resourceModeIndex = selectedIndex;
                        _playMode.enumValueIndex = selectedIndex;
                    }

                    int selectedVerifyIndex = EditorGUILayout.Popup("VerifyLevel", _verifyIndex, _verifyLevelNames);
                    if (selectedVerifyIndex != _verifyIndex)
                    {
                        _verifyIndex = selectedVerifyIndex;
                        _fileVerifyLevel.enumValueIndex = selectedVerifyIndex;
                    }
                }

                EditorGUILayout.PropertyField(_defaultPackageName);

                // _readWritePathType.enumValueIndex = (int)(ReadWritePathType)EditorGUILayout.EnumPopup("Read-Write Path Type", t.ReadWritePathType);
            }
            // EditorGUILayout.PropertyField(_updatableWhilePlaying);

            EditorGUI.EndDisabledGroup();

            int milliseconds = EditorGUILayout.DelayedIntField("Milliseconds", _milliseconds.intValue);
            if (milliseconds != _milliseconds.intValue)
            {
                _milliseconds.longValue = milliseconds;
            }

            EditorGUILayout.PropertyField(_useSystemUnloadUnusedAssets);

            float minUnloadUnusedAssetsInterval =
                EditorGUILayout.Slider("Min Unload Unused Assets Interval", _minUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (Math.Abs(minUnloadUnusedAssetsInterval - _minUnloadUnusedAssetsInterval.floatValue) > 0.01f)
            {
                _minUnloadUnusedAssetsInterval.floatValue = minUnloadUnusedAssetsInterval;
            }

            float maxUnloadUnusedAssetsInterval =
                EditorGUILayout.Slider("Max Unload Unused Assets Interval", _maxUnloadUnusedAssetsInterval.floatValue, 0f, 3600f);
            if (Math.Abs(maxUnloadUnusedAssetsInterval - _maxUnloadUnusedAssetsInterval.floatValue) > 0.01f)
            {
                _maxUnloadUnusedAssetsInterval.floatValue = maxUnloadUnusedAssetsInterval;
            }

            float downloadingMaxNum = EditorGUILayout.Slider("Max Downloading Num", _downloadingMaxNum.intValue, 1f, 48f);
            if (Math.Abs(downloadingMaxNum - _downloadingMaxNum.intValue) > 0.001f)
            {
                _downloadingMaxNum.intValue = (int)downloadingMaxNum;
            }

            float failedTryAgain = EditorGUILayout.Slider("Max FailedTryAgain Count", _failedTryAgain.intValue, 1f, 48f);
            if (Math.Abs(failedTryAgain - _failedTryAgain.intValue) > 0.001f)
            {
                _failedTryAgain.intValue = (int)failedTryAgain;
            }

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlaying);
            {
                float assetAutoReleaseInterval = EditorGUILayout.DelayedFloatField("Asset Auto Release Interval", _assetAutoReleaseInterval.floatValue);
                if (Math.Abs(assetAutoReleaseInterval - _assetAutoReleaseInterval.floatValue) > 0.01f)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetAutoReleaseInterval = assetAutoReleaseInterval;
                    }
                    else
                    {
                        _assetAutoReleaseInterval.floatValue = assetAutoReleaseInterval;
                    }
                }

                int assetCapacity = EditorGUILayout.DelayedIntField("Asset Capacity", _assetCapacity.intValue);
                if (assetCapacity != _assetCapacity.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetCapacity = assetCapacity;
                    }
                    else
                    {
                        _assetCapacity.intValue = assetCapacity;
                    }
                }

                float assetExpireTime = EditorGUILayout.DelayedFloatField("Asset Expire Time", _assetExpireTime.floatValue);
                if (Math.Abs(assetExpireTime - _assetExpireTime.floatValue) > 0.01f)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetExpireTime = assetExpireTime;
                    }
                    else
                    {
                        _assetExpireTime.floatValue = assetExpireTime;
                    }
                }

                int assetPriority = EditorGUILayout.DelayedIntField("Asset Priority", _assetPriority.intValue);
                if (assetPriority != _assetPriority.intValue)
                {
                    if (EditorApplication.isPlaying)
                    {
                        t.AssetPriority = assetPriority;
                    }
                    else
                    {
                        _assetPriority.intValue = assetPriority;
                    }
                }
            }

            EditorGUILayout.PropertyField(_resourceHelper);
            EditorGUILayout.PropertyField(_loadResourceAgentHelper);

            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject) && t.IsInitialized)
            {
                EditorGUILayout.LabelField("Unload Unused Assets",
                    Utility.Text.Format("{0:F2} / {1:F2}", t.LastUnloadUnusedAssetsOperationElapseSeconds, t.MaxUnloadUnusedAssetsInterval));
                EditorGUILayout.LabelField("Read-Only Path", t?.ReadOnlyPath?.ToString());
                EditorGUILayout.LabelField("Read-Write Path", t?.ReadWritePath?.ToString());
                // EditorGUILayout.LabelField("Applicable Game Version", t.ApplicableGameVersion ?? "<Unknwon>");
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
            _playMode = serializedObject.FindProperty("_playMode");
            _defaultPackageName = serializedObject.FindProperty("_defaultPackageName");
            // _updatableWhilePlaying = serializedObject.FindProperty("_updatableWhilePlaying");
            _fileVerifyLevel = serializedObject.FindProperty("_fileVerifyLevel");
            _milliseconds = serializedObject.FindProperty("_milliseconds");
            // _readWritePathType = serializedObject.FindProperty("_readWritePathType");
            _minUnloadUnusedAssetsInterval = serializedObject.FindProperty("_minUnloadUnusedAssetsInterval");
            _maxUnloadUnusedAssetsInterval = serializedObject.FindProperty("_maxUnloadUnusedAssetsInterval");
            _useSystemUnloadUnusedAssets = serializedObject.FindProperty("_useSystemUnloadUnusedAssets");
            _assetAutoReleaseInterval = serializedObject.FindProperty("_assetAutoReleaseInterval");
            _assetCapacity = serializedObject.FindProperty("_assetCapacity");
            _assetExpireTime = serializedObject.FindProperty("_assetExpireTime");
            _assetPriority = serializedObject.FindProperty("_assetPriority");
            _downloadingMaxNum = serializedObject.FindProperty("_downloadingMaxNum");
            _failedTryAgain = serializedObject.FindProperty("_failedTryAgain");
            _loadResourceAgentHelper = serializedObject.FindProperty("_loadResourceAgentHelper");
            _resourceHelper = serializedObject.FindProperty("_resourceHelper");

            RefreshModes();
            RefreshTypeNames();
        }

        private void RefreshModes()
        {
            _resourceModeIndex = _playMode.enumValueIndex > 0 ? _playMode.enumValueIndex : 0;
            _verifyIndex = _fileVerifyLevel.enumValueIndex > 0 ? _fileVerifyLevel.enumValueIndex : 0;
        }

        private void RefreshTypeNames()
        {
            serializedObject.ApplyModifiedProperties();
        }
    }
}
