using System;
using JulyArch;
using JulyCore.Provider.UI;
using JulyGame;
using UnityEngine;

namespace CozyYard
{
    /// <summary>
    /// UI窗口基类：桥接 UIBase (JulyCore) 和 JulyArch 的 Store/System/Event 能力
    /// 所有通过 GF.UI.Open 打开的窗口应继承此类
    /// 场景常驻View（如GridView、TimeLightingView）继续使用 GameView
    /// </summary>
    public abstract class GameUIView : UIBase, ICanGetStore, ICanEvent, ICanGetSystem
    {
        public IArchContext GetArchitecture() => GameArch.Context;

        protected override void OnBeforeOpen()
        {
            OnViewEnable();
        }

        protected override void OnClose()
        {
            UnsubscribeAll();
            OnViewDisable();
        }

        protected virtual void OnViewEnable() { }
        protected virtual void OnViewDisable() { }

        protected T GetStore<T>() where T : StoreBase
            => GetArchitecture().GetStore<T>();

        protected void Subscribe<T>(Action<T> handler)
            => GetArchitecture().Event.Subscribe(handler, this);

        protected void Unsubscribe<T>(Action<T> handler)
            => GetArchitecture().Event.Unsubscribe(handler);

        protected void Publish<T>(T eventData)
            => GetArchitecture().Event.Publish(eventData);

        protected T GetSystem<T>() where T : GameSystemBase
            => GetArchitecture().GetSystem<T>();

        private void UnsubscribeAll()
        {
            GetArchitecture().Event.UnsubscribeAll(this);
        }
    }
}
