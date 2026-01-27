//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using System;

namespace EasyGameFramework
{
    internal sealed class ShowEntityInfo : IReference
    {
        private Type _entityLogicType;
        private object _userData;

        public ShowEntityInfo()
        {
            _entityLogicType = null;
            _userData = null;
        }

        public Type EntityLogicType
        {
            get
            {
                return _entityLogicType;
            }
        }

        public object UserData
        {
            get
            {
                return _userData;
            }
        }

        public static ShowEntityInfo Create(Type entityLogicType, object userData)
        {
            ShowEntityInfo showEntityInfo = ReferencePool.Acquire<ShowEntityInfo>();
            showEntityInfo._entityLogicType = entityLogicType;
            showEntityInfo._userData = userData;
            return showEntityInfo;
        }

        public void Clear()
        {
            _entityLogicType = null;
            _userData = null;
        }
    }
}
