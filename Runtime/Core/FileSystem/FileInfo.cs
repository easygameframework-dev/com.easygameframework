//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Runtime.InteropServices;

namespace EasyGameFramework.Core.FileSystem
{
    /// <summary>
    /// 文件信息。
    /// </summary>
    [StructLayout(LayoutKind.Auto)]
    public struct FileInfo
    {
        private readonly string _name;
        private readonly long _offset;
        private readonly int _length;

        /// <summary>
        /// 初始化文件信息的新实例。
        /// </summary>
        /// <param name="name">文件名称。</param>
        /// <param name="offset">文件偏移。</param>
        /// <param name="length">文件长度。</param>
        public FileInfo(string name, long offset, int length)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0L)
            {
                throw new GameFrameworkException("Offset is invalid.");
            }

            if (length < 0)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            _name = name;
            _offset = offset;
            _length = length;
        }

        /// <summary>
        /// 获取文件信息是否有效。
        /// </summary>
        public bool IsValid
        {
            get
            {
                return !string.IsNullOrEmpty(_name) && _offset >= 0L && _length >= 0;
            }
        }

        /// <summary>
        /// 获取文件名称。
        /// </summary>
        public string Name
        {
            get
            {
                return _name;
            }
        }

        /// <summary>
        /// 获取文件偏移。
        /// </summary>
        public long Offset
        {
            get
            {
                return _offset;
            }
        }

        /// <summary>
        /// 获取文件长度。
        /// </summary>
        public int Length
        {
            get
            {
                return _length;
            }
        }
    }
}
