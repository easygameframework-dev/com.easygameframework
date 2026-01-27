//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using UnityEngine;

namespace EasyGameFramework
{
    internal sealed class WWWFormInfo : IReference
    {
        private WWWForm _wWWForm;
        private object _userData;

        public WWWFormInfo()
        {
            _wWWForm = null;
            _userData = null;
        }

        public WWWForm WWWForm
        {
            get
            {
                return _wWWForm;
            }
        }

        public object UserData
        {
            get
            {
                return _userData;
            }
        }

        public static WWWFormInfo Create(WWWForm wwwForm, object userData)
        {
            WWWFormInfo wwwFormInfo = ReferencePool.Acquire<WWWFormInfo>();
            wwwFormInfo._wWWForm = wwwForm;
            wwwFormInfo._userData = userData;
            return wwwFormInfo;
        }

        public void Clear()
        {
            _wWWForm = null;
            _userData = null;
        }
    }
}
