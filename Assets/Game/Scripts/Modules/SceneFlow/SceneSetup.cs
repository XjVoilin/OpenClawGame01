using System.Collections.Generic;
using JulyCore;
using JulyCore.Data.UI;
using UnityEngine;

namespace CozyYard
{
    /// <summary>
    /// 场景搭建基类：定义某个场景进入/退出时需要的 UI 窗口和场景 View。
    /// 子类重写 OnEnter 通过 OpenWindow / CreateSceneView 声明需求，
    /// Exit 时自动关闭所有已打开的窗口（场景内 View 随场景卸载自动销毁）。
    /// </summary>
    public abstract class SceneSetup
    {
        private readonly List<int> _openedWindows = new();

        public void Enter()
        {
            OnEnter();
        }

        public void Exit()
        {
            OnExit();

            foreach (int id in _openedWindows)
                GF.UI.Close(id, destroy: true, UIAnimationType.None);
            _openedWindows.Clear();
        }

        protected virtual void OnEnter() { }
        protected virtual void OnExit() { }

        protected void OpenWindow(int windowId)
        {
            GF.UI.Open(windowId);
            _openedWindows.Add(windowId);
        }

        protected T CreateSceneView<T>(string name) where T : MonoBehaviour
        {
            var go = new GameObject(name);
            return go.AddComponent<T>();
        }
    }
}
