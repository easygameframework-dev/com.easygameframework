//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace EasyGameFramework.Core.FileSystem
{
    internal sealed partial class FileSystem : IFileSystem
    {
        /// <summary>
        /// 头数据。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct HeaderData
        {
            private const int HeaderLength = 3;
            private const int FileSystemVersion = 0;
            private const int EncryptBytesLength = 4;
            private static readonly byte[] Header = new byte[HeaderLength] { (byte)'G', (byte)'F', (byte)'F' };

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = HeaderLength)]
            private readonly byte[] _header;

            private readonly byte _version;

            [MarshalAs(UnmanagedType.ByValArray, SizeConst = EncryptBytesLength)]
            private readonly byte[] _encryptBytes;

            private readonly int _maxFileCount;
            private readonly int _maxBlockCount;
            private readonly int _blockCount;

            public HeaderData(int maxFileCount, int maxBlockCount)
                : this(FileSystemVersion, new byte[EncryptBytesLength], maxFileCount, maxBlockCount, 0)
            {
                Utility.Random.GetRandomBytes(_encryptBytes);
            }

            public HeaderData(byte version, byte[] encryptBytes, int maxFileCount, int maxBlockCount, int blockCount)
            {
                _header = Header;
                _version = version;
                _encryptBytes = encryptBytes;
                _maxFileCount = maxFileCount;
                _maxBlockCount = maxBlockCount;
                _blockCount = blockCount;
            }

            public bool IsValid
            {
                get
                {
                    return _header.Length == HeaderLength && _header[0] == Header[0] && _header[1] == Header[1] && _header[2] == Header[2] && _version == FileSystemVersion && _encryptBytes.Length == EncryptBytesLength
                        && _maxFileCount > 0 && _maxBlockCount > 0 && _maxFileCount <= _maxBlockCount && _blockCount > 0 && _blockCount <= _maxBlockCount;
                }
            }

            public byte Version
            {
                get
                {
                    return _version;
                }
            }

            public int MaxFileCount
            {
                get
                {
                    return _maxFileCount;
                }
            }

            public int MaxBlockCount
            {
                get
                {
                    return _maxBlockCount;
                }
            }

            public int BlockCount
            {
                get
                {
                    return _blockCount;
                }
            }

            public byte[] GetEncryptBytes()
            {
                return _encryptBytes;
            }

            public HeaderData SetBlockCount(int blockCount)
            {
                return new HeaderData(_version, _encryptBytes, _maxFileCount, _maxBlockCount, blockCount);
            }
        }
    }
}
