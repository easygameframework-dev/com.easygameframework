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
        private sealed class OpenUIFormInfo : IReference
        {
            private int _serialId;
            private UIGroup _uIGroup;
            private bool _pauseCoveredUIForm;
            private object _userData;

            public OpenUIFormInfo()
            {
                _serialId = 0;
                _uIGroup = null;
                _pauseCoveredUIForm = false;
                _userData = null;
            }

            public int SerialId
            {
                get
                {
                    return _serialId;
                }
            }

            public UIGroup UIGroup
            {
                get
                {
                    return _uIGroup;
                }
            }

            public bool PauseCoveredUIForm
            {
                get
                {
                    return _pauseCoveredUIForm;
                }
            }

            public object UserData
            {
                get
                {
                    return _userData;
                }
            }

            public static OpenUIFormInfo Create(int serialId, UIGroup uiGroup, bool pauseCoveredUIForm, object userData)
            {
                OpenUIFormInfo openUIFormInfo = ReferencePool.Acquire<OpenUIFormInfo>();
                openUIFormInfo._serialId = serialId;
                openUIFormInfo._uIGroup = uiGroup;
                openUIFormInfo._pauseCoveredUIForm = pauseCoveredUIForm;
                openUIFormInfo._userData = userData;
                return openUIFormInfo;
            }

            public void Clear()
            {
                _serialId = 0;
                _uIGroup = null;
                _pauseCoveredUIForm = false;
                _userData = null;
            }
        }
    }
}
