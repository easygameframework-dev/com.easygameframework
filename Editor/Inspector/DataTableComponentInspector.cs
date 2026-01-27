//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.DataTable;
using UnityEditor;
using EasyGameFramework;

namespace EasyGameFramework.Editor
{
    [CustomEditor(typeof(DataTableComponent))]
    internal sealed class DataTableComponentInspector : GameFrameworkInspector
    {
        // private SerializedProperty _enableLoadDataTableUpdateEvent = null;
        // private SerializedProperty _enableLoadDataTableDependencyAssetEvent = null;
        private SerializedProperty _cachedBytesSize = null;

        private HelperInfo<DataTableHelperBase> _dataTableHelperInfo = new HelperInfo<DataTableHelperBase>("DataTable");

        private HelperInfo<DataRowHelperResolverBase> _dataRowHelperResolverInfo =
            new HelperInfo<DataRowHelperResolverBase>("DataRowHelper")
            {
                HelperTypeNameFormat = "m_{0}ResolverTypeName",
                CustomHelperFormat = "_custom{0}Resolver",
                DisplayNameFormat = "{0} Resolver"
            };

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            DataTableComponent t = (DataTableComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                // EditorGUILayout.PropertyField(_enableLoadDataTableUpdateEvent);
                // EditorGUILayout.PropertyField(_enableLoadDataTableDependencyAssetEvent);
                _dataTableHelperInfo.Draw();
                _dataRowHelperResolverInfo.Draw();
                EditorGUILayout.PropertyField(_cachedBytesSize);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Data Table Count", t.Count.ToString());
                EditorGUILayout.LabelField("Cached Bytes Size", t.CachedBytesSize.ToString());

                DataTableBase[] dataTables = t.GetAllDataTables();
                foreach (DataTableBase dataTable in dataTables)
                {
                    DrawDataTable(dataTable);
                }
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
            // _enableLoadDataTableUpdateEvent = serializedObject.FindProperty("_enableLoadDataTableUpdateEvent");
            // _enableLoadDataTableDependencyAssetEvent = serializedObject.FindProperty("_enableLoadDataTableDependencyAssetEvent");
            _cachedBytesSize = serializedObject.FindProperty("_cachedBytesSize");

            _dataTableHelperInfo.Init(serializedObject);
            _dataRowHelperResolverInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void DrawDataTable(DataTableBase dataTable)
        {
            EditorGUILayout.LabelField(dataTable.FullName, Utility.Text.Format("{0} Rows", dataTable.Count));
        }

        private void RefreshTypeNames()
        {
            _dataTableHelperInfo.Refresh();
            _dataRowHelperResolverInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
