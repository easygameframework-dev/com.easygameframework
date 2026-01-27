//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Entity;
using UnityEditor;
using EasyGameFramework;

namespace EasyGameFramework.Editor
{
    [CustomEditor(typeof(EntityComponent))]
    internal sealed class EntityComponentInspector : GameFrameworkInspector
    {
        // private SerializedProperty _enableShowEntityUpdateEvent = null;
        // private SerializedProperty _enableShowEntityDependencyAssetEvent = null;
        private SerializedProperty _instanceRoot = null;
        private SerializedProperty _entityGroups = null;

        private HelperInfo<EntityHelperBase> _entityHelperInfo = new HelperInfo<EntityHelperBase>("Entity");
        private HelperInfo<EntityGroupHelperBase> _entityGroupHelperInfo = new HelperInfo<EntityGroupHelperBase>("EntityGroup");

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();

            serializedObject.Update();

            EntityComponent t = (EntityComponent)target;

            EditorGUI.BeginDisabledGroup(EditorApplication.isPlayingOrWillChangePlaymode);
            {
                // EditorGUILayout.PropertyField(_enableShowEntityUpdateEvent);
                // EditorGUILayout.PropertyField(_enableShowEntityDependencyAssetEvent);
                EditorGUILayout.PropertyField(_instanceRoot);
                _entityHelperInfo.Draw();
                _entityGroupHelperInfo.Draw();
                EditorGUILayout.PropertyField(_entityGroups, true);
            }
            EditorGUI.EndDisabledGroup();

            if (EditorApplication.isPlaying && IsPrefabInHierarchy(t.gameObject))
            {
                EditorGUILayout.LabelField("Entity Group Count", t.EntityGroupCount.ToString());
                EditorGUILayout.LabelField("Entity Count (Total)", t.EntityCount.ToString());
                IEntityGroup[] entityGroups = t.GetAllEntityGroups();
                foreach (IEntityGroup entityGroup in entityGroups)
                {
                    EditorGUILayout.LabelField(Utility.Text.Format("Entity Count ({0})", entityGroup.Name), entityGroup.EntityCount.ToString());
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
            // _enableShowEntityUpdateEvent = serializedObject.FindProperty("_enableShowEntityUpdateEvent");
            // _enableShowEntityDependencyAssetEvent = serializedObject.FindProperty("_enableShowEntityDependencyAssetEvent");
            _instanceRoot = serializedObject.FindProperty("_instanceRoot");
            _entityGroups = serializedObject.FindProperty("_entityGroups");

            _entityHelperInfo.Init(serializedObject);
            _entityGroupHelperInfo.Init(serializedObject);

            RefreshTypeNames();
        }

        private void RefreshTypeNames()
        {
            _entityHelperInfo.Refresh();
            _entityGroupHelperInfo.Refresh();
            serializedObject.ApplyModifiedProperties();
        }
    }
}
