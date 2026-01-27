//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.UI
{
    internal sealed partial class UIManager : GameFrameworkModule, IUIManager
    {
        private sealed partial class UIGroup : IUIGroup
        {
            /// <summary>
            /// 界面组界面信息。
            /// </summary>
            private sealed class UIFormInfo : IReference
            {
                private IUIForm _uIForm;
                private bool _paused;
                private bool _covered;

                public UIFormInfo()
                {
                    _uIForm = null;
                    _paused = false;
                    _covered = false;
                }

                public IUIForm UIForm
                {
                    get
                    {
                        return _uIForm;
                    }
                }

                public bool Paused
                {
                    get
                    {
                        return _paused;
                    }
                    set
                    {
                        _paused = value;
                    }
                }

                public bool Covered
                {
                    get
                    {
                        return _covered;
                    }
                    set
                    {
                        _covered = value;
                    }
                }

                public static UIFormInfo Create(IUIForm uiForm)
                {
                    if (uiForm == null)
                    {
                        throw new GameFrameworkException("UI form is invalid.");
                    }

                    UIFormInfo uiFormInfo = ReferencePool.Acquire<UIFormInfo>();
                    uiFormInfo._uIForm = uiForm;
                    uiFormInfo._paused = true;
                    uiFormInfo._covered = true;
                    return uiFormInfo;
                }

                public void Clear()
                {
                    _uIForm = null;
                    _paused = false;
                    _covered = false;
                }
            }
        }
    }
}
