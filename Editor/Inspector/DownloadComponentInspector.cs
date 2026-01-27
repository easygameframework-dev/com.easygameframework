//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using System;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;
using EasyGameFramework;

namespace EasyGameFramework.Editor
{
    [CustomEditor(typeof(DownloadComponent))]
    internal sealed class DownloadComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty _instanceRoot = null;
        private SerializedProperty _downloadAgentHelperCount = null;
        private SerializedProperty _timeout = null;
        private SerializedProperty _flushSize = null;

        private HelperInfo<DownloadAgentHelperBase> _downloadAgentHelperInfo = new HelperInfo<DownloadAgentHelperBase>("DownloadAgent");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DownloadComponent t = (DownloadComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                EditorGUILayout.PropertyField(_instanceRoot);
                _downloadAgentHelperInfo.Draw();
                _downloadAgentHelperCount.intValue = EditorGUILayout.IntSlider("Download Agent Helper Count", _downloadAgentHelperCount.intValue, 1, 16);
            }
            EditorGUI.EndDisabledGroup();

            float timeout = EditorGUILayout.Slider("Timeout", _timeout.floatValue, 0f, 120f);
            if (timeout != _timeout.floatValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.Timeout = timeout;
                }
                else
                {
                    _timeout.floatValue = timeout;
                }
            }

            int flushSize = EditorGUILayout.DelayedIntField("Flush Size", _flushSize.intValue);
            if (flushSize != _flushSize.intValue)
            {
                if (EditorApplication.isPlaying)
                {
                    t.FlushSize = flushSize;
                }
                else
                {
                    _flushSize.intValue = flushSize;
                }
            }

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Paused", t.Paused.ToString());
                EditorGUILayout.LabelField("Total Agent Count", t.TotalAgentCount.ToString());
                EditorGUILayout.LabelField("Free Agent Count", t.FreeAgentCount.ToString());
                EditorGUILayout.LabelField("Working Agent Count", t.WorkingAgentCount.ToString());
                EditorGUILayout.LabelField("Waiting Agent Count", t.WaitingTaskCount.ToString());
                EditorGUILayout.LabelField("Current Speed", t.CurrentSpeed.ToString());
                EditorGUILayout.BeginVertical("box");
                {
                    TaskInfo[] downloadInfos = t.GetAllDownloadInfos();
                    if (downloadInfos.Length > 0)
                    {
                        foreach (TaskInfo downloadInfo in downloadInfos)
                        {
                            DrawDownloadInfo(downloadInfo);
                        }

                        if (GUILayout.Button("Export CSV Data"))
                        {
                            string exportFileName = EditorUtility.SaveFilePanel("Export CSV Data", string.Empty, "Download Task Data.csv", string.Empty);
                            if (!string.IsNullOrEmpty(exportFileName))
                            {
                                try
                                {
                                    int index = 0;
                                    string[] data = new string[downloadInfos.Length + 1];
                                    data[index++] = "Download Path,Serial Id,Tag,Priority,Status";
                                    foreach (TaskInfo downloadInfo in downloadInfos)
                                    {
                                        data[index++] = Utility.Text.Format("{0},{1},{2},{3},{4}", downloadInfo.Description, downloadInfo.SerialId, downloadInfo.Tag ?? string.Empty, downloadInfo.Priority, downloadInfo.Status);
                                    }

                                    File.WriteAllLines(exportFileName, data, Encoding.UTF8);
                                    Debug.Log(Utility.Text.Format("Export download task CSV data to '{0}' success.", exportFileName));
                                }
                                catch (Exception exception)
                                {
                                    Debug.LogError(Utility.Text.Format("Export download task CSV data to '{0}' failure, exception is '{1}'.", exportFileName, exception));
                                }
                            }
                        }
                    }
                    else
                    {
                        GUILayout.Label("Download Task is Empty ...");
                    }
                }
                EditorGUILayout.EndVertical();
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
            _instanceRoot = serializedObject.FindProperty("_instanceRoot");
            _downloadAgentHelperCount = serializedObject.FindProperty("_downloadAgentHelperCount");
            _timeout = serializedObject.FindProperty("_timeout");
            _flushSize = serializedObject.FindProperty("_flushSize");

            _downloadAgentHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void DrawDownloadInfo(TaskInfo downloadInfo)
        {
            EditorGUILayout.LabelField(downloadInfo.Description, Utility.Text.Format("[SerialId]{0} [Tag]{1} [Priority]{2} [Status]{3}", downloadInfo.SerialId, downloadInfo.Tag ?? "<None>", downloadInfo.Priority, downloadInfo.Status));
        }

        private void RefreshTypeNames()
        {
            _downloadAgentHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
