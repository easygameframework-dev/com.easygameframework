//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;

namespace EasyGameFramework.Core.Sound
{
    internal sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        /// <summary>
        /// 声音代理。
        /// </summary>
        private sealed class SoundAgent : ISoundAgent
        {
            private readonly SoundGroup _soundGroup;
            private readonly ISoundHelper _soundHelper;
            private readonly ISoundAgentHelper _soundAgentHelper;
            private int _serialId;
            private object _soundAsset;
            private DateTime _setSoundAssetTime;
            private bool _muteInSoundGroup;
            private float _volumeInSoundGroup;

            /// <summary>
            /// 初始化声音代理的新实例。
            /// </summary>
            /// <param name="soundGroup">所在的声音组。</param>
            /// <param name="soundHelper">声音辅助器接口。</param>
            /// <param name="soundAgentHelper">声音代理辅助器接口。</param>
            public SoundAgent(SoundGroup soundGroup, ISoundHelper soundHelper, ISoundAgentHelper soundAgentHelper)
            {
                if (soundGroup == null)
                {
                    throw new GameFrameworkException("Sound group is invalid.");
                }

                if (soundHelper == null)
                {
                    throw new GameFrameworkException("Sound helper is invalid.");
                }

                if (soundAgentHelper == null)
                {
                    throw new GameFrameworkException("Sound agent helper is invalid.");
                }

                _soundGroup = soundGroup;
                _soundHelper = soundHelper;
                _soundAgentHelper = soundAgentHelper;
                _soundAgentHelper.ResetSoundAgent += OnResetSoundAgent;
                _serialId = 0;
                _soundAsset = null;
                Reset();
            }

            /// <summary>
            /// 获取所在的声音组。
            /// </summary>
            public ISoundGroup SoundGroup
            {
                get
                {
                    return _soundGroup;
                }
            }

            /// <summary>
            /// 获取或设置声音的序列编号。
            /// </summary>
            public int SerialId
            {
                get
                {
                    return _serialId;
                }
                set
                {
                    _serialId = value;
                }
            }

            /// <summary>
            /// 获取当前是否正在播放。
            /// </summary>
            public bool IsPlaying
            {
                get
                {
                    return _soundAgentHelper.IsPlaying;
                }
            }

            /// <summary>
            /// 获取声音长度。
            /// </summary>
            public float Length
            {
                get
                {
                    return _soundAgentHelper.Length;
                }
            }

            /// <summary>
            /// 获取或设置播放位置。
            /// </summary>
            public float Time
            {
                get
                {
                    return _soundAgentHelper.Time;
                }
                set
                {
                    _soundAgentHelper.Time = value;
                }
            }

            /// <summary>
            /// 获取是否静音。
            /// </summary>
            public bool Mute
            {
                get
                {
                    return _soundAgentHelper.Mute;
                }
            }

            /// <summary>
            /// 获取或设置在声音组内是否静音。
            /// </summary>
            public bool MuteInSoundGroup
            {
                get
                {
                    return _muteInSoundGroup;
                }
                set
                {
                    _muteInSoundGroup = value;
                    RefreshMute();
                }
            }

            /// <summary>
            /// 获取或设置是否循环播放。
            /// </summary>
            public bool Loop
            {
                get
                {
                    return _soundAgentHelper.Loop;
                }
                set
                {
                    _soundAgentHelper.Loop = value;
                }
            }

            /// <summary>
            /// 获取或设置声音优先级。
            /// </summary>
            public int Priority
            {
                get
                {
                    return _soundAgentHelper.Priority;
                }
                set
                {
                    _soundAgentHelper.Priority = value;
                }
            }

            /// <summary>
            /// 获取音量大小。
            /// </summary>
            public float Volume
            {
                get
                {
                    return _soundAgentHelper.Volume;
                }
            }

            /// <summary>
            /// 获取或设置在声音组内音量大小。
            /// </summary>
            public float VolumeInSoundGroup
            {
                get
                {
                    return _volumeInSoundGroup;
                }
                set
                {
                    _volumeInSoundGroup = value;
                    RefreshVolume();
                }
            }

            /// <summary>
            /// 获取或设置声音音调。
            /// </summary>
            public float Pitch
            {
                get
                {
                    return _soundAgentHelper.Pitch;
                }
                set
                {
                    _soundAgentHelper.Pitch = value;
                }
            }

            /// <summary>
            /// 获取或设置声音立体声声相。
            /// </summary>
            public float PanStereo
            {
                get
                {
                    return _soundAgentHelper.PanStereo;
                }
                set
                {
                    _soundAgentHelper.PanStereo = value;
                }
            }

            /// <summary>
            /// 获取或设置声音空间混合量。
            /// </summary>
            public float SpatialBlend
            {
                get
                {
                    return _soundAgentHelper.SpatialBlend;
                }
                set
                {
                    _soundAgentHelper.SpatialBlend = value;
                }
            }

            /// <summary>
            /// 获取或设置声音最大距离。
            /// </summary>
            public float MaxDistance
            {
                get
                {
                    return _soundAgentHelper.MaxDistance;
                }
                set
                {
                    _soundAgentHelper.MaxDistance = value;
                }
            }

            /// <summary>
            /// 获取或设置声音多普勒等级。
            /// </summary>
            public float DopplerLevel
            {
                get
                {
                    return _soundAgentHelper.DopplerLevel;
                }
                set
                {
                    _soundAgentHelper.DopplerLevel = value;
                }
            }

            /// <summary>
            /// 获取声音代理辅助器。
            /// </summary>
            public ISoundAgentHelper Helper
            {
                get
                {
                    return _soundAgentHelper;
                }
            }

            /// <summary>
            /// 获取声音创建时间。
            /// </summary>
            internal DateTime SetSoundAssetTime
            {
                get
                {
                    return _setSoundAssetTime;
                }
            }

            /// <summary>
            /// 播放声音。
            /// </summary>
            public void Play()
            {
                _soundAgentHelper.Play(Constant.DefaultFadeInSeconds);
            }

            /// <summary>
            /// 播放声音。
            /// </summary>
            /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
            public void Play(float fadeInSeconds)
            {
                _soundAgentHelper.Play(fadeInSeconds);
            }

            /// <summary>
            /// 停止播放声音。
            /// </summary>
            public void Stop()
            {
                _soundAgentHelper.Stop(Constant.DefaultFadeOutSeconds);
            }

            /// <summary>
            /// 停止播放声音。
            /// </summary>
            /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
            public void Stop(float fadeOutSeconds)
            {
                _soundAgentHelper.Stop(fadeOutSeconds);
            }

            /// <summary>
            /// 暂停播放声音。
            /// </summary>
            public void Pause()
            {
                _soundAgentHelper.Pause(Constant.DefaultFadeOutSeconds);
            }

            /// <summary>
            /// 暂停播放声音。
            /// </summary>
            /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
            public void Pause(float fadeOutSeconds)
            {
                _soundAgentHelper.Pause(fadeOutSeconds);
            }

            /// <summary>
            /// 恢复播放声音。
            /// </summary>
            public void Resume()
            {
                _soundAgentHelper.Resume(Constant.DefaultFadeInSeconds);
            }

            /// <summary>
            /// 恢复播放声音。
            /// </summary>
            /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
            public void Resume(float fadeInSeconds)
            {
                _soundAgentHelper.Resume(fadeInSeconds);
            }

            /// <summary>
            /// 重置声音代理。
            /// </summary>
            public void Reset()
            {
                if (_soundAsset != null)
                {
                    _soundHelper.ReleaseSoundAsset(_soundAsset);
                    _soundAsset = null;
                }

                _setSoundAssetTime = DateTime.MinValue;
                Time = Constant.DefaultTime;
                MuteInSoundGroup = Constant.DefaultMute;
                Loop = Constant.DefaultLoop;
                Priority = Constant.DefaultPriority;
                VolumeInSoundGroup = Constant.DefaultVolume;
                Pitch = Constant.DefaultPitch;
                PanStereo = Constant.DefaultPanStereo;
                SpatialBlend = Constant.DefaultSpatialBlend;
                MaxDistance = Constant.DefaultMaxDistance;
                DopplerLevel = Constant.DefaultDopplerLevel;
                _soundAgentHelper.Reset();
            }

            internal bool SetSoundAsset(object soundAsset)
            {
                Reset();
                _soundAsset = soundAsset;
                _setSoundAssetTime = DateTime.UtcNow;
                return _soundAgentHelper.SetSoundAsset(soundAsset);
            }

            internal void RefreshMute()
            {
                _soundAgentHelper.Mute = _soundGroup.Mute || _muteInSoundGroup;
            }

            internal void RefreshVolume()
            {
                _soundAgentHelper.Volume = _soundGroup.Volume * _volumeInSoundGroup;
            }

            private void OnResetSoundAgent(object sender, ResetSoundAgentEventArgs e)
            {
                Reset();
            }
        }
    }
}
