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
        private int? m_Id = 0;
        
        public override int Id
        {
            get
            {
                if (m_Id == null)
                {
                    m_Id = EventExtensions.GetEventId(GetType());
                }
                
                return m_Id.Value;
            }
        }
    }
}
