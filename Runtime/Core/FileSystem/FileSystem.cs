//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;

namespace EasyGameFramework.Core.FileSystem
{
    /// <summary>
    /// 文件系统。
    /// </summary>
    internal sealed partial class FileSystem : IFileSystem
    {
        private const int ClusterSize = 1024 * 4;
        private const int CachedBytesLength = 0x1000;

        private static readonly string[] EmptyStringArray = new string[] { };
        private static readonly byte[] s_cachedBytes = new byte[CachedBytesLength];

        private static readonly int HeaderDataSize = Marshal.SizeOf(typeof(HeaderData));
        private static readonly int BlockDataSize = Marshal.SizeOf(typeof(BlockData));
        private static readonly int StringDataSize = Marshal.SizeOf(typeof(StringData));

        private readonly string _fullPath;
        private readonly FileSystemAccess _access;
        private readonly FileSystemStream _stream;
        private readonly Dictionary<string, int> _fileDatas;
        private readonly List<BlockData> _blockDatas;
        private readonly GameFrameworkMultiDictionary<int, int> _freeBlockIndexes;
        private readonly SortedDictionary<int, StringData> _stringDatas;
        private readonly Queue<int> _freeStringIndexes;
        private readonly Queue<StringData> _freeStringDatas;

        private HeaderData _headerData;
        private int _blockDataOffset;
        private int _stringDataOffset;
        private int _fileDataOffset;

        /// <summary>
        /// 初始化文件系统的新实例。
        /// </summary>
        /// <param name="fullPath">文件系统完整路径。</param>
        /// <param name="access">文件系统访问方式。</param>
        /// <param name="stream">文件系统流。</param>
        private FileSystem(string fullPath, FileSystemAccess access, FileSystemStream stream)
        {
            if (string.IsNullOrEmpty(fullPath))
            {
                throw new GameFrameworkException("Full path is invalid.");
            }

            if (access == FileSystemAccess.Unspecified)
            {
                throw new GameFrameworkException("Access is invalid.");
            }

            if (stream == null)
            {
                throw new GameFrameworkException("Stream is invalid.");
            }

            _fullPath = fullPath;
            _access = access;
            _stream = stream;
            _fileDatas = new Dictionary<string, int>(StringComparer.Ordinal);
            _blockDatas = new List<BlockData>();
            _freeBlockIndexes = new GameFrameworkMultiDictionary<int, int>();
            _stringDatas = new SortedDictionary<int, StringData>();
            _freeStringIndexes = new Queue<int>();
            _freeStringDatas = new Queue<StringData>();

            _headerData = default(HeaderData);
            _blockDataOffset = 0;
            _stringDataOffset = 0;
            _fileDataOffset = 0;

            Utility.Marshal.EnsureCachedHGlobalSize(CachedBytesLength);
        }

        /// <summary>
        /// 获取文件系统完整路径。
        /// </summary>
        public string FullPath
        {
            get
            {
                return _fullPath;
            }
        }

        /// <summary>
        /// 获取文件系统访问方式。
        /// </summary>
        public FileSystemAccess Access
        {
            get
            {
                return _access;
            }
        }

        /// <summary>
        /// 获取文件数量。
        /// </summary>
        public int FileCount
        {
            get
            {
                return _fileDatas.Count;
            }
        }

        /// <summary>
        /// 获取最大文件数量。
        /// </summary>
        public int MaxFileCount
        {
            get
            {
                return _headerData.MaxFileCount;
            }
        }

        /// <summary>
        /// 创建文件系统。
        /// </summary>
        /// <param name="fullPath">要创建的文件系统的完整路径。</param>
        /// <param name="access">要创建的文件系统的访问方式。</param>
        /// <param name="stream">要创建的文件系统的文件系统流。</param>
        /// <param name="maxFileCount">要创建的文件系统的最大文件数量。</param>
        /// <param name="maxBlockCount">要创建的文件系统的最大块数据数量。</param>
        /// <returns>创建的文件系统。</returns>
        public static FileSystem Create(string fullPath, FileSystemAccess access, FileSystemStream stream, int maxFileCount, int maxBlockCount)
        {
            if (maxFileCount <= 0)
            {
                throw new GameFrameworkException("Max file count is invalid.");
            }

            if (maxBlockCount <= 0)
            {
                throw new GameFrameworkException("Max block count is invalid.");
            }

            if (maxFileCount > maxBlockCount)
            {
                throw new GameFrameworkException("Max file count can not larger than max block count.");
            }

            FileSystem fileSystem = new FileSystem(fullPath, access, stream);
            fileSystem._headerData = new HeaderData(maxFileCount, maxBlockCount);
            CalcOffsets(fileSystem);
            Utility.Marshal.StructureToBytes(fileSystem._headerData, HeaderDataSize, s_cachedBytes);

            try
            {
                stream.Write(s_cachedBytes, 0, HeaderDataSize);
                stream.SetLength(fileSystem._fileDataOffset);
                return fileSystem;
            }
            catch
            {
                fileSystem.Shutdown();
                return null;
            }
        }

        /// <summary>
        /// 加载文件系统。
        /// </summary>
        /// <param name="fullPath">要加载的文件系统的完整路径。</param>
        /// <param name="access">要加载的文件系统的访问方式。</param>
        /// <param name="stream">要加载的文件系统的文件系统流。</param>
        /// <returns>加载的文件系统。</returns>
        public static FileSystem Load(string fullPath, FileSystemAccess access, FileSystemStream stream)
        {
            FileSystem fileSystem = new FileSystem(fullPath, access, stream);

            stream.Read(s_cachedBytes, 0, HeaderDataSize);
            fileSystem._headerData = Utility.Marshal.BytesToStructure<HeaderData>(HeaderDataSize, s_cachedBytes);
            if (!fileSystem._headerData.IsValid)
            {
                return null;
            }

            CalcOffsets(fileSystem);

            if (fileSystem._blockDatas.Capacity < fileSystem._headerData.BlockCount)
            {
                fileSystem._blockDatas.Capacity = fileSystem._headerData.BlockCount;
            }

            for (int i = 0; i < fileSystem._headerData.BlockCount; i++)
            {
                stream.Read(s_cachedBytes, 0, BlockDataSize);
                BlockData blockData = Utility.Marshal.BytesToStructure<BlockData>(BlockDataSize, s_cachedBytes);
                fileSystem._blockDatas.Add(blockData);
            }

            for (int i = 0; i < fileSystem._blockDatas.Count; i++)
            {
                BlockData blockData = fileSystem._blockDatas[i];
                if (blockData.Using)
                {
                    StringData stringData = fileSystem.ReadStringData(blockData.StringIndex);
                    fileSystem._stringDatas.Add(blockData.StringIndex, stringData);
                    fileSystem._fileDatas.Add(stringData.GetString(fileSystem._headerData.GetEncryptBytes()), i);
                }
                else
                {
                    fileSystem._freeBlockIndexes.Add(blockData.Length, i);
                }
            }

            int index = 0;
            foreach (KeyValuePair<int, StringData> i in fileSystem._stringDatas)
            {
                while (index < i.Key)
                {
                    fileSystem._freeStringIndexes.Enqueue(index++);
                }

                index++;
            }

            return fileSystem;
        }

        /// <summary>
        /// 关闭并清理文件系统。
        /// </summary>
        public void Shutdown()
        {
            _stream.Close();

            _fileDatas.Clear();
            _blockDatas.Clear();
            _freeBlockIndexes.Clear();
            _stringDatas.Clear();
            _freeStringIndexes.Clear();
            _freeStringDatas.Clear();

            _blockDataOffset = 0;
            _stringDataOffset = 0;
            _fileDataOffset = 0;
        }

        /// <summary>
        /// 获取文件信息。
        /// </summary>
        /// <param name="name">要获取文件信息的文件名称。</param>
        /// <returns>获取的文件信息。</returns>
        public FileInfo GetFileInfo(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            int blockIndex = 0;
            if (!_fileDatas.TryGetValue(name, out blockIndex))
            {
                return default(FileInfo);
            }

            BlockData blockData = _blockDatas[blockIndex];
            return new FileInfo(name, GetClusterOffset(blockData.ClusterIndex), blockData.Length);
        }

        /// <summary>
        /// 获取所有文件信息。
        /// </summary>
        /// <returns>获取的所有文件信息。</returns>
        public FileInfo[] GetAllFileInfos()
        {
            int index = 0;
            FileInfo[] results = new FileInfo[_fileDatas.Count];
            foreach (KeyValuePair<string, int> fileData in _fileDatas)
            {
                BlockData blockData = _blockDatas[fileData.Value];
                results[index++] = new FileInfo(fileData.Key, GetClusterOffset(blockData.ClusterIndex), blockData.Length);
            }

            return results;
        }

        /// <summary>
        /// 获取所有文件信息。
        /// </summary>
        /// <param name="results">获取的所有文件信息。</param>
        public void GetAllFileInfos(List<FileInfo> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, int> fileData in _fileDatas)
            {
                BlockData blockData = _blockDatas[fileData.Value];
                results.Add(new FileInfo(fileData.Key, GetClusterOffset(blockData.ClusterIndex), blockData.Length));
            }
        }

        /// <summary>
        /// 检查是否存在指定文件。
        /// </summary>
        /// <param name="name">要检查的文件名称。</param>
        /// <returns>是否存在指定文件。</returns>
        public bool HasFile(string name)
        {
            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            return _fileDatas.ContainsKey(name);
        }

        /// <summary>
        /// 读取指定文件。
        /// </summary>
        /// <param name="name">要读取的文件名称。</param>
        /// <returns>存储读取文件内容的二进制流。</returns>
        public byte[] ReadFile(string name)
        {
            if (_access != FileSystemAccess.Read && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return null;
            }

            int length = fileInfo.Length;
            byte[] buffer = new byte[length];
            if (length > 0)
            {
                _stream.Position = fileInfo.Offset;
                _stream.Read(buffer, 0, length);
            }

            return buffer;
        }

        /// <summary>
        /// 读取指定文件。
        /// </summary>
        /// <param name="name">要读取的文件名称。</param>
        /// <param name="buffer">存储读取文件内容的二进制流。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFile(string name, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return ReadFile(name, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 读取指定文件。
        /// </summary>
        /// <param name="name">要读取的文件名称。</param>
        /// <param name="buffer">存储读取文件内容的二进制流。</param>
        /// <param name="startIndex">存储读取文件内容的二进制流的起始位置。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFile(string name, byte[] buffer, int startIndex)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return ReadFile(name, buffer, startIndex, buffer.Length - startIndex);
        }

        /// <summary>
        /// 读取指定文件。
        /// </summary>
        /// <param name="name">要读取的文件名称。</param>
        /// <param name="buffer">存储读取文件内容的二进制流。</param>
        /// <param name="startIndex">存储读取文件内容的二进制流的起始位置。</param>
        /// <param name="length">存储读取文件内容的二进制流的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFile(string name, byte[] buffer, int startIndex, int length)
        {
            if (_access != FileSystemAccess.Read && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            if (startIndex < 0 || length < 0 || startIndex + length > buffer.Length)
            {
                throw new GameFrameworkException("Start index or length is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return 0;
            }

            _stream.Position = fileInfo.Offset;
            if (length > fileInfo.Length)
            {
                length = fileInfo.Length;
            }

            if (length > 0)
            {
                return _stream.Read(buffer, startIndex, length);
            }

            return 0;
        }

        /// <summary>
        /// 读取指定文件。
        /// </summary>
        /// <param name="name">要读取的文件名称。</param>
        /// <param name="stream">存储读取文件内容的二进制流。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFile(string name, Stream stream)
        {
            if (_access != FileSystemAccess.Read && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (stream == null)
            {
                throw new GameFrameworkException("Stream is invalid.");
            }

            if (!stream.CanWrite)
            {
                throw new GameFrameworkException("Stream is not writable.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return 0;
            }

            int length = fileInfo.Length;
            if (length > 0)
            {
                _stream.Position = fileInfo.Offset;
                return _stream.Read(stream, length);
            }

            return 0;
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>存储读取文件片段内容的二进制流。</returns>
        public byte[] ReadFileSegment(string name, int length)
        {
            return ReadFileSegment(name, 0, length);
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="offset">要读取片段的偏移。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>存储读取文件片段内容的二进制流。</returns>
        public byte[] ReadFileSegment(string name, int offset, int length)
        {
            if (_access != FileSystemAccess.Read && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0)
            {
                throw new GameFrameworkException("Index is invalid.");
            }

            if (length < 0)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return null;
            }

            if (offset > fileInfo.Length)
            {
                offset = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - offset;
            if (length > leftLength)
            {
                length = leftLength;
            }

            byte[] buffer = new byte[length];
            if (length > 0)
            {
                _stream.Position = fileInfo.Offset + offset;
                _stream.Read(buffer, 0, length);
            }

            return buffer;
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="buffer">存储读取文件片段内容的二进制流。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFileSegment(string name, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return ReadFileSegment(name, 0, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="buffer">存储读取文件片段内容的二进制流。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFileSegment(string name, byte[] buffer, int length)
        {
            return ReadFileSegment(name, 0, buffer, 0, length);
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="buffer">存储读取文件片段内容的二进制流。</param>
        /// <param name="startIndex">存储读取文件片段内容的二进制流的起始位置。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFileSegment(string name, byte[] buffer, int startIndex, int length)
        {
            return ReadFileSegment(name, 0, buffer, startIndex, length);
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="offset">要读取片段的偏移。</param>
        /// <param name="buffer">存储读取文件片段内容的二进制流。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFileSegment(string name, int offset, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return ReadFileSegment(name, offset, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="offset">要读取片段的偏移。</param>
        /// <param name="buffer">存储读取文件片段内容的二进制流。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFileSegment(string name, int offset, byte[] buffer, int length)
        {
            return ReadFileSegment(name, offset, buffer, 0, length);
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="offset">要读取片段的偏移。</param>
        /// <param name="buffer">存储读取文件片段内容的二进制流。</param>
        /// <param name="startIndex">存储读取文件片段内容的二进制流的起始位置。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFileSegment(string name, int offset, byte[] buffer, int startIndex, int length)
        {
            if (_access != FileSystemAccess.Read && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0)
            {
                throw new GameFrameworkException("Index is invalid.");
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            if (startIndex < 0 || length < 0 || startIndex + length > buffer.Length)
            {
                throw new GameFrameworkException("Start index or length is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return 0;
            }

            if (offset > fileInfo.Length)
            {
                offset = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - offset;
            if (length > leftLength)
            {
                length = leftLength;
            }

            if (length > 0)
            {
                _stream.Position = fileInfo.Offset + offset;
                return _stream.Read(buffer, startIndex, length);
            }

            return 0;
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="stream">存储读取文件片段内容的二进制流。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFileSegment(string name, Stream stream, int length)
        {
            return ReadFileSegment(name, 0, stream, length);
        }

        /// <summary>
        /// 读取指定文件的指定片段。
        /// </summary>
        /// <param name="name">要读取片段的文件名称。</param>
        /// <param name="offset">要读取片段的偏移。</param>
        /// <param name="stream">存储读取文件片段内容的二进制流。</param>
        /// <param name="length">要读取片段的长度。</param>
        /// <returns>实际读取了多少字节。</returns>
        public int ReadFileSegment(string name, int offset, Stream stream, int length)
        {
            if (_access != FileSystemAccess.Read && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (offset < 0)
            {
                throw new GameFrameworkException("Index is invalid.");
            }

            if (stream == null)
            {
                throw new GameFrameworkException("Stream is invalid.");
            }

            if (!stream.CanWrite)
            {
                throw new GameFrameworkException("Stream is not writable.");
            }

            if (length < 0)
            {
                throw new GameFrameworkException("Length is invalid.");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return 0;
            }

            if (offset > fileInfo.Length)
            {
                offset = fileInfo.Length;
            }

            int leftLength = fileInfo.Length - offset;
            if (length > leftLength)
            {
                length = leftLength;
            }

            if (length > 0)
            {
                _stream.Position = fileInfo.Offset + offset;
                return _stream.Read(stream, length);
            }

            return 0;
        }

        /// <summary>
        /// 写入指定文件。
        /// </summary>
        /// <param name="name">要写入的文件名称。</param>
        /// <param name="buffer">存储写入文件内容的二进制流。</param>
        /// <returns>是否写入指定文件成功。</returns>
        public bool WriteFile(string name, byte[] buffer)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return WriteFile(name, buffer, 0, buffer.Length);
        }

        /// <summary>
        /// 写入指定文件。
        /// </summary>
        /// <param name="name">要写入的文件名称。</param>
        /// <param name="buffer">存储写入文件内容的二进制流。</param>
        /// <param name="startIndex">存储写入文件内容的二进制流的起始位置。</param>
        /// <returns>是否写入指定文件成功。</returns>
        public bool WriteFile(string name, byte[] buffer, int startIndex)
        {
            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            return WriteFile(name, buffer, startIndex, buffer.Length - startIndex);
        }

        /// <summary>
        /// 写入指定文件。
        /// </summary>
        /// <param name="name">要写入的文件名称。</param>
        /// <param name="buffer">存储写入文件内容的二进制流。</param>
        /// <param name="startIndex">存储写入文件内容的二进制流的起始位置。</param>
        /// <param name="length">存储写入文件内容的二进制流的长度。</param>
        /// <returns>是否写入指定文件成功。</returns>
        public bool WriteFile(string name, byte[] buffer, int startIndex, int length)
        {
            if (_access != FileSystemAccess.Write && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not writable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (name.Length > byte.MaxValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name '{0}' is too long.", name));
            }

            if (buffer == null)
            {
                throw new GameFrameworkException("Buffer is invalid.");
            }

            if (startIndex < 0 || length < 0 || startIndex + length > buffer.Length)
            {
                throw new GameFrameworkException("Start index or length is invalid.");
            }

            bool hasFile = false;
            int oldBlockIndex = -1;
            if (_fileDatas.TryGetValue(name, out oldBlockIndex))
            {
                hasFile = true;
            }

            if (!hasFile && _fileDatas.Count >= _headerData.MaxFileCount)
            {
                return false;
            }

            int blockIndex = AllocBlock(length);
            if (blockIndex < 0)
            {
                return false;
            }

            if (length > 0)
            {
                _stream.Position = GetClusterOffset(_blockDatas[blockIndex].ClusterIndex);
                _stream.Write(buffer, startIndex, length);
            }

            ProcessWriteFile(name, hasFile, oldBlockIndex, blockIndex, length);
            _stream.Flush();
            return true;
        }

        /// <summary>
        /// 写入指定文件。
        /// </summary>
        /// <param name="name">要写入的文件名称。</param>
        /// <param name="stream">存储写入文件内容的二进制流。</param>
        /// <returns>是否写入指定文件成功。</returns>
        public bool WriteFile(string name, Stream stream)
        {
            if (_access != FileSystemAccess.Write && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not writable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (name.Length > byte.MaxValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("Name '{0}' is too long.", name));
            }

            if (stream == null)
            {
                throw new GameFrameworkException("Stream is invalid.");
            }

            if (!stream.CanRead)
            {
                throw new GameFrameworkException("Stream is not readable.");
            }

            bool hasFile = false;
            int oldBlockIndex = -1;
            if (_fileDatas.TryGetValue(name, out oldBlockIndex))
            {
                hasFile = true;
            }

            if (!hasFile && _fileDatas.Count >= _headerData.MaxFileCount)
            {
                return false;
            }

            int length = (int)(stream.Length - stream.Position);
            int blockIndex = AllocBlock(length);
            if (blockIndex < 0)
            {
                return false;
            }

            if (length > 0)
            {
                _stream.Position = GetClusterOffset(_blockDatas[blockIndex].ClusterIndex);
                _stream.Write(stream, length);
            }

            ProcessWriteFile(name, hasFile, oldBlockIndex, blockIndex, length);
            _stream.Flush();
            return true;
        }

        /// <summary>
        /// 写入指定文件。
        /// </summary>
        /// <param name="name">要写入的文件名称。</param>
        /// <param name="filePath">存储写入文件内容的文件路径。</param>
        /// <returns>是否写入指定文件成功。</returns>
        public bool WriteFile(string name, string filePath)
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new GameFrameworkException("File path is invalid");
            }

            if (!File.Exists(filePath))
            {
                return false;
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read))
            {
                return WriteFile(name, fileStream);
            }
        }

        /// <summary>
        /// 将指定文件另存为物理文件。
        /// </summary>
        /// <param name="name">要另存为的文件名称。</param>
        /// <param name="filePath">存储写入文件内容的文件路径。</param>
        /// <returns>是否将指定文件另存为物理文件成功。</returns>
        public bool SaveAsFile(string name, string filePath)
        {
            if (_access != FileSystemAccess.Read && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not readable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            if (string.IsNullOrEmpty(filePath))
            {
                throw new GameFrameworkException("File path is invalid");
            }

            FileInfo fileInfo = GetFileInfo(name);
            if (!fileInfo.IsValid)
            {
                return false;
            }

            if (File.Exists(filePath))
            {
                File.Delete(filePath);
            }

            string directory = Path.GetDirectoryName(filePath);
            if (!Directory.Exists(directory))
            {
                Directory.CreateDirectory(directory);
            }

            using (FileStream fileStream = new FileStream(filePath, FileMode.Create, FileAccess.Write, FileShare.None))
            {
                int length = fileInfo.Length;
                if (length > 0)
                {
                    _stream.Position = fileInfo.Offset;
                    return _stream.Read(fileStream, length) == length;
                }

                return true;
            }
        }

        /// <summary>
        /// 重命名指定文件。
        /// </summary>
        /// <param name="oldName">要重命名的文件名称。</param>
        /// <param name="newName">重命名后的文件名称。</param>
        /// <returns>是否重命名指定文件成功。</returns>
        public bool RenameFile(string oldName, string newName)
        {
            if (_access != FileSystemAccess.Write && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not writable.");
            }

            if (string.IsNullOrEmpty(oldName))
            {
                throw new GameFrameworkException("Old name is invalid.");
            }

            if (string.IsNullOrEmpty(newName))
            {
                throw new GameFrameworkException("New name is invalid.");
            }

            if (newName.Length > byte.MaxValue)
            {
                throw new GameFrameworkException(Utility.Text.Format("New name '{0}' is too long.", newName));
            }

            if (oldName == newName)
            {
                return true;
            }

            if (_fileDatas.ContainsKey(newName))
            {
                return false;
            }

            int blockIndex = 0;
            if (!_fileDatas.TryGetValue(oldName, out blockIndex))
            {
                return false;
            }

            int stringIndex = _blockDatas[blockIndex].StringIndex;
            StringData stringData = _stringDatas[stringIndex].SetString(newName, _headerData.GetEncryptBytes());
            _stringDatas[stringIndex] = stringData;
            WriteStringData(stringIndex, stringData);
            _fileDatas.Add(newName, blockIndex);
            _fileDatas.Remove(oldName);
            _stream.Flush();
            return true;
        }

        /// <summary>
        /// 删除指定文件。
        /// </summary>
        /// <param name="name">要删除的文件名称。</param>
        /// <returns>是否删除指定文件成功。</returns>
        public bool DeleteFile(string name)
        {
            if (_access != FileSystemAccess.Write && _access != FileSystemAccess.ReadWrite)
            {
                throw new GameFrameworkException("File system is not writable.");
            }

            if (string.IsNullOrEmpty(name))
            {
                throw new GameFrameworkException("Name is invalid.");
            }

            int blockIndex = 0;
            if (!_fileDatas.TryGetValue(name, out blockIndex))
            {
                return false;
            }

            _fileDatas.Remove(name);

            BlockData blockData = _blockDatas[blockIndex];
            int stringIndex = blockData.StringIndex;
            StringData stringData = _stringDatas[stringIndex].Clear();
            _freeStringIndexes.Enqueue(stringIndex);
            _freeStringDatas.Enqueue(stringData);
            _stringDatas.Remove(stringIndex);
            WriteStringData(stringIndex, stringData);

            blockData = blockData.Free();
            _blockDatas[blockIndex] = blockData;
            if (!TryCombineFreeBlocks(blockIndex))
            {
                _freeBlockIndexes.Add(blockData.Length, blockIndex);
                WriteBlockData(blockIndex);
            }

            _stream.Flush();
            return true;
        }

        private void ProcessWriteFile(string name, bool hasFile, int oldBlockIndex, int blockIndex, int length)
        {
            BlockData blockData = _blockDatas[blockIndex];
            if (hasFile)
            {
                BlockData oldBlockData = _blockDatas[oldBlockIndex];
                blockData = new BlockData(oldBlockData.StringIndex, blockData.ClusterIndex, length);
                _blockDatas[blockIndex] = blockData;
                WriteBlockData(blockIndex);

                oldBlockData = oldBlockData.Free();
                _blockDatas[oldBlockIndex] = oldBlockData;
                if (!TryCombineFreeBlocks(oldBlockIndex))
                {
                    _freeBlockIndexes.Add(oldBlockData.Length, oldBlockIndex);
                    WriteBlockData(oldBlockIndex);
                }
            }
            else
            {
                int stringIndex = AllocString(name);
                blockData = new BlockData(stringIndex, blockData.ClusterIndex, length);
                _blockDatas[blockIndex] = blockData;
                WriteBlockData(blockIndex);
            }

            if (hasFile)
            {
                _fileDatas[name] = blockIndex;
            }
            else
            {
                _fileDatas.Add(name, blockIndex);
            }
        }

        private bool TryCombineFreeBlocks(int freeBlockIndex)
        {
            BlockData freeBlockData = _blockDatas[freeBlockIndex];
            if (freeBlockData.Length <= 0)
            {
                return false;
            }

            int previousFreeBlockIndex = -1;
            int nextFreeBlockIndex = -1;
            int nextBlockDataClusterIndex = freeBlockData.ClusterIndex + GetUpBoundClusterCount(freeBlockData.Length);
            foreach (KeyValuePair<int, GameFrameworkLinkedListRange<int>> blockIndexes in _freeBlockIndexes)
            {
                if (blockIndexes.Key <= 0)
                {
                    continue;
                }

                int blockDataClusterCount = GetUpBoundClusterCount(blockIndexes.Key);
                foreach (int blockIndex in blockIndexes.Value)
                {
                    BlockData blockData = _blockDatas[blockIndex];
                    if (blockData.ClusterIndex + blockDataClusterCount == freeBlockData.ClusterIndex)
                    {
                        previousFreeBlockIndex = blockIndex;
                    }
                    else if (blockData.ClusterIndex == nextBlockDataClusterIndex)
                    {
                        nextFreeBlockIndex = blockIndex;
                    }
                }
            }

            if (previousFreeBlockIndex < 0 && nextFreeBlockIndex < 0)
            {
                return false;
            }

            _freeBlockIndexes.Remove(freeBlockData.Length, freeBlockIndex);
            if (previousFreeBlockIndex >= 0)
            {
                BlockData previousFreeBlockData = _blockDatas[previousFreeBlockIndex];
                _freeBlockIndexes.Remove(previousFreeBlockData.Length, previousFreeBlockIndex);
                freeBlockData = new BlockData(previousFreeBlockData.ClusterIndex, previousFreeBlockData.Length + freeBlockData.Length);
                _blockDatas[previousFreeBlockIndex] = BlockData.Empty;
                _freeBlockIndexes.Add(0, previousFreeBlockIndex);
                WriteBlockData(previousFreeBlockIndex);
            }

            if (nextFreeBlockIndex >= 0)
            {
                BlockData nextFreeBlockData = _blockDatas[nextFreeBlockIndex];
                _freeBlockIndexes.Remove(nextFreeBlockData.Length, nextFreeBlockIndex);
                freeBlockData = new BlockData(freeBlockData.ClusterIndex, freeBlockData.Length + nextFreeBlockData.Length);
                _blockDatas[nextFreeBlockIndex] = BlockData.Empty;
                _freeBlockIndexes.Add(0, nextFreeBlockIndex);
                WriteBlockData(nextFreeBlockIndex);
            }

            _blockDatas[freeBlockIndex] = freeBlockData;
            _freeBlockIndexes.Add(freeBlockData.Length, freeBlockIndex);
            WriteBlockData(freeBlockIndex);
            return true;
        }

        private int GetEmptyBlockIndex()
        {
            GameFrameworkLinkedListRange<int> lengthRange = default(GameFrameworkLinkedListRange<int>);
            if (_freeBlockIndexes.TryGetValue(0, out lengthRange))
            {
                int blockIndex = lengthRange.First.Value;
                _freeBlockIndexes.Remove(0, blockIndex);
                return blockIndex;
            }

            if (_blockDatas.Count < _headerData.MaxBlockCount)
            {
                int blockIndex = _blockDatas.Count;
                _blockDatas.Add(BlockData.Empty);
                WriteHeaderData();
                return blockIndex;
            }

            return -1;
        }

        private int AllocBlock(int length)
        {
            if (length <= 0)
            {
                return GetEmptyBlockIndex();
            }

            length = (int)GetUpBoundClusterOffset(length);

            int lengthFound = -1;
            GameFrameworkLinkedListRange<int> lengthRange = default(GameFrameworkLinkedListRange<int>);
            foreach (KeyValuePair<int, GameFrameworkLinkedListRange<int>> i in _freeBlockIndexes)
            {
                if (i.Key < length)
                {
                    continue;
                }

                if (lengthFound >= 0 && lengthFound < i.Key)
                {
                    continue;
                }

                lengthFound = i.Key;
                lengthRange = i.Value;
            }

            if (lengthFound >= 0)
            {
                if (lengthFound > length && _blockDatas.Count >= _headerData.MaxBlockCount)
                {
                    return -1;
                }

                int blockIndex = lengthRange.First.Value;
                _freeBlockIndexes.Remove(lengthFound, blockIndex);
                if (lengthFound > length)
                {
                    BlockData blockData = _blockDatas[blockIndex];
                    _blockDatas[blockIndex] = new BlockData(blockData.ClusterIndex, length);
                    WriteBlockData(blockIndex);

                    int deltaLength = lengthFound - length;
                    int anotherBlockIndex = GetEmptyBlockIndex();
                    _blockDatas[anotherBlockIndex] = new BlockData(blockData.ClusterIndex + GetUpBoundClusterCount(length), deltaLength);
                    _freeBlockIndexes.Add(deltaLength, anotherBlockIndex);
                    WriteBlockData(anotherBlockIndex);
                }

                return blockIndex;
            }
            else
            {
                int blockIndex = GetEmptyBlockIndex();
                if (blockIndex < 0)
                {
                    return -1;
                }

                long fileLength = _stream.Length;
                try
                {
                    _stream.SetLength(fileLength + length);
                }
                catch
                {
                    return -1;
                }

                _blockDatas[blockIndex] = new BlockData(GetUpBoundClusterCount(fileLength), length);
                WriteBlockData(blockIndex);
                return blockIndex;
            }
        }

        private int AllocString(string value)
        {
            int stringIndex = -1;
            StringData stringData = default(StringData);

            if (_freeStringIndexes.Count > 0)
            {
                stringIndex = _freeStringIndexes.Dequeue();
            }
            else
            {
                stringIndex = _stringDatas.Count;
            }

            if (_freeStringDatas.Count > 0)
            {
                stringData = _freeStringDatas.Dequeue();
            }
            else
            {
                byte[] bytes = new byte[byte.MaxValue];
                Utility.Random.GetRandomBytes(bytes);
                stringData = new StringData(0, bytes);
            }

            stringData = stringData.SetString(value, _headerData.GetEncryptBytes());
            _stringDatas.Add(stringIndex, stringData);
            WriteStringData(stringIndex, stringData);
            return stringIndex;
        }

        private void WriteHeaderData()
        {
            _headerData = _headerData.SetBlockCount(_blockDatas.Count);
            Utility.Marshal.StructureToBytes(_headerData, HeaderDataSize, s_cachedBytes);
            _stream.Position = 0L;
            _stream.Write(s_cachedBytes, 0, HeaderDataSize);
        }

        private void WriteBlockData(int blockIndex)
        {
            Utility.Marshal.StructureToBytes(_blockDatas[blockIndex], BlockDataSize, s_cachedBytes);
            _stream.Position = _blockDataOffset + BlockDataSize * blockIndex;
            _stream.Write(s_cachedBytes, 0, BlockDataSize);
        }

        private StringData ReadStringData(int stringIndex)
        {
            _stream.Position = _stringDataOffset + StringDataSize * stringIndex;
            _stream.Read(s_cachedBytes, 0, StringDataSize);
            return Utility.Marshal.BytesToStructure<StringData>(StringDataSize, s_cachedBytes);
        }

        private void WriteStringData(int stringIndex, StringData stringData)
        {
            Utility.Marshal.StructureToBytes(stringData, StringDataSize, s_cachedBytes);
            _stream.Position = _stringDataOffset + StringDataSize * stringIndex;
            _stream.Write(s_cachedBytes, 0, StringDataSize);
        }

        private static void CalcOffsets(FileSystem fileSystem)
        {
            fileSystem._blockDataOffset = HeaderDataSize;
            fileSystem._stringDataOffset = fileSystem._blockDataOffset + BlockDataSize * fileSystem._headerData.MaxBlockCount;
            fileSystem._fileDataOffset = (int)GetUpBoundClusterOffset(fileSystem._stringDataOffset + StringDataSize * fileSystem._headerData.MaxFileCount);
        }

        private static long GetUpBoundClusterOffset(long offset)
        {
            return (offset - 1L + ClusterSize) / ClusterSize * ClusterSize;
        }

        private static int GetUpBoundClusterCount(long length)
        {
            return (int)((length - 1L + ClusterSize) / ClusterSize);
        }

        private static long GetClusterOffset(int clusterIndex)
        {
            return (long)ClusterSize * clusterIndex;
        }
    }
}
