//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace EasyGameFramework.Editor
{
    internal sealed class HelperInfo<T> where T : MonoBehaviour
    {
        private const string CustomOptionName = "<Custom>";

        private readonly string _name;

        private SerializedProperty _helperTypeName;
        private SerializedProperty _customHelper;
        private string[] _helperTypeNames;
        private int _helperTypeNameIndex;

        public string HelperTypeNameFormat { get; set; } = "m_{0}HelperTypeName";
        public string CustomHelperFormat { get; set; } = "_custom{0}Helper";
        public string DisplayNameFormat { get; set; } = "{0} Helper";

        public HelperInfo(string name)
        {
            _name = name;

            _helperTypeName = null;
            _customHelper = null;
            _helperTypeNames = null;
            _helperTypeNameIndex = 0;
        }

        public void Init(SerializedObject serializedObject)
        {
            _helperTypeName = serializedObject.FindProperty(Utility.Text.Format(HelperTypeNameFormat, _name));
            _customHelper = serializedObject.FindProperty(Utility.Text.Format(CustomHelperFormat, _name));
        }

        public void Draw()
        {
            string displayName = FieldNameForDisplay(_name);
            int selectedIndex = EditorGUILayout.Popup(Utility.Text.Format(DisplayNameFormat, displayName), _helperTypeNameIndex, _helperTypeNames);
            if (selectedIndex != _helperTypeNameIndex)
            {
                _helperTypeNameIndex = selectedIndex;
                _helperTypeName.stringValue = selectedIndex <= 0 ? null : _helperTypeNames[selectedIndex];
            }

            if (_helperTypeNameIndex <= 0)
            {
                EditorGUILayout.PropertyField(_customHelper);
                if (_customHelper.objectReferenceValue == null)
                {
                    EditorGUILayout.HelpBox(Utility.Text.Format($"You must set Custom {DisplayNameFormat}.", displayName), MessageType.Error);
                }
            }
        }

        public void Refresh()
        {
            List<string> helperTypeNameList = new List<string>
            {
                CustomOptionName
            };

            helperTypeNameList.AddRange(Type.GetRuntimeTypeNames(typeof(T)));
            _helperTypeNames = helperTypeNameList.ToArray();

            _helperTypeNameIndex = 0;
            if (!string.IsNullOrEmpty(_helperTypeName.stringValue))
            {
                _helperTypeNameIndex = helperTypeNameList.IndexOf(_helperTypeName.stringValue);
                if (_helperTypeNameIndex <= 0)
                {
                    _helperTypeNameIndex = 0;
                    _helperTypeName.stringValue = null;
                }
            }
        }

        private string FieldNameForDisplay(string fieldName)
        {
            if (string.IsNullOrEmpty(fieldName))
            {
                return string.Empty;
            }

            string str = Regex.Replace(fieldName, @"^m_", string.Empty);
            str = Regex.Replace(str, @"((?<=[a-z])[A-Z]|[A-Z](?=[a-z]))", @" $1").TrimStart();
            return str;
        }
    }
}
