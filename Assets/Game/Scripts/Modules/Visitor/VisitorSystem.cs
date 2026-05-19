using System.Collections.Generic;
using System.Linq;
using cfg;
using JulyArch;
using JulyCore;
using UnityEngine;

namespace SpiritHealer
{
    /// <summary>
    /// 来客系统。
    /// 每日早晨按声望权重从配表生成来客队列，管理接诊/遣散流程。
    /// 营业时间结束（进入傍晚）时，未接诊的访客自动离开。
    /// </summary>
    public class VisitorSystem : GameSystemBase
    {
        private const int BaseVisitorCount = 3;
        private const int VisitorCountPerRepTier = 1;
        private const int RepPerTier = 30;

        private VisitorStore _store;
        private PlayerStore _playerStore;
        private TimeSystem _timeSystem;

        protected override void OnInitialize()
        {
            _store = GetStore<VisitorStore>();
            _playerStore = GetStore<PlayerStore>();
            _timeSystem = GetSystem<TimeSystem>();
            Subscribe<PhaseChangedEvent>(OnPhaseChanged);
        }

        private void OnPhaseChanged(PhaseChangedEvent e)
        {
            if (e.NewPhase == ETimePhase.Morning)
            {
                GenerateDailyVisitors();
            }
            else if (e.NewPhase == ETimePhase.Evening)
            {
                DismissRemainingVisitors();
            }
        }

        /// <summary>
        /// 每日清晨根据声望 + 权重随机生成当天来客队列。
        /// </summary>
        public void GenerateDailyVisitors()
        {
            _store.ClearQueue();
            _store.SetCurrentVisitor(null);

            var reputation = _playerStore.Reputation;
            var count = BaseVisitorCount + reputation / RepPerTier * VisitorCountPerRepTier;

            var templates = CfgTable.VisitorTemplate.DataList
                .Where(t => t.MinReputation <= reputation)
                .ToList();

            if (templates.Count == 0) return;

            var totalWeight = templates.Sum(t => t.Weight);

            for (var i = 0; i < count; i++)
            {
                var template = PickByWeight(templates, totalWeight);
                var visitor = CreateVisitorFromTemplate(template);
                _store.AddToQueue(visitor);
            }

            Publish(new VisitorChangedEvent());
        }

        /// <summary>从等候队列中接入下一位来客，消耗看诊时间。</summary>
        public bool AcceptNextVisitor()
        {
            if (_store.CurrentVisitor != null) return false;

            var queue = _store.WaitingQueue;
            if (queue.Count == 0) return false;

            var next = queue[0];
            _store.RemoveFromQueue(next);
            _store.SetCurrentVisitor(next);

            var cause = CfgTable.Cause.GetOrDefault(next.CauseId);
            if (cause != null)
                _timeSystem.ConsumeTime(cause.TimeCost);

            Publish(new VisitorChangedEvent());
            return true;
        }

        /// <summary>
        /// 结算当前来客的治疗结果，发放奖励后送走。
        /// </summary>
        public void CompleteTreatment(float efficacyScore)
        {
            var visitor = _store.CurrentVisitor;
            if (visitor == null) return;

            visitor.Treated = true;
            visitor.TreatmentScore = efficacyScore;
            _store.IncrementTreated();

            int repReward = visitor.BaseReputation;
            int coinReward = visitor.BaseCoin;

            if (efficacyScore >= 90f)
            {
                repReward = (int)(repReward * 1.5f);
                coinReward = (int)(coinReward * 1.5f);
                _store.IncrementCured();
            }
            else if (efficacyScore >= 70f)
            {
                // standard reward
            }
            else if (efficacyScore >= 50f)
            {
                repReward = (int)(repReward * 0.5f);
                coinReward = (int)(coinReward * 0.5f);
            }
            else
            {
                repReward = 0;
                coinReward = (int)(coinReward * 0.3f);
            }

            _playerStore.AddReputation(repReward);
            _playerStore.AddCoins(coinReward);

            Publish(new TreatmentCompletedEvent
            {
                Visitor = visitor,
                EfficacyScore = efficacyScore,
                ReputationGained = repReward,
                CoinsGained = coinReward
            });

            _store.SetCurrentVisitor(null);
            Publish(new VisitorChangedEvent());
        }

        /// <summary>送走当前来客（主动拒诊，无奖励）。</summary>
        public void DismissCurrentVisitor()
        {
            _store.SetCurrentVisitor(null);
            Publish(new VisitorChangedEvent());
        }

        private void DismissRemainingVisitors()
        {
            _store.ClearQueue();
            _store.SetCurrentVisitor(null);
            Publish(new VisitorChangedEvent());
        }

        private VisitorInstance CreateVisitorFromTemplate(VisitorTemplate template)
        {
            var causeId = template.CauseIds[Random.Range(0, template.CauseIds.Count)];
            return new VisitorInstance
            {
                TemplateId = template.Id,
                Name = template.Name,
                Type = (VisitorType)template.Type,
                CauseId = causeId,
                TimeCost = CfgTable.Cause.GetOrDefault(causeId)?.TimeCost ?? 5,
                BaseReputation = template.BaseReputation,
                BaseCoin = template.BaseCoin
            };
        }

        private static VisitorTemplate PickByWeight(List<VisitorTemplate> templates, int totalWeight)
        {
            var roll = Random.Range(0, totalWeight);
            var cumulative = 0;
            foreach (var t in templates)
            {
                cumulative += t.Weight;
                if (roll < cumulative) return t;
            }
            return templates[^1];
        }
    }
}
