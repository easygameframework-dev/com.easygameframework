//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;

namespace EasyGameFramework.Core.Entity
{
    internal sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        /// <summary>
        /// 实体信息。
        /// </summary>
        private sealed class EntityInfo : IReference
        {
            private IEntity _entity;
            private EntityStatus _status;
            private IEntity _parentEntity;
            private List<IEntity> _childEntities;

            public EntityInfo()
            {
                _entity = null;
                _status = EntityStatus.Unknown;
                _parentEntity = null;
                _childEntities = new List<IEntity>();
            }

            public IEntity Entity
            {
                get
                {
                    return _entity;
                }
            }

            public EntityStatus Status
            {
                get
                {
                    return _status;
                }
                set
                {
                    _status = value;
                }
            }

            public IEntity ParentEntity
            {
                get
                {
                    return _parentEntity;
                }
                set
                {
                    _parentEntity = value;
                }
            }

            public int ChildEntityCount
            {
                get
                {
                    return _childEntities.Count;
                }
            }

            public static EntityInfo Create(IEntity entity)
            {
                if (entity == null)
                {
                    throw new GameFrameworkException("Entity is invalid.");
                }

                EntityInfo entityInfo = ReferencePool.Acquire<EntityInfo>();
                entityInfo._entity = entity;
                entityInfo._status = EntityStatus.WillInit;
                return entityInfo;
            }

            public void Clear()
            {
                _entity = null;
                _status = EntityStatus.Unknown;
                _parentEntity = null;
                _childEntities.Clear();
            }

            public IEntity GetChildEntity()
            {
                return _childEntities.Count > 0 ? _childEntities[0] : null;
            }

            public IEntity[] GetChildEntities()
            {
                return _childEntities.ToArray();
            }

            public void GetChildEntities(List<IEntity> results)
            {
                if (results == null)
                {
                    throw new GameFrameworkException("Results is invalid.");
                }

                results.Clear();
                foreach (IEntity childEntity in _childEntities)
                {
                    results.Add(childEntity);
                }
            }

            public void AddChildEntity(IEntity childEntity)
            {
                if (_childEntities.Contains(childEntity))
                {
                    throw new GameFrameworkException("Can not add child entity which is already exist.");
                }

                _childEntities.Add(childEntity);
            }

            public void RemoveChildEntity(IEntity childEntity)
            {
                if (!_childEntities.Remove(childEntity))
                {
                    throw new GameFrameworkException("Can not remove child entity which is not exist.");
                }
            }
        }
    }
}
