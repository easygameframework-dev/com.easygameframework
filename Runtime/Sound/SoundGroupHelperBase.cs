//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core.Sound;
using UnityEngine;
using UnityEngine.Audio;

namespace EasyGameFramework
{
    /// <summary>
    /// 声音组辅助器基类。
    /// </summary>
    public abstract class SoundGroupHelperBase : MonoBehaviour, ISoundGroupHelper
    {
        [SerializeField]
        private AudioMixerGroup _audioMixerGroup = null;

        /// <summary>
        /// 获取或设置声音组辅助器所在的混音组。
        /// </summary>
        public AudioMixerGroup AudioMixerGroup
        {
            get
            {
                return _audioMixerGroup;
            }
            set
            {
                _audioMixerGroup = value;
            }
        }
    }
}
