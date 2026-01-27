//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace EasyGameFramework.Core.Config
{
    internal sealed partial class ConfigManager : GameFrameworkModule, IConfigManager
    {
        [StructLayout(LayoutKind.Auto)]
        private struct ConfigData
        {
            private readonly bool _boolValue;
            private readonly int _intValue;
            private readonly float _floatValue;
            private readonly string _stringValue;

            public ConfigData(bool boolValue, int intValue, float floatValue, string stringValue)
            {
                _boolValue = boolValue;
                _intValue = intValue;
                _floatValue = floatValue;
                _stringValue = stringValue;
            }

            public bool BoolValue
            {
                get
                {
                    return _boolValue;
                }
            }

            public int IntValue
            {
                get
                {
                    return _intValue;
                }
            }

            public float FloatValue
            {
                get
                {
                    return _floatValue;
                }
            }

            public string StringValue
            {
                get
                {
                    return _stringValue;
                }
            }
        }
    }
}
