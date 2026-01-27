//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Runtime.InteropServices;

namespace EasyGameFramework.Core.FileSystem
{
    internal sealed partial class FileSystem : IFileSystem
    {
        /// <summary>
        /// 字符串数据。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct StringData
        {
            private static readonly byte[] s_cachedBytes = new byte[byte.MaxValue + 1];

            private readonly byte _length;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = byte.MaxValue)]
            private readonly byte[] _bytes;

            public StringData(byte length, byte[] bytes)
            {
                _length = length;
                _bytes = bytes;
            }

            public string GetString(byte[] encryptBytes)
            {
                if (_length <= 0)
                {
                    return null;
                }

                Array.Copy(_bytes, 0, s_cachedBytes, 0, _length);
                Utility.Encryption.GetSelfXorBytes(s_cachedBytes, 0, _length, encryptBytes);
                return Utility.Converter.GetString(s_cachedBytes, 0, _length);
            }

            public StringData SetString(string value, byte[] encryptBytes)
            {
                if (string.IsNullOrEmpty(value))
                {
                    return Clear();
                }

                int length = Utility.Converter.GetBytes(value, s_cachedBytes);
                if (length > byte.MaxValue)
                {
                    throw new GameFrameworkException(Utility.Text.Format("String '{0}' is too long.", value));
                }

                Utility.Encryption.GetSelfXorBytes(s_cachedBytes, encryptBytes);
                Array.Copy(s_cachedBytes, 0, _bytes, 0, length);
                return new StringData((byte)length, _bytes);
            }

            public StringData Clear()
            {
                return new StringData(0, _bytes);
            }
        }
    }
}
