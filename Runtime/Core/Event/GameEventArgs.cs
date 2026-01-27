//------------------------------------------------------------
// Game Framework
// Copyright © 2013-2021 Jiang Yin. All rights reserved.
// Homepage: https://gameframework.cn/
// Feedback: mailto:ellan@gameframework.cn
//------------------------------------------------------------

namespace EasyGameFramework.Core.Event
{
    /// <summary>
    /// 游戏逻辑事件基类。
    /// </summary>
    public abstract class GameEventArgs : BaseEventArgs
    {
        private int? _id = 0;
        
        public override int Id
        {
            get
            {
                if (_id == null)
                {
                    _id = EventExtensions.GetEventId(GetType());
                }
                
                return _id.Value;
            }
        }
    }
}
