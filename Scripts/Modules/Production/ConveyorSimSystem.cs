using UnityEngine;
using IsleWorks.Data;
using JulyArch;
using System.Collections.Generic;

namespace IsleWorks.Systems
{
    /// <summary>
    /// 传送带模拟系统，负责物品的流动逻辑。
    /// </summary>
    public class ConveyorSimSystem : GameSystemBase, IUpdatableSystem
    {
        private List<ConveyorSegment> _conveyorSegments;
        private float _moveTimer;

        public ConveyorSimSystem()
        {
            _conveyorSegments = new List<ConveyorSegment>();
            _moveTimer = 0f;
        }

        /// <summary>
        /// 注册传送带。
        /// </summary>
        public void RegisterSegment(ConveyorSegment segment)
        {
            _conveyorSegments.Add(segment);
            Debug.Log($"Conveyor segment {segment.Id} registered.");
        }

        /// <summary>
        /// 每帧更新。
        /// </summary>
        public void OnUpdate(float deltaTime)
        {
            _moveTimer += deltaTime;
            if (_moveTimer < SimConstants.ConveyorMoveInterval) return;
            _moveTimer -= SimConstants.ConveyorMoveInterval;

            // 遍历传送带，反向处理
            for (int i = _conveyorSegments.Count - 1; i >= 0; i--)
            {
                var segment = _conveyorSegments[i];
                if (segment.Count <= 0) continue;

                // 尝试推进物品到下游
                var item = segment.Slots[segment.HeadIndex];
                var nextSegment = GetNextSegment(segment);

                if (nextSegment != null && nextSegment.TryAcceptItem(item))
                {
                    segment.RemoveItem();
                    segment.IsBlocked = false;
                }
                else
                {
                    segment.IsBlocked = true; // 下游堵塞
                }
            }
        }

        /// <summary>
        /// 获取下游目标。
        /// </summary>
        private ConveyorSegment GetNextSegment(ConveyorSegment segment)
        {
            // 示例逻辑，根据 segment.NextSegmentId 获取下游
            int nextId = segment.NextSegmentId;
            return _conveyorSegments.Find(s => s.Id == nextId);
        }
    }
}