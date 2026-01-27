//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using System.Collections.Generic;
using System.IO;
using System.Xml;
using UnityEditor;
using UnityEngine;

namespace EasyGameFramework.Editor
{
    /// <summary>
    /// 构建配置相关的实用函数。
    /// </summary>
    internal static class BuildSettings
    {
        private static readonly string s_configurationPath = null;
        private static readonly List<string> s_defaultSceneNames = new List<string>();
        private static readonly List<string> s_searchScenePaths = new List<string>();

        static BuildSettings()
        {
            s_configurationPath = Type.GetConfigurationPath<BuildSettingsConfigPathAttribute>() ?? Utility.Path.GetRegularPath(Path.Combine(Application.dataPath, "GameFramework/Configs/BuildSettings.xml"));
            s_defaultSceneNames.Clear();
            s_searchScenePaths.Clear();

            if (!File.Exists(s_configurationPath))
            {
                return;
            }

            try
            {
                XmlDocument xmlDocument = new XmlDocument();
                xmlDocument.Load(s_configurationPath);
                XmlNode xmlRoot = xmlDocument.SelectSingleNode("UnityGameFramework");
                XmlNode xmlBuildSettings = xmlRoot.SelectSingleNode("BuildSettings");
                XmlNode xmlDefaultScenes = xmlBuildSettings.SelectSingleNode("DefaultScenes");
                XmlNode xmlSearchScenePaths = xmlBuildSettings.SelectSingleNode("SearchScenePaths");

                XmlNodeList xmlNodeList = null;
                XmlNode xmlNode = null;

                xmlNodeList = xmlDefaultScenes.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "DefaultScene")
                    {
                        continue;
                    }

                    string defaultSceneName = xmlNode.Attributes.GetNamedItem("Name").Value;
                    s_defaultSceneNames.Add(defaultSceneName);
                }

                xmlNodeList = xmlSearchScenePaths.ChildNodes;
                for (int i = 0; i < xmlNodeList.Count; i++)
                {
                    xmlNode = xmlNodeList.Item(i);
                    if (xmlNode.Name != "SearchScenePath")
                    {
                        continue;
                    }

                    string searchScenePath = xmlNode.Attributes.GetNamedItem("Path").Value;
                    s_searchScenePaths.Add(searchScenePath);
                }
            }
            catch
            {
            }
        }

        /// <summary>
        /// 将构建场景设置为默认。
        /// </summary>
        [MenuItem("Game Framework/Scenes in Build Settings/Default Scenes", false, 20)]
        public static void DefaultScenes()
        {
            HashSet<string> sceneNames = new HashSet<string>();
            foreach (string sceneName in s_defaultSceneNames)
            {
                sceneNames.Add(sceneName);
            }

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in sceneNames)
            {
                scenes.Add(new EditorBuildSettingsScene(sceneName, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("Set scenes of build settings to default scenes.");
        }

        /// <summary>
        /// 将构建场景设置为所有。
        /// </summary>
        [MenuItem("Game Framework/Scenes in Build Settings/All Scenes", false, 21)]
        public static void AllScenes()
        {
            HashSet<string> sceneNames = new HashSet<string>();
            foreach (string sceneName in s_defaultSceneNames)
            {
                sceneNames.Add(sceneName);
            }

            string[] sceneGuids = AssetDatabase.FindAssets("t:Scene", s_searchScenePaths.ToArray());
            foreach (string sceneGuid in sceneGuids)
            {
                string sceneName = AssetDatabase.GUIDToAssetPath(sceneGuid);
                sceneNames.Add(sceneName);
            }

            List<EditorBuildSettingsScene> scenes = new List<EditorBuildSettingsScene>();
            foreach (string sceneName in sceneNames)
            {
                scenes.Add(new EditorBuildSettingsScene(sceneName, true));
            }

            EditorBuildSettings.scenes = scenes.ToArray();

            Debug.Log("Set scenes of build settings to all scenes.");
        }
    }
}
