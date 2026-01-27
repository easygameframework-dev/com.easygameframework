//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Entity;
using EasyGameFramework.Core.Resource;
using System;
using UnityEngine;

namespace EasyGameFramework
{
    /// <summary>
    /// 实体。
    /// </summary>
    public sealed class Entity : MonoBehaviour, IEntity
    {
        private int _id;
        private AssetAddress _entityAssetAddress;
        private IEntityGroup _entityGroup;
        private EntityLogic _entityLogic;

        /// <summary>
        /// 获取实体编号。
        /// </summary>
        public int Id
        {
            get
            {
                return _id;
            }
        }

        /// <summary>
        /// 获取实体资源地址。
        /// </summary>
        public AssetAddress EntityAssetAddress
        {
            get
            {
                return _entityAssetAddress;
            }
        }

        /// <summary>
        /// 获取实体实例。
        /// </summary>
        public object Handle
        {
            get
            {
                return gameObject;
            }
        }

        /// <summary>
        /// 获取实体所属的实体组。
        /// </summary>
        public IEntityGroup EntityGroup
        {
            get
            {
                return _entityGroup;
            }
        }

        /// <summary>
        /// 获取实体逻辑。
        /// </summary>
        public EntityLogic Logic
        {
            get
            {
                return _entityLogic;
            }
        }

        /// <summary>
        /// 实体初始化。
        /// </summary>
        /// <param name="entityId">实体编号。</param>
        /// <param name="entityAssetAddress">实体资源地址。</param>
        /// <param name="entityGroup">实体所属的实体组。</param>
        /// <param name="isNewInstance">是否是新实例。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnInit(int entityId, AssetAddress entityAssetAddress, IEntityGroup entityGroup, bool isNewInstance, object userData)
        {
            _id = entityId;
            _entityAssetAddress = entityAssetAddress;
            if (isNewInstance)
            {
                _entityGroup = entityGroup;
            }
            else if (_entityGroup != entityGroup)
            {
                Log.Error("Entity group is inconsistent for non-new-instance entity.");
                return;
            }

            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            Type entityLogicType = showEntityInfo.EntityLogicType;
            if (entityLogicType == null)
            {
                Log.Error("Entity logic type is invalid.");
                return;
            }

            if (_entityLogic != null)
            {
                if (_entityLogic.GetType() == entityLogicType)
                {
                    _entityLogic.enabled = true;
                    return;
                }

                Destroy(_entityLogic);
                _entityLogic = null;
            }

            _entityLogic = gameObject.GetOrAddComponent(entityLogicType) as EntityLogic;
            if (_entityLogic == null)
            {
                Log.Error("Entity '{0}' can not add entity logic.", entityAssetAddress);
                return;
            }

            try
            {
                _entityLogic.OnInit(showEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnInit with exception '{2}'.", _id, _entityAssetAddress, exception);
            }
        }

        /// <summary>
        /// 实体回收。
        /// </summary>
        public void OnRecycle()
        {
            try
            {
                _entityLogic.OnRecycle();
                _entityLogic.enabled = false;
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnRecycle with exception '{2}'.", _id, _entityAssetAddress, exception);
            }

            _id = 0;
        }

        /// <summary>
        /// 实体显示。
        /// </summary>
        /// <param name="userData">用户自定义数据。</param>
        public void OnShow(object userData)
        {
            ShowEntityInfo showEntityInfo = (ShowEntityInfo)userData;
            try
            {
                _entityLogic.OnShow(showEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnShow with exception '{2}'.", _id, _entityAssetAddress, exception);
            }
        }

        /// <summary>
        /// 实体隐藏。
        /// </summary>
        /// <param name="isShutdown">是否是关闭实体管理器时触发。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnHide(bool isShutdown, object userData)
        {
            try
            {
                _entityLogic.OnHide(isShutdown, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnHide with exception '{2}'.", _id, _entityAssetAddress, exception);
            }
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="childEntity">附加的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnAttached(IEntity childEntity, object userData)
        {
            AttachEntityInfo attachEntityInfo = (AttachEntityInfo)userData;
            try
            {
                _entityLogic.OnAttached(((Entity)childEntity).Logic, attachEntityInfo.ParentTransform, attachEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnAttached with exception '{2}'.", _id, _entityAssetAddress, exception);
            }
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="childEntity">解除的子实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnDetached(IEntity childEntity, object userData)
        {
            try
            {
                _entityLogic.OnDetached(((Entity)childEntity).Logic, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnDetached with exception '{2}'.", _id, _entityAssetAddress, exception);
            }
        }

        /// <summary>
        /// 实体附加子实体。
        /// </summary>
        /// <param name="parentEntity">被附加的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnAttachTo(IEntity parentEntity, object userData)
        {
            AttachEntityInfo attachEntityInfo = (AttachEntityInfo)userData;
            try
            {
                _entityLogic.OnAttachTo(((Entity)parentEntity).Logic, attachEntityInfo.ParentTransform, attachEntityInfo.UserData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnAttachTo with exception '{2}'.", _id, _entityAssetAddress, exception);
            }

            ReferencePool.Release(attachEntityInfo);
        }

        /// <summary>
        /// 实体解除子实体。
        /// </summary>
        /// <param name="parentEntity">被解除的父实体。</param>
        /// <param name="userData">用户自定义数据。</param>
        public void OnDetachFrom(IEntity parentEntity, object userData)
        {
            try
            {
                _entityLogic.OnDetachFrom(((Entity)parentEntity).Logic, userData);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnDetachFrom with exception '{2}'.", _id, _entityAssetAddress, exception);
            }
        }

        /// <summary>
        /// 实体轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        public void OnUpdate(float elapseSeconds, float realElapseSeconds)
        {
            try
            {
                _entityLogic.OnUpdate(elapseSeconds, realElapseSeconds);
            }
            catch (Exception exception)
            {
                Log.Error("Entity '[{0}]{1}' OnUpdate with exception '{2}'.", _id, _entityAssetAddress, exception);
            }
        }
    }
}
