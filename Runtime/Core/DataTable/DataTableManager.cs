//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Core.DataTable
{
    /// <summary>
    /// 数据表管理器。
    /// </summary>
    internal sealed partial class DataTableManager : GameFrameworkModule, IDataTableManager
    {
        private readonly Dictionary<TypeNamePair, DataTableBase> _dataTables;
        private IResourceManager _resourceManager;
        private IDataProviderHelper<DataTableBase> _dataProviderHelper;
        private IDataTableHelper _dataTableHelper;
        private IDataRowHelperResolver _dataRowHelperResolver;

        /// <summary>
        /// 初始化数据表管理器的新实例。
        /// </summary>
        public DataTableManager()
        {
            _dataTables = new Dictionary<TypeNamePair, DataTableBase>();
            _resourceManager = null;
            _dataProviderHelper = null;
            _dataTableHelper = null;
        }

        /// <summary>
        /// 获取数据表数量。
        /// </summary>
        public int Count
        {
            get
            {
                return _dataTables.Count;
            }
        }

        /// <summary>
        /// 获取缓冲二进制流的大小。
        /// </summary>
        public int CachedBytesSize
        {
            get
            {
                return DataProvider<DataTableBase>.CachedBytesSize;
            }
        }

        /// <summary>
        /// 数据表管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理数据表管理器。
        /// </summary>
        internal override void Shutdown()
        {
            foreach (KeyValuePair<TypeNamePair, DataTableBase> dataTable in _dataTables)
            {
                dataTable.Value.Shutdown();
            }

            _dataTables.Clear();
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            _resourceManager = resourceManager;
        }

        /// <summary>
        /// 设置数据表数据提供者辅助器。
        /// </summary>
        /// <param name="dataProviderHelper">数据表数据提供者辅助器。</param>
        public void SetDataProviderHelper(IDataProviderHelper<DataTableBase> dataProviderHelper)
        {
            if (dataProviderHelper == null)
            {
                throw new GameFrameworkException("Data provider helper is invalid.");
            }

            _dataProviderHelper = dataProviderHelper;
        }

        /// <summary>
        /// 设置数据表辅助器。
        /// </summary>
        /// <param name="dataTableHelper">数据表辅助器。</param>
        public void SetDataTableHelper(IDataTableHelper dataTableHelper)
        {
            if (dataTableHelper == null)
            {
                throw new GameFrameworkException("Data table helper is invalid.");
            }

            _dataTableHelper = dataTableHelper;
        }

        /// <summary>
        /// 设置数据表行辅助器解析器。
        /// </summary>
        /// <param name="dataRowHelperResolver">数据表行辅助器解析器。</param>
        public void SetDataRowHelperResolver(IDataRowHelperResolver dataRowHelperResolver)
        {
            if (dataRowHelperResolver == null)
            {
                throw new GameFrameworkException("Data row helper resolver is invalid.");
            }
            _dataRowHelperResolver = dataRowHelperResolver;
        }

        /// <summary>
        /// 确保二进制流缓存分配足够大小的内存并缓存。
        /// </summary>
        /// <param name="ensureSize">要确保二进制流缓存分配内存的大小。</param>
        public void EnsureCachedBytesSize(int ensureSize)
        {
            DataProvider<DataTableBase>.EnsureCachedBytesSize(ensureSize);
        }

        /// <summary>
        /// 释放缓存的二进制流。
        /// </summary>
        public void FreeCachedBytes()
        {
            DataProvider<DataTableBase>.FreeCachedBytes();
        }

        /// <summary>
        /// 是否存在数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        /// <returns>是否存在数据表。</returns>
        public bool HasDataTable<T>()
        {
            return InternalHasDataTable(new TypeNamePair(typeof(T)));
        }

        /// <summary>
        /// 是否存在数据表。
        /// </summary>
        /// <param name="dataRowType">数据表行的类型。</param>
        /// <returns>是否存在数据表。</returns>
        public bool HasDataTable(Type dataRowType)
        {
            if (dataRowType == null)
            {
                throw new GameFrameworkException("Data row type is invalid.");
            }

            // if (!typeof(IDataRow).IsAssignableFrom(dataRowType))
            // {
            //     throw new GameFrameworkException(Utility.Text.Format("Data row type '{0}' is invalid.", dataRowType.FullName));
            // }

            return InternalHasDataTable(new TypeNamePair(dataRowType));
        }

        /// <summary>
        /// 是否存在数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        /// <param name="name">数据表名称。</param>
        /// <returns>是否存在数据表。</returns>
        public bool HasDataTable<T>(string name)
        {
            return InternalHasDataTable(new TypeNamePair(typeof(T), name));
        }

        /// <summary>
        /// 是否存在数据表。
        /// </summary>
        /// <param name="dataRowType">数据表行的类型。</param>
        /// <param name="name">数据表名称。</param>
        /// <returns>是否存在数据表。</returns>
        public bool HasDataTable(Type dataRowType, string name)
        {
            if (dataRowType == null)
            {
                throw new GameFrameworkException("Data row type is invalid.");
            }

            // if (!typeof(IDataRow).IsAssignableFrom(dataRowType))
            // {
            //     throw new GameFrameworkException(Utility.Text.Format("Data row type '{0}' is invalid.", dataRowType.FullName));
            // }

            return InternalHasDataTable(new TypeNamePair(dataRowType, name));
        }

        /// <summary>
        /// 获取数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        /// <returns>要获取的数据表。</returns>
        public IDataTable<T> GetDataTable<T>()
        {
            return (IDataTable<T>)InternalGetDataTable(new TypeNamePair(typeof(T)));
        }

        /// <summary>
        /// 获取数据表。
        /// </summary>
        /// <param name="dataRowType">数据表行的类型。</param>
        /// <returns>要获取的数据表。</returns>
        public DataTableBase GetDataTable(Type dataRowType)
        {
            if (dataRowType == null)
            {
                throw new GameFrameworkException("Data row type is invalid.");
            }

            // if (!typeof(IDataRow).IsAssignableFrom(dataRowType))
            // {
            //     throw new GameFrameworkException(Utility.Text.Format("Data row type '{0}' is invalid.", dataRowType.FullName));
            // }

            return InternalGetDataTable(new TypeNamePair(dataRowType));
        }

        /// <summary>
        /// 获取数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        /// <param name="name">数据表名称。</param>
        /// <returns>要获取的数据表。</returns>
        public IDataTable<T> GetDataTable<T>(string name)
        {
            return (IDataTable<T>)InternalGetDataTable(new TypeNamePair(typeof(T), name));
        }

        /// <summary>
        /// 获取数据表。
        /// </summary>
        /// <param name="dataRowType">数据表行的类型。</param>
        /// <param name="name">数据表名称。</param>
        /// <returns>要获取的数据表。</returns>
        public DataTableBase GetDataTable(Type dataRowType, string name)
        {
            if (dataRowType == null)
            {
                throw new GameFrameworkException("Data row type is invalid.");
            }

            // if (!typeof(IDataRow).IsAssignableFrom(dataRowType))
            // {
            //     throw new GameFrameworkException(Utility.Text.Format("Data row type '{0}' is invalid.", dataRowType.FullName));
            // }

            return InternalGetDataTable(new TypeNamePair(dataRowType, name));
        }

        /// <summary>
        /// 获取所有数据表。
        /// </summary>
        /// <returns>所有数据表。</returns>
        public DataTableBase[] GetAllDataTables()
        {
            int index = 0;
            DataTableBase[] results = new DataTableBase[_dataTables.Count];
            foreach (KeyValuePair<TypeNamePair, DataTableBase> dataTable in _dataTables)
            {
                results[index++] = dataTable.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有数据表。
        /// </summary>
        /// <param name="results">所有数据表。</param>
        public void GetAllDataTables(List<DataTableBase> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<TypeNamePair, DataTableBase> dataTable in _dataTables)
            {
                results.Add(dataTable.Value);
            }
        }

        /// <summary>
        /// 创建数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        /// <returns>要创建的数据表。</returns>
        public IDataTable<T> CreateDataTable<T>()
        {
            return CreateDataTable<T>(string.Empty);
        }

        /// <summary>
        /// 创建数据表。
        /// </summary>
        /// <param name="dataRowType">数据表行的类型。</param>
        /// <returns>要创建的数据表。</returns>
        public DataTableBase CreateDataTable(Type dataRowType)
        {
            return CreateDataTable(dataRowType, string.Empty);
        }

        /// <summary>
        /// 创建数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        /// <param name="name">数据表名称。</param>
        /// <returns>要创建的数据表。</returns>
        public IDataTable<T> CreateDataTable<T>(string name)
        {
            if (_resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (_dataProviderHelper == null)
            {
                throw new GameFrameworkException("You must set data provider helper first.");
            }

            TypeNamePair typeNamePair = new TypeNamePair(typeof(T), name);
            if (HasDataTable<T>(name))
            {
                throw new GameFrameworkException(Utility.Text.Format("Already exist data table '{0}'.", typeNamePair));
            }

            DataTable<T> dataTable = new DataTable<T>(name, _dataRowHelperResolver.GetHelper<T>());
            dataTable.SetResourceManager(_resourceManager);
            dataTable.SetDataProviderHelper(_dataProviderHelper);
            _dataTables.Add(typeNamePair, dataTable);
            return dataTable;
        }

        /// <summary>
        /// 创建数据表。
        /// </summary>
        /// <param name="dataRowType">数据表行的类型。</param>
        /// <param name="name">数据表名称。</param>
        /// <returns>要创建的数据表。</returns>
        public DataTableBase CreateDataTable(Type dataRowType, string name)
        {
            if (_resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (_dataProviderHelper == null)
            {
                throw new GameFrameworkException("You must set data provider helper first.");
            }

            if (_dataRowHelperResolver == null)
            {
                throw new GameFrameworkException("You must set data row helper resolver first.");
            }

            TypeNamePair typeNamePair = new TypeNamePair(dataRowType, name);
            if (HasDataTable(dataRowType, name))
            {
                throw new GameFrameworkException(Utility.Text.Format("Already exist data table '{0}'.", typeNamePair));
            }

            Type dataTableType = typeof(DataTable<>).MakeGenericType(dataRowType);
            DataTableBase dataTable = (DataTableBase)Activator.CreateInstance(dataTableType, name, _dataRowHelperResolver.GetHelper(dataRowType));
            dataTable.SetResourceManager(_resourceManager);
            dataTable.SetDataProviderHelper(_dataProviderHelper);
            _dataTables.Add(typeNamePair, dataTable);
            return dataTable;
        }

        /// <summary>
        /// 销毁数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        public bool DestroyDataTable<T>()
        {
            return InternalDestroyDataTable(new TypeNamePair(typeof(T)));
        }

        /// <summary>
        /// 销毁数据表。
        /// </summary>
        /// <param name="dataRowType">数据表行的类型。</param>
        /// <returns>是否销毁数据表成功。</returns>
        public bool DestroyDataTable(Type dataRowType)
        {
            if (dataRowType == null)
            {
                throw new GameFrameworkException("Data row type is invalid.");
            }

            // if (!typeof(IDataRow).IsAssignableFrom(dataRowType))
            // {
            //     throw new GameFrameworkException(Utility.Text.Format("Data row type '{0}' is invalid.", dataRowType.FullName));
            // }

            return InternalDestroyDataTable(new TypeNamePair(dataRowType));
        }

        /// <summary>
        /// 销毁数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        /// <param name="name">数据表名称。</param>
        public bool DestroyDataTable<T>(string name)
        {
            return InternalDestroyDataTable(new TypeNamePair(typeof(T), name));
        }

        /// <summary>
        /// 销毁数据表。
        /// </summary>
        /// <param name="dataRowType">数据表行的类型。</param>
        /// <param name="name">数据表名称。</param>
        /// <returns>是否销毁数据表成功。</returns>
        public bool DestroyDataTable(Type dataRowType, string name)
        {
            if (dataRowType == null)
            {
                throw new GameFrameworkException("Data row type is invalid.");
            }

            // if (!typeof(IDataRow).IsAssignableFrom(dataRowType))
            // {
            //     throw new GameFrameworkException(Utility.Text.Format("Data row type '{0}' is invalid.", dataRowType.FullName));
            // }

            return InternalDestroyDataTable(new TypeNamePair(dataRowType, name));
        }

        /// <summary>
        /// 销毁数据表。
        /// </summary>
        /// <typeparam name="T">数据表行的类型。</typeparam>
        /// <param name="dataTable">要销毁的数据表。</param>
        /// <returns>是否销毁数据表成功。</returns>
        public bool DestroyDataTable<T>(IDataTable<T> dataTable)
        {
            if (dataTable == null)
            {
                throw new GameFrameworkException("Data table is invalid.");
            }

            return InternalDestroyDataTable(new TypeNamePair(typeof(T), dataTable.Name));
        }

        /// <summary>
        /// 销毁数据表。
        /// </summary>
        /// <param name="dataTable">要销毁的数据表。</param>
        /// <returns>是否销毁数据表成功。</returns>
        public bool DestroyDataTable(DataTableBase dataTable)
        {
            if (dataTable == null)
            {
                throw new GameFrameworkException("Data table is invalid.");
            }

            return InternalDestroyDataTable(new TypeNamePair(dataTable.Type, dataTable.Name));
        }

        private bool InternalHasDataTable(TypeNamePair typeNamePair)
        {
            return _dataTables.ContainsKey(typeNamePair);
        }

        private DataTableBase InternalGetDataTable(TypeNamePair typeNamePair)
        {
            DataTableBase dataTable = null;
            if (_dataTables.TryGetValue(typeNamePair, out dataTable))
            {
                return dataTable;
            }

            return null;
        }

        private bool InternalDestroyDataTable(TypeNamePair typeNamePair)
        {
            DataTableBase dataTable = null;
            if (_dataTables.TryGetValue(typeNamePair, out dataTable))
            {
                dataTable.Shutdown();
                return _dataTables.Remove(typeNamePair);
            }

            return false;
        }
    }
}
