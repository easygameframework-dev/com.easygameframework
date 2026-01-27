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
    public static partial class ReferencePool
    {
        private sealed class ReferenceCollection
        {
            private readonly Queue<IReference> _references;
            private readonly HashSet<IReference> _usingReferences;
            private readonly Type _referenceType;
            private int _usingReferenceCount;
            private int _acquireReferenceCount;
            private int _releaseReferenceCount;
            private int _addReferenceCount;
            private int _removeReferenceCount;

            public ReferenceCollection(Type referenceType)
            {
                _references = new Queue<IReference>();
                _usingReferences = new HashSet<IReference>();
                _referenceType = referenceType;
                _usingReferenceCount = 0;
                _acquireReferenceCount = 0;
                _releaseReferenceCount = 0;
                _addReferenceCount = 0;
                _removeReferenceCount = 0;
            }

            public Type ReferenceType
            {
                get
                {
                    return _referenceType;
                }
            }

            public int UnusedReferenceCount
            {
                get
                {
                    return _references.Count;
                }
            }

            public int UsingReferenceCount
            {
                get
                {
                    return _usingReferenceCount;
                }
            }

            public int AcquireReferenceCount
            {
                get
                {
                    return _acquireReferenceCount;
                }
            }

            public int ReleaseReferenceCount
            {
                get
                {
                    return _releaseReferenceCount;
                }
            }

            public int AddReferenceCount
            {
                get
                {
                    return _addReferenceCount;
                }
            }

            public int RemoveReferenceCount
            {
                get
                {
                    return _removeReferenceCount;
                }
            }

            public T Acquire<T>() where T : class, IReference, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new GameFrameworkException("Type is invalid.");
                }

                _usingReferenceCount++;
                _acquireReferenceCount++;
                lock (_references)
                {
                    if (_references.Count > 0)
                    {
                        return (T)_references.Dequeue();
                    }
                }

                _addReferenceCount++;
                return new T();
            }

            public IReference Acquire()
            {
                var reference = AcquireImpl();
                if (_enableStrictCheck && !_usingReferences.Add(reference))
                {
                    throw new InvalidOperationException("Acquired a using reference.");
                }
                return reference;
            }

            private IReference AcquireImpl()
            {
                _usingReferenceCount++;
                _acquireReferenceCount++;
                lock (_references)
                {
                    if (_references.Count > 0)
                    {
                        return _references.Dequeue();
                    }
                }

                _addReferenceCount++;
                return (IReference)Activator.CreateInstance(_referenceType);
            }

            public void Release(IReference reference)
            {
                reference.Clear();
                lock (_references)
                {
                    if (_enableStrictCheck && _usingReferences.Remove(reference))
                    {
                        throw new GameFrameworkException("The reference has been released.");
                    }

                    _references.Enqueue(reference);
                }

                _releaseReferenceCount++;
                _usingReferenceCount--;
            }

            public void Add<T>(int count) where T : class, IReference, new()
            {
                if (typeof(T) != _referenceType)
                {
                    throw new GameFrameworkException("Type is invalid.");
                }

                lock (_references)
                {
                    _addReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Enqueue(new T());
                    }
                }
            }

            public void Add(int count)
            {
                lock (_references)
                {
                    _addReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Enqueue((IReference)Activator.CreateInstance(_referenceType));
                    }
                }
            }

            public void Remove(int count)
            {
                lock (_references)
                {
                    if (count > _references.Count)
                    {
                        count = _references.Count;
                    }

                    _removeReferenceCount += count;
                    while (count-- > 0)
                    {
                        _references.Dequeue();
                    }
                }
            }

            public void RemoveAll()
            {
                lock (_references)
                {
                    _removeReferenceCount += _references.Count;
                    _references.Clear();
                }
            }
        }
    }
}
