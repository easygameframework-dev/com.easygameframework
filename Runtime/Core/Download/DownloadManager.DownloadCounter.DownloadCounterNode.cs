//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Download
{
    internal sealed partial class DownloadManager : GameFrameworkModule, IDownloadManager
    {
        private sealed partial class DownloadCounter
        {
            private sealed class DownloadCounterNode : IReference
            {
                private long _deltaLength;
                private float _elapseSeconds;

                public DownloadCounterNode()
                {
                    _deltaLength = 0L;
                    _elapseSeconds = 0f;
                }

                public long DeltaLength
                {
                    get
                    {
                        return _deltaLength;
                    }
                }

                public float ElapseSeconds
                {
                    get
                    {
                        return _elapseSeconds;
                    }
                }

                public static DownloadCounterNode Create()
                {
                    return ReferencePool.Acquire<DownloadCounterNode>();
                }

                public void Update(float elapseSeconds, float realElapseSeconds)
                {
                    _elapseSeconds += realElapseSeconds;
                }

                public void AddDeltaLength(int deltaLength)
                {
                    _deltaLength += deltaLength;
                }

                public void Clear()
                {
                    _deltaLength = 0L;
                    _elapseSeconds = 0f;
                }
            }
        }
    }
}
