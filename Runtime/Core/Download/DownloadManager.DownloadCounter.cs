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
            private readonly GameFrameworkLinkedList<DownloadCounterNode> _downloadCounterNodes;
            private float _updateInterval;
            private float _recordInterval;
            private float _currentSpeed;
            private float _accumulator;
            private float _timeLeft;

            public DownloadCounter(float updateInterval, float recordInterval)
            {
                if (updateInterval <= 0f)
                {
                    throw new GameFrameworkException("Update interval is invalid.");
                }

                if (recordInterval <= 0f)
                {
                    throw new GameFrameworkException("Record interval is invalid.");
                }

                _downloadCounterNodes = new GameFrameworkLinkedList<DownloadCounterNode>();
                _updateInterval = updateInterval;
                _recordInterval = recordInterval;
                Reset();
            }

            public float UpdateInterval
            {
                get
                {
                    return _updateInterval;
                }
                set
                {
                    if (value <= 0f)
                    {
                        throw new GameFrameworkException("Update interval is invalid.");
                    }

                    _updateInterval = value;
                    Reset();
                }
            }

            public float RecordInterval
            {
                get
                {
                    return _recordInterval;
                }
                set
                {
                    if (value <= 0f)
                    {
                        throw new GameFrameworkException("Record interval is invalid.");
                    }

                    _recordInterval = value;
                    Reset();
                }
            }

            public float CurrentSpeed
            {
                get
                {
                    return _currentSpeed;
                }
            }

            public void Shutdown()
            {
                Reset();
            }

            public void Update(float elapseSeconds, float realElapseSeconds)
            {
                if (_downloadCounterNodes.Count <= 0)
                {
                    return;
                }

                _accumulator += realElapseSeconds;
                if (_accumulator > _recordInterval)
                {
                    _accumulator = _recordInterval;
                }

                _timeLeft -= realElapseSeconds;
                foreach (DownloadCounterNode downloadCounterNode in _downloadCounterNodes)
                {
                    downloadCounterNode.Update(elapseSeconds, realElapseSeconds);
                }

                while (_downloadCounterNodes.Count > 0)
                {
                    DownloadCounterNode downloadCounterNode = _downloadCounterNodes.First.Value;
                    if (downloadCounterNode.ElapseSeconds < _recordInterval)
                    {
                        break;
                    }

                    ReferencePool.Release(downloadCounterNode);
                    _downloadCounterNodes.RemoveFirst();
                }

                if (_downloadCounterNodes.Count <= 0)
                {
                    Reset();
                    return;
                }

                if (_timeLeft <= 0f)
                {
                    long totalDeltaLength = 0L;
                    foreach (DownloadCounterNode downloadCounterNode in _downloadCounterNodes)
                    {
                        totalDeltaLength += downloadCounterNode.DeltaLength;
                    }

                    _currentSpeed = _accumulator > 0f ? totalDeltaLength / _accumulator : 0f;
                    _timeLeft += _updateInterval;
                }
            }

            public void RecordDeltaLength(int deltaLength)
            {
                if (deltaLength <= 0)
                {
                    return;
                }

                DownloadCounterNode downloadCounterNode = null;
                if (_downloadCounterNodes.Count > 0)
                {
                    downloadCounterNode = _downloadCounterNodes.Last.Value;
                    if (downloadCounterNode.ElapseSeconds < _updateInterval)
                    {
                        downloadCounterNode.AddDeltaLength(deltaLength);
                        return;
                    }
                }

                downloadCounterNode = DownloadCounterNode.Create();
                downloadCounterNode.AddDeltaLength(deltaLength);
                _downloadCounterNodes.AddLast(downloadCounterNode);
            }

            private void Reset()
            {
                _downloadCounterNodes.Clear();
                _currentSpeed = 0f;
                _accumulator = 0f;
                _timeLeft = 0f;
            }
        }
    }
}
