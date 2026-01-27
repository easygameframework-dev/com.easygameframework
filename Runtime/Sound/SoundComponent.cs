//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

using EasyGameFramework.Core;
using EasyGameFramework.Core.Resource;
#if UNITY_5_3
using GameFramework.Scene;
#endif
using EasyGameFramework.Core.Sound;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;

namespace EasyGameFramework
{
    /// <summary>
    /// 声音组件。
    /// </summary>
    [DisallowMultipleComponent]
    [AddComponentMenu("Game Framework/Sound")]
    public sealed partial class SoundComponent : GameFrameworkComponent
    {
        private const int DefaultPriority = 0;

        private ISoundManager _soundManager = null;
        private EventComponent _eventComponent = null;
        private AudioListener _audioListener = null;
        private IResourceManager _resourceManager = null;

        [SerializeField]
        private Transform _instanceRoot = null;

        [SerializeField]
        private AudioMixer _audioMixer = null;

        [SerializeField]
        private string _soundHelperTypeName = "UnityGameFramework.Runtime.DefaultSoundHelper";

        [SerializeField]
        private SoundHelperBase _customSoundHelper = null;

        [SerializeField]
        private string _soundGroupHelperTypeName = "UnityGameFramework.Runtime.DefaultSoundGroupHelper";

        [SerializeField]
        private SoundGroupHelperBase _customSoundGroupHelper = null;

        [SerializeField]
        private string _soundAgentHelperTypeName = "UnityGameFramework.Runtime.DefaultSoundAgentHelper";

        [SerializeField]
        private SoundAgentHelperBase _customSoundAgentHelper = null;

        [SerializeField]
        private SoundGroup[] _soundGroups = null;

        /// <summary>
        /// 获取声音组数量。
        /// </summary>
        public int SoundGroupCount
        {
            get
            {
                return _soundManager.SoundGroupCount;
            }
        }

        /// <summary>
        /// 获取声音混响器。
        /// </summary>
        public AudioMixer AudioMixer
        {
            get
            {
                return _audioMixer;
            }
        }

        /// <summary>
        /// 游戏框架组件初始化。
        /// </summary>
        protected override void Awake()
        {
            base.Awake();

            _soundManager = GameFrameworkEntry.GetModule<ISoundManager>();
            if (_soundManager == null)
            {
                Log.Fatal("Sound manager is invalid.");
                return;
            }

            _soundManager.PlaySoundSuccess += OnPlaySoundSuccess;
            _soundManager.PlaySoundFailure += OnPlaySoundFailure;

            _audioListener = gameObject.GetOrAddComponent<AudioListener>();

#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded += OnSceneLoaded;
            SceneManager.sceneUnloaded += OnSceneUnloaded;
#else
            ISceneManager sceneManager = GameFrameworkEntry.GetModule<ISceneManager>();
            if (sceneManager == null)
            {
                Log.Fatal("Scene manager is invalid.");
                return;
            }

            sceneManager.LoadSceneSuccess += OnLoadSceneSuccess;
            sceneManager.LoadSceneFailure += OnLoadSceneFailure;
            sceneManager.UnloadSceneSuccess += OnUnloadSceneSuccess;
            sceneManager.UnloadSceneFailure += OnUnloadSceneFailure;
#endif
        }

        private void Start()
        {
            BaseComponent baseComponent = GameEntry.GetComponent<BaseComponent>();
            if (baseComponent == null)
            {
                Log.Fatal("Base component is invalid.");
                return;
            }

            _eventComponent = GameEntry.GetComponent<EventComponent>();
            if (_eventComponent == null)
            {
                Log.Fatal("Event component is invalid.");
                return;
            }

            _resourceManager = GameFrameworkEntry.GetModule<IResourceManager>();
            _soundManager.SetResourceManager(_resourceManager);

            SoundHelperBase soundHelper = Helper.CreateHelper(_soundHelperTypeName, _customSoundHelper);
            if (soundHelper == null)
            {
                Log.Error("Can not create sound helper.");
                return;
            }

            soundHelper.name = "Sound Helper";
            Transform transform = soundHelper.transform;
            transform.SetParent(this.transform);
            transform.localScale = Vector3.one;

            _soundManager.SetSoundHelper(soundHelper);

            if (_instanceRoot == null)
            {
                _instanceRoot = new GameObject("Sound Instances").transform;
                _instanceRoot.SetParent(gameObject.transform);
                _instanceRoot.localScale = Vector3.one;
            }

            for (int i = 0; i < _soundGroups.Length; i++)
            {
                if (!AddSoundGroup(_soundGroups[i].Name, _soundGroups[i].AvoidBeingReplacedBySamePriority, _soundGroups[i].Mute, _soundGroups[i].Volume, _soundGroups[i].AgentHelperCount))
                {
                    Log.Warning("Add sound group '{0}' failure.", _soundGroups[i].Name);
                    continue;
                }
            }
        }

        private void OnDestroy()
        {
#if UNITY_5_4_OR_NEWER
            SceneManager.sceneLoaded -= OnSceneLoaded;
            SceneManager.sceneUnloaded -= OnSceneUnloaded;
#endif
        }

        /// <summary>
        /// 是否存在指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>指定声音组是否存在。</returns>
        public bool HasSoundGroup(string soundGroupName)
        {
            return _soundManager.HasSoundGroup(soundGroupName);
        }

        /// <summary>
        /// 获取指定声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <returns>要获取的声音组。</returns>
        public ISoundGroup GetSoundGroup(string soundGroupName)
        {
            return _soundManager.GetSoundGroup(soundGroupName);
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <returns>所有声音组。</returns>
        public ISoundGroup[] GetAllSoundGroups()
        {
            return _soundManager.GetAllSoundGroups();
        }

        /// <summary>
        /// 获取所有声音组。
        /// </summary>
        /// <param name="results">所有声音组。</param>
        public void GetAllSoundGroups(List<ISoundGroup> results)
        {
            _soundManager.GetAllSoundGroups(results);
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, int soundAgentHelperCount)
        {
            return AddSoundGroup(soundGroupName, false, false, 1f, soundAgentHelperCount);
        }

        /// <summary>
        /// 增加声音组。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupAvoidBeingReplacedBySamePriority">声音组中的声音是否避免被同优先级声音替换。</param>
        /// <param name="soundGroupMute">声音组是否静音。</param>
        /// <param name="soundGroupVolume">声音组音量。</param>
        /// <param name="soundAgentHelperCount">声音代理辅助器数量。</param>
        /// <returns>是否增加声音组成功。</returns>
        public bool AddSoundGroup(string soundGroupName, bool soundGroupAvoidBeingReplacedBySamePriority, bool soundGroupMute, float soundGroupVolume, int soundAgentHelperCount)
        {
            if (_soundManager.HasSoundGroup(soundGroupName))
            {
                return false;
            }

            SoundGroupHelperBase soundGroupHelper = Helper.CreateHelper(_soundGroupHelperTypeName, _customSoundGroupHelper, SoundGroupCount);
            if (soundGroupHelper == null)
            {
                Log.Error("Can not create sound group helper.");
                return false;
            }

            soundGroupHelper.name = Utility.Text.Format("Sound Group - {0}", soundGroupName);
            Transform transform = soundGroupHelper.transform;
            transform.SetParent(_instanceRoot);
            transform.localScale = Vector3.one;

            if (_audioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups = _audioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}", soundGroupName));
                if (audioMixerGroups.Length > 0)
                {
                    soundGroupHelper.AudioMixerGroup = audioMixerGroups[0];
                }
                else
                {
                    soundGroupHelper.AudioMixerGroup = _audioMixer.FindMatchingGroups("Master")[0];
                }
            }

            if (!_soundManager.AddSoundGroup(soundGroupName, soundGroupAvoidBeingReplacedBySamePriority, soundGroupMute, soundGroupVolume, soundGroupHelper))
            {
                return false;
            }

            for (int i = 0; i < soundAgentHelperCount; i++)
            {
                if (!AddSoundAgentHelper(soundGroupName, soundGroupHelper, i))
                {
                    return false;
                }
            }

            return true;
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <returns>所有正在加载声音的序列编号。</returns>
        public int[] GetAllLoadingSoundSerialIds()
        {
            return _soundManager.GetAllLoadingSoundSerialIds();
        }

        /// <summary>
        /// 获取所有正在加载声音的序列编号。
        /// </summary>
        /// <param name="results">所有正在加载声音的序列编号。</param>
        public void GetAllLoadingSoundSerialIds(List<int> results)
        {
            _soundManager.GetAllLoadingSoundSerialIds(results);
        }

        /// <summary>
        /// 是否正在加载声音。
        /// </summary>
        /// <param name="serialId">声音序列编号。</param>
        /// <returns>是否正在加载声音。</returns>
        public bool IsLoadingSound(int serialId)
        {
            return _soundManager.IsLoadingSound(serialId);
        }

        /// <summary>
        /// 播放声音。
        /// </summary>
        /// <param name="soundAssetAddress">声音资源地址。</param>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="customPriority">加载声音资源的优先级。</param>
        /// <param name="playSoundParams">播放声音参数。</param>
        /// <param name="bindingEntity">要绑定到的实体</param>
        /// <param name="worldPosition">声音所在的世界坐标。</param>
        /// <param name="userData">用户自定义数据。</param>
        /// <returns>声音的序列编号。</returns>
        public int PlaySound(
            AssetAddress soundAssetAddress,
            string soundGroupName,
            PlaySoundParams playSoundParams = null,
            Entity bindingEntity = null,
            Vector3 worldPosition = new Vector3(),
            int? customPriority = null,
            object userData = null)
        {
            return _soundManager.PlaySound(soundAssetAddress, soundGroupName, playSoundParams, customPriority, PlaySoundInfo.Create(bindingEntity, worldPosition, userData));
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId)
        {
            return _soundManager.StopSound(serialId);
        }

        /// <summary>
        /// 停止播放声音。
        /// </summary>
        /// <param name="serialId">要停止播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        /// <returns>是否停止播放声音成功。</returns>
        public bool StopSound(int serialId, float fadeOutSeconds)
        {
            return _soundManager.StopSound(serialId, fadeOutSeconds);
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        public void StopAllLoadedSounds()
        {
            _soundManager.StopAllLoadedSounds();
        }

        /// <summary>
        /// 停止所有已加载的声音。
        /// </summary>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void StopAllLoadedSounds(float fadeOutSeconds)
        {
            _soundManager.StopAllLoadedSounds(fadeOutSeconds);
        }

        /// <summary>
        /// 停止所有正在加载的声音。
        /// </summary>
        public void StopAllLoadingSounds()
        {
            _soundManager.StopAllLoadingSounds();
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        public void PauseSound(int serialId)
        {
            _soundManager.PauseSound(serialId);
        }

        /// <summary>
        /// 暂停播放声音。
        /// </summary>
        /// <param name="serialId">要暂停播放声音的序列编号。</param>
        /// <param name="fadeOutSeconds">声音淡出时间，以秒为单位。</param>
        public void PauseSound(int serialId, float fadeOutSeconds)
        {
            _soundManager.PauseSound(serialId, fadeOutSeconds);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        public void ResumeSound(int serialId)
        {
            _soundManager.ResumeSound(serialId);
        }

        /// <summary>
        /// 恢复播放声音。
        /// </summary>
        /// <param name="serialId">要恢复播放声音的序列编号。</param>
        /// <param name="fadeInSeconds">声音淡入时间，以秒为单位。</param>
        public void ResumeSound(int serialId, float fadeInSeconds)
        {
            _soundManager.ResumeSound(serialId, fadeInSeconds);
        }

        /// <summary>
        /// 增加声音代理辅助器。
        /// </summary>
        /// <param name="soundGroupName">声音组名称。</param>
        /// <param name="soundGroupHelper">声音组辅助器。</param>
        /// <param name="index">声音代理辅助器索引。</param>
        /// <returns>是否增加声音代理辅助器成功。</returns>
        private bool AddSoundAgentHelper(string soundGroupName, SoundGroupHelperBase soundGroupHelper, int index)
        {
            SoundAgentHelperBase soundAgentHelper = Helper.CreateHelper(_soundAgentHelperTypeName, _customSoundAgentHelper, index);
            if (soundAgentHelper == null)
            {
                Log.Error("Can not create sound agent helper.");
                return false;
            }

            soundAgentHelper.name = Utility.Text.Format("Sound Agent Helper - {0} - {1}", soundGroupName, index);
            Transform transform = soundAgentHelper.transform;
            transform.SetParent(soundGroupHelper.transform);
            transform.localScale = Vector3.one;

            if (_audioMixer != null)
            {
                AudioMixerGroup[] audioMixerGroups = _audioMixer.FindMatchingGroups(Utility.Text.Format("Master/{0}/{1}", soundGroupName, index));
                if (audioMixerGroups.Length > 0)
                {
                    soundAgentHelper.AudioMixerGroup = audioMixerGroups[0];
                }
                else
                {
                    soundAgentHelper.AudioMixerGroup = soundGroupHelper.AudioMixerGroup;
                }
            }

            _soundManager.AddSoundAgentHelper(soundGroupName, soundAgentHelper);

            return true;
        }

        private void OnPlaySoundSuccess(object sender, EasyGameFramework.Core.Sound.PlaySoundSuccessEventArgs e)
        {
            PlaySoundInfo playSoundInfo = (PlaySoundInfo)e.UserData;
            if (playSoundInfo != null)
            {
                SoundAgentHelperBase soundAgentHelper = (SoundAgentHelperBase)e.SoundAgent.Helper;
                if (playSoundInfo.BindingEntity != null)
                {
                    soundAgentHelper.SetBindingEntity(playSoundInfo.BindingEntity);
                }
                else
                {
                    soundAgentHelper.SetWorldPosition(playSoundInfo.WorldPosition);
                }
            }

            _eventComponent.Fire(this, PlaySoundSuccessEventArgs.Create(e));
        }

        private void OnPlaySoundFailure(object sender, EasyGameFramework.Core.Sound.PlaySoundFailureEventArgs e)
        {
            string logMessage = Utility.Text.Format("Play sound failure, asset address '{0}', sound group name '{1}', error code '{2}', error message '{3}'.", e.SoundAssetAddress, e.SoundGroupName, e.ErrorCode, e.ErrorMessage);
            if (e.ErrorCode == PlaySoundErrorCode.IgnoredDueToLowPriority)
            {
                Log.Info(logMessage);
            }
            else
            {
                Log.Warning(logMessage);
            }

            _eventComponent.Fire(this, PlaySoundFailureEventArgs.Create(e));
        }

        private void OnLoadSceneSuccess(object sender, EasyGameFramework.Core.Scene.LoadSceneSuccessEventArgs e)
        {
            RefreshAudioListener();
        }

        private void OnLoadSceneFailure(object sender, EasyGameFramework.Core.Scene.LoadSceneFailureEventArgs e)
        {
            RefreshAudioListener();
        }

        private void OnUnloadSceneSuccess(object sender, EasyGameFramework.Core.Scene.UnloadSceneSuccessEventArgs e)
        {
            RefreshAudioListener();
        }

        private void OnUnloadSceneFailure(object sender, EasyGameFramework.Core.Scene.UnloadSceneFailureEventArgs e)
        {
            RefreshAudioListener();
        }

        private void OnSceneLoaded(Scene scene, LoadSceneMode loadSceneMode)
        {
            RefreshAudioListener();
        }

        private void OnSceneUnloaded(Scene scene)
        {
            RefreshAudioListener();
        }

        private void RefreshAudioListener()
        {
            _audioListener.enabled = FindObjectsOfType<AudioListener>().Length <= 1;
        }
    }
}
