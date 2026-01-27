//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System.Collections.Generic;

namespace EasyGameFramework.Core.Sound
{
    internal sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        /// <summary>
        /// 声音组。
        /// </summary>
        private sealed class SoundGroup : ISoundGroup
        {
            private readonly string _name;
            private readonly ISoundGroupHelper _soundGroupHelper;
            private readonly List<SoundAgent> _soundAgents;
            private bool _avoidBeingReplacedBySamePriority;
            private bool _mute;
            private float _volume;

            /// <summary>
            /// 初始化声音组的新实例。
            /// </summary>
            /// <param name="name">声音组名称。</param>
            /// <param name="soundGroupHelper">声音组辅助器。</param>
            public SoundGroup(string name, ISoundGroupHelper soundGroupHelper)
            {
                if (string.IsNullOrEmpty(name))
                {
                    throw new GameFrameworkException("Sound group name is invalid.");
                }

                if (soundGroupHelper == null)
                {
                    throw new GameFrameworkException("Sound group helper is invalid.");
                }

                _name = name;
                _soundGroupHelper = soundGroupHelper;
                _soundAgents = new List<SoundAgent>();
            }

            /// <summary>
            /// 获取声音组名称。
            /// </summary>
            public string Name
            {
                get
                {
                    return _name;
                }
            }

            /// <summary>
            /// 获取声音代理数。
            /// </summary>
            public int SoundAgentCount
            {
                get
                {
                    return _soundAgents.Count;
                }
            }

            /// <summary>
            /// 获取或设置声音组中的声音是否避免被同优先级声音替换。
            /// </summary>
            public bool AvoidBeingReplacedBySamePriority
            {
                get
                {
                    return _avoidBeingReplacedBySamePriority;
                }
                set
                {
                    _avoidBeingReplacedBySamePriority = value;
                }
            }

            /// <summary>
            /// 获取或设置声音组静音。
            /// </summary>
            public bool Mute
            {
                get
                {
                    return _mute;
                }
                set
                {
                    _mute = value;
                    foreach (SoundAgent soundAgent in _soundAgents)
                    {
                        soundAgent.RefreshMute();
                    }
                }
            }

            /// <summary>
            /// 获取或设置声音组音量。
            /// </summary>
            public float Volume
            {
                get
                {
                    return _volume;
                }
                set
                {
                    _volume = value;
                    foreach (SoundAgent soundAgent in _soundAgents)
                    {
                        soundAgent.RefreshVolume();
                    }
                }
            }

            /// <summary>
            /// 获取声音组辅助器。
            /// </summary>
            public ISoundGroupHelper Helper
            {
                get
                {
                    return _soundGroupHelper;
                }
            }

            /// <summary>
            /// 增加声音代理辅助器。
            /// </summary>
            /// <param name="soundHelper">声音辅助器接口。</param>
            /// <param name="soundAgentHelper">要增加的声音代理辅助器。</param>
            public void AddSoundAgentHelper(ISoundHelper soundHelper, ISoundAgentHelper soundAgentHelper)
            {
                _soundAgents.Add(new SoundAgent(this, soundHelper, soundAgentHelper));
            }

            /// <summary>
            /// 播放声音。
            /// </summary>
            /// <param name="serialId">声音的序列编号。</param>
            /// <param name="soundAsset">声音资源。</param>
            /// <param name="playSoundParams">播放声音参数。</param>
            /// <param name="errorCode">错误码。</param>
            /// <returns>用于播放的声音代理。</returns>
            public ISoundAgent PlaySound(int serialId, object soundAsset, PlaySoundParams playSoundParams, out PlaySoundErrorCode? errorCode)
            {
                errorCode = null;
                SoundAgent candidateAgent = null;
                foreach (SoundAgent soundAgent in _soundAgents)
                {
                    if (!soundAgent.IsPlaying)
                    {
                        candidateAgent = soundAgent;
                        break;
                    }

                    if (soundAgent.Priority < playSoundParams.Priority)
                    {
                        if (candidateAgent == null || soundAgent.Priority < candidateAgent.Priority)
                        {
                            candidateAgent = soundAgent;
                        }
                    }
                    else if (!_avoidBeingReplacedBySamePriority && soundAgent.Priority == playSoundParams.Priority)
                    {
                        if (candidateAgent == null || soundAgent.SetSoundAssetTime < candidateAgent.SetSoundAssetTime)
                        {
                            candidateAgent = soundAgent;
                        }
                    }
                }

                if (candidateAgent == null)
                {
                    errorCode = PlaySoundErrorCode.IgnoredDueToLowPriority;
                    return null;
                }

                if (!candidateAgent.SetSoundAsset(soundAsset))
                {
                    errorCode = PlaySoundErrorCode.SetSoundAssetFailure;
                    return null;
                }

                candidateAgent.SerialId = serialId;
                candidateAgent.Time = playSoundParams.Time;
                candidateAgent.MuteInSoundGroup = playSoundParams.MuteInSoundGroup;
                candidateAgent.Loop = playSoundParams.Loop;
                candidateAgent.Priority = playSoundParams.Priority;
                candidateAgent.VolumeInSoundGroup = playSoundParams.VolumeInSoundGroup;
                candidateAgent.Pitch = playSoundParams.Pitch;
                candidateAgent.PanStereo = playSoundParams.PanStereo;
                candidateAgent.SpatialBlend = playSoundParams.SpatialBlend;
                candidateAgent.MaxDistance = playSoundParams.MaxDistance;
                candidateAgent.DopplerLevel = playSoundParams.DopplerLevel;
                candidateAgent.Play(playSoundParams.FadeInSeconds);
                return candidateAgent;
            }

            /// <summary>
            /// 停止播放声音。
            /// </summary>
            /// <param name="serialId">要停止播放声音的序列编号。</param>
            /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
            /// <returns>是否停止播放声音成功。</returns>
            public bool StopSound(int serialId, float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in _soundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Stop(fadeOutSeconds);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 暂停播放声音。
            /// </summary>
            /// <param name="serialId">要暂停播放声音的序列编号。</param>
            /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
            /// <returns>是否暂停播放声音成功。</returns>
            public bool PauseSound(int serialId, float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in _soundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Pause(fadeOutSeconds);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 恢复播放声音。
            /// </summary>
            /// <param name="serialId">要恢复播放声音的序列编号。</param>
            /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
            /// <returns>是否恢复播放声音成功。</returns>
            public bool ResumeSound(int serialId, float fadeInSeconds)
            {
                foreach (SoundAgent soundAgent in _soundAgents)
                {
                    if (soundAgent.SerialId != serialId)
                    {
                        continue;
                    }

                    soundAgent.Resume(fadeInSeconds);
                    return true;
                }

                return false;
            }

            /// <summary>
            /// 停止所有已加载的声音。
            /// </summary>
            public void StopAllLoadedSounds()
            {
                foreach (SoundAgent soundAgent in _soundAgents)
                {
                    if (soundAgent.IsPlaying)
                    {
                        soundAgent.Stop();
                    }
                }
            }

            /// <summary>
            /// 停止所有已加载的声音。
            /// </summary>
            /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
            public void StopAllLoadedSounds(float fadeOutSeconds)
            {
                foreach (SoundAgent soundAgent in _soundAgents)
                {
                    if (soundAgent.IsPlaying)
                    {
                        soundAgent.Stop(fadeOutSeconds);
                    }
                }
            }
        }
    }
}
