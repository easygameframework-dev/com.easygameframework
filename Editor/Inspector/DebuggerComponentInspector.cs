//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using UnityEditor;
using UnityEngine;
using EasyGameFramework;

namespace EasyGameFramework.Editor
{
    [CustomEditor(typeof(DebuggerComponent))]
    internal sealed class DebuggerComponentInspector : GameFrameworkInspector
    {
        private SerializedProperty _skin = null;
        private SerializedProperty _activeWindow = null;
        private SerializedProperty _showFullWindow = null;
        private SerializedProperty _consoleWindow = null;

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DebuggerComponent t = (DebuggerComponent)target;

            EditorGUILayout.PropertyField(_skin);

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                bool activeWindow = EditorGUILayout.Toggle("Active Window", t.ActiveWindow);
                if (activeWindow != t.ActiveWindow)
                {
                    t.ActiveWindow = activeWindow;
                }
            }
            else
            {
                EditorGUILayout.PropertyField(_activeWindow);
            }

            EditorGUILayout.PropertyField(_showFullWindow);

            if (EditorApplication.isPlaying)
            {
                if (GUILayout.Button("Reset Layout"))
                {
                    t.ResetLayout();
                }
            }

            EditorGUILayout.PropertyField(_consoleWindow, true);

            serializedObject.ApplyModifiedProperties();
        }

        private void OnEnable()
        {
            _skin = serializedObject.FindProperty("_skin");
            _activeWindow = serializedObject.FindProperty("_activeWindow");
            _showFullWindow = serializedObject.FindProperty("_showFullWindow");
            _consoleWindow = serializedObject.FindProperty("_consoleWindow");
        }
    }
}
