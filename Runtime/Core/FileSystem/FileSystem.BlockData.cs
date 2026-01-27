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
        /// 块数据。
        /// </summary>
        [StructLayout(LayoutKind.Sequential)]
        private struct BlockData
        {
            public static readonly BlockData Empty = new BlockData(0, 0);

            private readonly int _stringIndex;
            private readonly int _clusterIndex;
            private readonly int _length;

            public BlockData(int clusterIndex, int length)
                : this(-1, clusterIndex, length)
            {
            }

            public BlockData(int stringIndex, int clusterIndex, int length)
            {
                _stringIndex = stringIndex;
                _clusterIndex = clusterIndex;
                _length = length;
            }

            public bool Using
            {
                get
                {
                    return _stringIndex >= 0;
                }
            }

            public int StringIndex
            {
                get
                {
                    return _stringIndex;
                }
            }

            public int ClusterIndex
            {
                get
                {
                    return _clusterIndex;
                }
            }

            public int Length
            {
                get
                {
                    return _length;
                }
            }

            public BlockData Free()
            {
                return new BlockData(_clusterIndex, (int)GetUpBoundClusterOffset(_length));
            }
        }
    }
}
