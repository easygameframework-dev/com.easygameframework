//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Sound
{
    internal sealed partial class SoundManager : GameFrameworkModule, ISoundManager
    {
        private sealed class PlaySoundInfo : IReference
        {
            private int _serialId;
            private SoundGroup _soundGroup;
            private PlaySoundParams _playSoundParams;
            private object _userData;

            public PlaySoundInfo()
            {
                _serialId = 0;
                _soundGroup = null;
                _playSoundParams = null;
                _userData = null;
            }

            public int SerialId
            {
                get
                {
                    return _serialId;
                }
            }

            public SoundGroup SoundGroup
            {
                get
                {
                    return _soundGroup;
                }
            }

            public PlaySoundParams PlaySoundParams
            {
                get
                {
                    return _playSoundParams;
                }
            }

            public object UserData
            {
                get
                {
                    return _userData;
                }
            }

            public static PlaySoundInfo Create(int serialId, SoundGroup soundGroup, PlaySoundParams playSoundParams, object userData)
            {
                PlaySoundInfo playSoundInfo = ReferencePool.Acquire<PlaySoundInfo>();
                playSoundInfo._serialId = serialId;
                playSoundInfo._soundGroup = soundGroup;
                playSoundInfo._playSoundParams = playSoundParams;
                playSoundInfo._userData = userData;
                return playSoundInfo;
            }

            public void Clear()
            {
                _serialId = 0;
                _soundGroup = null;
                _playSoundParams = null;
                _userData = null;
            }
        }
    }
}
