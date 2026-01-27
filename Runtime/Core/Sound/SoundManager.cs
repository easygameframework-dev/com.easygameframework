//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using System;
using System.Collections.Generic;
using EasyGameFramework.Core.Resource;

namespace EasyGameFramework.Core.Sound
{
    /// <summary>
    /// 声音管理器。
    /// </summary>
    internal sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        private readonly Dictionary<string, SoundGroup> _soundGroups;
        private readonly List<int> _soundsBeingLoaded;
        private readonly HashSet<int> _soundsToReleaseOnLoad;
        private readonly LoadAssetCallbacks _loadAssetCallbacks;
        private IResourceManager _resourceManager;
        private ISoundHelper _soundHelper;
        private int _serial;
        private EventHandler<PlaySoundSuccessEventArgs> _playSoundSuccessEventHandler;
        private EventHandler<PlaySoundFailureEventArgs> _playSoundFailureEventHandler;

        /// <summary>
        /// 初始化声音管理器的新实例。
        /// </summary>
        public SoundManager()
        {
            _soundGroups = new Dictionary<string, SoundGroup>(StringComparer.Ordinal);
            _soundsBeingLoaded = new List<int>();
            _soundsToReleaseOnLoad = new HashSet<int>();
            _loadAssetCallbacks = new LoadAssetCallbacks(LoadAssetSuccessCallback, LoadAssetFailureCallback);
            _resourceManager = null;
            _soundHelper = null;
            _serial = 0;
            _playSoundSuccessEventHandler = null;
            _playSoundFailureEventHandler = null;
        }

        /// <summary>
        /// 获取声音组数量。
        /// </summary>
        public int SoundGroupCount
        {
            get
            {
                return _soundGroups.Count;
            }
        }

        /// <summary>
        /// 播放声音成功事件。
        /// </summary>
        public event EventHandler<PlaySoundSuccessEventArgs> PlaySoundSuccess
        {
            add
            {
                _playSoundSuccessEventHandler += value;
            }
            remove
            {
                _playSoundSuccessEventHandler -= value;
            }
        }

        /// <summary>
        /// 播放声音失败事件。
        /// </summary>
        public event EventHandler<PlaySoundFailureEventArgs> PlaySoundFailure
        {
            add
            {
                _playSoundFailureEventHandler += value;
            }
            remove
            {
                _playSoundFailureEventHandler -= value;
            }
        }

        /// <summary>
        /// 声音管理器轮询。
        /// </summary>
        /// <param name="elapseSeconds">逻辑流逝时间，以秒为单位。</param>
        /// <param name="realElapseSeconds">真实流逝时间，以秒为单位。</param>
        internal override void Update(float elapseSeconds, float realElapseSeconds)
        {
        }

        /// <summary>
        /// 关闭并清理声音管理器。
        /// </summary>
        internal override void Shutdown()
        {
            StopAllLoadedSounds();
            _soundGroups.Clear();
            _soundsBeingLoaded.Clear();
            _soundsToReleaseOnLoad.Clear();
        }

        /// <summary>
        /// 设置资源管理器。
        /// </summary>
        /// <param name="resourceManager">资源管理器。</param>
        public void SetResourceManager(IResourceManager resourceManager)
        {
            if (resourceManager == null)
            {
                throw new GameFrameworkException("Resource manager is invalid.");
            }

            _resourceManager = resourceManager;
        }

        /// <summary>
        /// 设置声音辅助器。
        /// </summary>
        /// <param name="soundHelper">声音辅助器。</param>
        public void SetSoundHelper(ISoundHelper soundHelper)
        {
            if (soundHelper == null)
            {
                throw new GameFrameworkException("Sound helper is invalid.");
            }

            _soundHelper = soundHelper;
        }

        /// <summary>
        /// 是否存在指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>指定声音组是否存在。</returns>
        public bool HasSoundGroup(string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new GameFrameworkException("Sound group name is invalid.");
            }

            return _soundGroups.ContainsKey(soundGroupName);
        }

        /// <summary>
        /// 获取指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>要获取的声音组。</returns>
        public ISoundGroup GetSoundGroup(string soundGroupName)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new GameFrameworkException("Sound group name is invalid.");
            }

            SoundGroup soundGroup = null;
            if (_soundGroups.TryGetValue(soundGroupName, out soundGroup))
            {
                return soundGroup;
            }

            return null;
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <returns>所有声音组。</returns>
        public ISoundGroup[] GetAllSoundGroups()
        {
            int index = 0;
            ISoundGroup[] results = new ISoundGroup[_soundGroups.Count];
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                results[index++] = soundGroup.Value;
            }

            return results;
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <param name="results">所有声音组。</param>
        public void GetAllSoundGroups(List<ISoundGroup> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                results.Add(soundGroup.Value);
            }
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupHelper">声音组辅助器。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, ISoundGroupHelper soundGroupHelper)
        {
            return AddSoundGroup(soundGroupName, false, Constant.DefaultMute, Constant.DefaultVolume, soundGroupHelper);
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="soundGroupMute">声音组是否静音。</param>
        /// <param name="soundGroupVolume">声音组音量。</param>
        /// <param name="soundGroupHelper">声音组辅助器。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute, float soundGroupVolume, ISoundGroupHelper soundGroupHelper)
        {
            if (string.IsNullOrEmpty(soundGroupName))
            {
                throw new GameFrameworkException("Sound group name is invalid.");
            }

            if (soundGroupHelper == null)
            {
                throw new GameFrameworkException("Sound group helper is invalid.");
            }

            if (HasSoundGroup(soundGroupName))
            {
                return false;
            }

            SoundGroup soundGroup = new SoundGroup(soundGroupName, soundGroupHelper)
            {
                AvoidBeingReplacedBySamePriority = soundGroupAvoidBeingReplacedBySamePriority,
                Mute = soundGroupMute,
                Volume = soundGroupVolume
            };

            _soundGroups.Add(soundGroupName, soundGroup);

            return true;
        }

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundAgentHelper">要增加的声音代理辅助器。</param>
        public void AddSoundAgentHelper(string soundGroupName, ISoundAgentHelper soundAgentHelper)
        {
            if (_soundHelper == null)
            {
                throw new GameFrameworkException("You must set sound helper first.");
            }

            SoundGroup soundGroup = (SoundGroup)GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                throw new GameFrameworkException(Utility.Text.Format("Sound group '{0}' is not exist.", soundGroupName));
            }

            soundGroup.AddSoundAgentHelper(_soundHelper, soundAgentHelper);
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <returns>所有正在加载声音的序列编号。</returns>
        public int[] GetAllLoadingSoundSerialIds()
        {
            return _soundsBeingLoaded.ToArray();
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载声音的序列编号。</param>
        public void GetAllLoadingSoundSerialIds(List<int> results)
        {
            if (results == null)
            {
                throw new GameFrameworkException("Results is invalid.");
            }

            results.Clear();
            results.AddRange(_soundsBeingLoaded);
        }

        /// <summary>
        /// 是否正在加载声音。
        /// </summary>
        /// <param name="serialId">声音序列编号。</param>
        /// <returns>是否正在加载声音。</returns>
        public bool IsLoadingSound(int serialId)
        {
            return _soundsBeingLoaded.Contains(serialId);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetAddress">声音资源地址。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="customPriority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(AssetAddress soundAssetAddress, string soundGroupName, PlaySoundParams playSoundParams = null, int? customPriority = null, object userData = null)
        {
            if (_resourceManager == null)
            {
                throw new GameFrameworkException("You must set resource manager first.");
            }

            if (_soundHelper == null)
            {
                throw new GameFrameworkException("You must set sound helper first.");
            }

            if (playSoundParams == null)
            {
                playSoundParams = PlaySoundParams.Create();
            }

            int serialId = ++_serial;
            PlaySoundErrorCode? errorCode = null;
            string errorMessage = null;
            SoundGroup soundGroup = (SoundGroup)GetSoundGroup(soundGroupName);
            if (soundGroup == null)
            {
                errorCode = PlaySoundErrorCode.SoundGroupNotExist;
                errorMessage = Utility.Text.Format("Sound group '{0}' is not exist.", soundGroupName);
            }
            else if (soundGroup.SoundAgentCount <= 0)
            {
                errorCode = PlaySoundErrorCode.SoundGroupHasNoAgent;
                errorMessage = Utility.Text.Format("Sound group '{0}' is have no sound agent.", soundGroupName);
            }

            if (errorCode.HasValue)
            {
                if (_playSoundFailureEventHandler != null)
                {
                    PlaySoundFailureEventArgs playSoundFailureEventArgs = PlaySoundFailureEventArgs.Create(serialId, soundAssetAddress, soundGroupName, playSoundParams, errorCode.Value, errorMessage, userData);
                    _playSoundFailureEventHandler(this, playSoundFailureEventArgs);
                    ReferencePool.Release(playSoundFailureEventArgs);

                    if (playSoundParams.Referenced)
                    {
                        ReferencePool.Release(playSoundParams);
                    }

                    return serialId;
                }

                throw new GameFrameworkException(errorMessage);
            }

            _soundsBeingLoaded.Add(serialId);

            // Use AssetAddress for resource loading
            AssetAddress loadAddress = soundAssetAddress;
            if (string.IsNullOrEmpty(soundAssetAddress.PackageName))
            {
                loadAddress = new AssetAddress("DefaultPackage", soundAssetAddress.Location);
            }
            _resourceManager.LoadAsset(loadAddress, _loadAssetCallbacks, null, customPriority, PlaySoundInfo.Create(serialId, soundGroup, playSoundParams, userData));
            return serialId;
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId)
        {
            return StopSound(serialId, Constant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            if (IsLoadingSound(serialId))
            {
                _soundsToReleaseOnLoad.Add(serialId);
                _soundsBeingLoaded.Remove(serialId);
                return true;
            }

            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                if (soundGroup.Value.StopSound(serialId, fadeOutSeconds))
                {
                    return true;
                }
            }

            return false;
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedSounds()
        {
            StopAllLoadedSounds(Constant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                soundGroup.Value.StopAllLoadedSounds(fadeOutSeconds);
            }
        }

        /// <summary>
        /// 停止所有正在加载的声音。
        /// </summary>
        public void StopAllLoadingSounds()
        {
            foreach (int serialId in _soundsBeingLoaded)
            {
                _soundsToReleaseOnLoad.Add(serialId);
            }
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        public void PauseSound(int serialId)
        {
            PauseSound(serialId, Constant.DefaultFadeOutSeconds);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void PauseSound(int serialId, float fadeOutSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                if (soundGroup.Value.PauseSound(serialId, fadeOutSeconds))
                {
                    return;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("Can not find sound '{0}'.", serialId));
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        public void ResumeSound(int serialId)
        {
            ResumeSound(serialId, Constant.DefaultFadeInSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void ResumeSound(int serialId, float fadeInSeconds)
        {
            foreach (KeyValuePair<string, SoundGroup> soundGroup in _soundGroups)
            {
                if (soundGroup.Value.ResumeSound(serialId, fadeInSeconds))
                {
                    return;
                }
            }

            throw new GameFrameworkException(Utility.Text.Format("Can not find sound '{0}'.", serialId));
        }

        private void LoadAssetSuccessCallback(AssetAddress soundAssetAddress, object soundAsset, float duration, object userData)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)userData;
            if (playSoundInfo == null)
            {
                throw new GameFrameworkException("Play sound info is invalid.");
            }

            if (_soundsToReleaseOnLoad.Contains(playSoundInfo.SerialId))
            {
                _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                ReferencePool.Release(playSoundInfo);
                _soundHelper.ReleaseSoundAsset(soundAsset);
                return;
            }

            _soundsBeingLoaded.Remove(playSoundInfo.SerialId);

            PlaySoundErrorCode? errorCode = null;
            ISoundAgent soundAgent = playSoundInfo.SoundGroup.PlaySound(playSoundInfo.SerialId, soundAsset, playSoundInfo.PlaySoundParams, out errorCode);
            if (soundAgent != null)
            {
                if (_playSoundSuccessEventHandler != null)
                {
                    PlaySoundSuccessEventArgs playSoundSuccessEventArgs = PlaySoundSuccessEventArgs.Create(playSoundInfo.SerialId, soundAssetAddress, soundAgent, duration, playSoundInfo.UserData);
                    _playSoundSuccessEventHandler(this, playSoundSuccessEventArgs);
                    ReferencePool.Release(playSoundSuccessEventArgs);
                }

                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                ReferencePool.Release(playSoundInfo);
                return;
            }

            _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
            _soundHelper.ReleaseSoundAsset(soundAsset);
            string errorMessage = Utility.Text.Format("Sound group '{0}' play sound '{1}' failure.", playSoundInfo.SoundGroup.Name, soundAssetAddress);
            if (_playSoundFailureEventHandler != null)
            {
                PlaySoundFailureEventArgs playSoundFailureEventArgs = PlaySoundFailureEventArgs.Create(playSoundInfo.SerialId, soundAssetAddress, playSoundInfo.SoundGroup.Name, playSoundInfo.PlaySoundParams, errorCode.Value, errorMessage, playSoundInfo.UserData);
                _playSoundFailureEventHandler(this, playSoundFailureEventArgs);
                ReferencePool.Release(playSoundFailureEventArgs);

                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                ReferencePool.Release(playSoundInfo);
                return;
            }

            if (playSoundInfo.PlaySoundParams.Referenced)
            {
                ReferencePool.Release(playSoundInfo.PlaySoundParams);
            }

            ReferencePool.Release(playSoundInfo);
            throw new GameFrameworkException(errorMessage);
        }

        private void LoadAssetFailureCallback(AssetAddress soundAssetAddress, LoadResourceStatus status, string errorMessage, object userData)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)userData;
            if (playSoundInfo == null)
            {
                throw new GameFrameworkException("Play sound info is invalid.");
            }

            if (_soundsToReleaseOnLoad.Contains(playSoundInfo.SerialId))
            {
                _soundsToReleaseOnLoad.Remove(playSoundInfo.SerialId);
                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                return;
            }

            _soundsBeingLoaded.Remove(playSoundInfo.SerialId);
            string appendErrorMessage = Utility.Text.Format("Load sound failure, asset name '{0}', status '{1}', error message '{2}'.", soundAssetAddress, status, errorMessage);
            if (_playSoundFailureEventHandler != null)
            {
                PlaySoundFailureEventArgs playSoundFailureEventArgs = PlaySoundFailureEventArgs.Create(playSoundInfo.SerialId, soundAssetAddress, playSoundInfo.SoundGroup.Name, playSoundInfo.PlaySoundParams, PlaySoundErrorCode.LoadAssetFailure, appendErrorMessage, playSoundInfo.UserData);
                _playSoundFailureEventHandler(this, playSoundFailureEventArgs);
                ReferencePool.Release(playSoundFailureEventArgs);

                if (playSoundInfo.PlaySoundParams.Referenced)
                {
                    ReferencePool.Release(playSoundInfo.PlaySoundParams);
                }

                return;
            }

            throw new GameFrameworkException(appendErrorMessage);
        }
    }
}
