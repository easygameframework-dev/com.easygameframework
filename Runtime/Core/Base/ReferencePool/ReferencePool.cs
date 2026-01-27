//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;

namespace EasyGameFramework.Core
{
    /// <summary>
    /// 引用池。
    /// </summary>
    public static partial class ReferencePool
    {
        private struct ReferenceCollectionFastGetter<T> where T : class, IReference
        {
            public static readonly ReferenceCollection Instance = GetReferenceCollection(typeof(T));
        }

        private static readonly Dictionary<Type, ReferenceCollection> s_referenceCollections = new Dictionary<Type, ReferenceCollection>();
        private static bool _enableStrictCheck = false;

        /// <summary>
        /// 获取或设置是否开启强制检查。
        /// </summary>
        public static bool EnableStrictCheck
        {
            get
            {
                return _enableStrictCheck;
            }
            set
            {
                _enableStrictCheck = value;
            }
        }

        /// <summary>
        /// 获取引用池的数量。
        /// </summary>
        public static int Count
        {
            get
            {
                return s_referenceCollections.Count;
            }
        }

        /// <summary>
        /// 获取所有引用池的信息。
        /// </summary>
        /// <returns>所有引用池的信息。</returns>
        public static ReferencePoolInfo[] GetAllReferencePoolInfos()
        {
            int index = 0;
            ReferencePoolInfo[] results = null;

            lock (s_referenceCollections)
            {
                results = new ReferencePoolInfo[s_referenceCollections.Count];
                foreach (KeyValuePair<Type, ReferenceCollection> referenceCollection in s_referenceCollections)
                {
                    results[index++] = new ReferencePoolInfo(referenceCollection.Key, referenceCollection.Value.UnusedReferenceCount, referenceCollection.Value.UsingReferenceCount, referenceCollection.Value.AcquireReferenceCount, referenceCollection.Value.ReleaseReferenceCount, referenceCollection.Value.AddReferenceCount, referenceCollection.Value.RemoveReferenceCount);
                }
            }

            return results;
        }

        /// <summary>
        /// 清除所有引用池。
        /// </summary>
        public static void ClearAll()
        {
            lock (s_referenceCollections)
            {
                foreach (KeyValuePair<Type, ReferenceCollection> referenceCollection in s_referenceCollections)
                {
                    referenceCollection.Value.RemoveAll();
                }

                s_referenceCollections.Clear();
            }
        }

        /// <summary>
        /// 从引用池获取引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <returns>引用。</returns>
        public static T Acquire<T>() where T : class, IReference, new()
        {
            return ReferenceCollectionFastGetter<T>.Instance.Acquire<T>();
            // return GetReferenceCollection(typeof(T)).Acquire<T>();
        }

        /// <summary>
        /// 从引用池获取引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <returns>引用。</returns>
        public static IReference Acquire(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            return GetReferenceCollection(referenceType).Acquire();
        }

        /// <summary>
        /// 将引用归还引用池。
        /// </summary>
        /// <param name="reference">引用。</param>
        public static void Release<T>(T reference) where T : class, IReference
        {
            if (reference == null)
            {
                throw new GameFrameworkException("Reference is invalid.");
            }

            if (typeof(T).IsAbstract || typeof(T).IsInterface)
            {
                InternalCheckReferenceType(reference.GetType());
            }
            ReferenceCollectionFastGetter<T>.Instance.Release(reference);
        }

        // /// <summary>
        // /// 将引用归还引用池。
        // /// </summary>
        // /// <param name="reference">引用。</param>
        // public static void Release(IReference reference)
        // {
        //     if (reference == null)
        //     {
        //         throw new GameFrameworkException("Reference is invalid.");
        //     }
        //
        //     Type referenceType = reference.GetType();
        //     InternalCheckReferenceType(referenceType);
        //     GetReferenceCollection(referenceType).Release(reference);
        // }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">追加数量。</param>
        public static void Add<T>(int count) where T : class, IReference, new()
        {
            ReferenceCollectionFastGetter<T>.Instance.Add<T>(count);
        }

        /// <summary>
        /// 向引用池中追加指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">追加数量。</param>
        public static void Add(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Add(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        /// <param name="count">移除数量。</param>
        public static void Remove<T>(int count) where T : class, IReference
        {
            ReferenceCollectionFastGetter<T>.Instance.Remove(count);
            // GetReferenceCollection(typeof(T)).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除指定数量的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        /// <param name="count">移除数量。</param>
        public static void Remove(Type referenceType, int count)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).Remove(count);
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <typeparam name="T">引用类型。</typeparam>
        public static void RemoveAll<T>() where T : class, IReference
        {
            ReferenceCollectionFastGetter<T>.Instance.RemoveAll();
        }

        /// <summary>
        /// 从引用池中移除所有的引用。
        /// </summary>
        /// <param name="referenceType">引用类型。</param>
        public static void RemoveAll(Type referenceType)
        {
            InternalCheckReferenceType(referenceType);
            GetReferenceCollection(referenceType).RemoveAll();
        }

        private static void InternalCheckReferenceType(Type referenceType)
        {
            if (!_enableStrictCheck)
            {
                return;
            }

            if (referenceType == null)
            {
                throw new GameFrameworkException("Reference type is invalid.");
            }

            if (!referenceType.IsClass || referenceType.IsAbstract)
            {
                throw new GameFrameworkException("Reference type is not a non-abstract class type.");
            }

            if (!typeof(IReference).IsAssignableFrom(referenceType))
            {
                throw new GameFrameworkException(Utility.Text.Format("Reference type '{0}' is invalid.", referenceType.FullName));
            }
        }

        private static ReferenceCollection GetReferenceCollection(Type referenceType)
        {
            if (referenceType == null)
            {
                throw new GameFrameworkException("ReferenceType is invalid.");
            }

            ReferenceCollection referenceCollection = null;
            lock (s_referenceCollections)
            {
                if (!s_referenceCollections.TryGetValue(referenceType, out referenceCollection))
                {
                    referenceCollection = new ReferenceCollection(referenceType);
                    s_referenceCollections.Add(referenceType, referenceCollection);
                }
            }

            return referenceCollection;
        }
    }
}
