//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core.ObjectPool;

namespace EasyGameFramework.Core.UI
{
    internal sealed partial class UIManager : GameFrameworkModule, IUIManager
    {
        /// <summary>
        /// 界面实例对象。
        /// </summary>
        private sealed class UIFormInstanceObject : ObjectBase
        {
            private object _uIFormAsset;
            private IUIFormHelper _uIFormHelper;

            public UIFormInstanceObject()
            {
                _uIFormAsset = null;
                _uIFormHelper = null;
            }

            public static UIFormInstanceObject Create(string name, object uiFormAsset, object uiFormInstance, IUIFormHelper uiFormHelper)
            {
                if (uiFormAsset == null)
                {
                    throw new GameFrameworkException("UI form asset is invalid.");
                }

                if (uiFormHelper == null)
                {
                    throw new GameFrameworkException("UI form helper is invalid.");
                }

                UIFormInstanceObject uiFormInstanceObject = ReferencePool.Acquire<UIFormInstanceObject>();
                uiFormInstanceObject.Initialize(name, uiFormInstance);
                uiFormInstanceObject._uIFormAsset = uiFormAsset;
                uiFormInstanceObject._uIFormHelper = uiFormHelper;
                return uiFormInstanceObject;
            }

            public override void Clear()
            {
                base.Clear();
                _uIFormAsset = null;
                _uIFormHelper = null;
            }

            protected internal override void Release(bool isShutdown)
            {
                _uIFormHelper.ReleaseUIForm(_uIFormAsset, Target);
            }
        }
    }
}
