//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Entity
{
    internal sealed partial class EntityManager : GameFrameworkModule, IEntityManager
    {
        private sealed class ShowEntityInfo : IReference
        {
            private int _serialId;
            private int _entityId;
            private EntityGroup _entityGroup;
            private object _userData;

            public ShowEntityInfo()
            {
                _serialId = 0;
                _entityId = 0;
                _entityGroup = null;
                _userData = null;
            }

            public int SerialId
            {
                get
                {
                    return _serialId;
                }
            }

            public int EntityId
            {
                get
                {
                    return _entityId;
                }
            }

            public EntityGroup EntityGroup
            {
                get
                {
                    return _entityGroup;
                }
            }

            public object UserData
            {
                get
                {
                    return _userData;
                }
            }

            public static ShowEntityInfo Create(int serialId, int entityId, EntityGroup entityGroup, object userData)
            {
                ShowEntityInfo showEntityInfo = ReferencePool.Acquire<ShowEntityInfo>();
                showEntityInfo._serialId = serialId;
                showEntityInfo._entityId = entityId;
                showEntityInfo._entityGroup = entityGroup;
                showEntityInfo._userData = userData;
                return showEntityInfo;
            }

            public void Clear()
            {
                _serialId = 0;
                _entityId = 0;
                _entityGroup = null;
                _userData = null;
            }
        }
    }
}
