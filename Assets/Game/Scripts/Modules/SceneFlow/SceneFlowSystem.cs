using System.Collections.Generic;
using JulyArch;
using JulyCore;
using JulyCore.Provider.Scene.Events;
using JulyCore.Provider.UI;

namespace CozyYard
{
    /// <summary>
    /// 场景流程系统：监听场景切换事件，管理 URP Camera Stack，自动执行对应 SceneSetup 的进入/退出。
    /// </summary>
    public class SceneFlowSystem : GameSystemBase
    {
        private readonly Dictionary<string, SceneSetup> _setups = new();
        private SceneSetup _currentSetup;

        public string CurrentSceneName { get; private set; }

        protected override void OnInitialize()
        {
            RegisterSetup("Main", new MainSceneSetup());
            GF.CoreEvent.Subscribe<SceneSwitchStartEvent>(OnSceneSwitchStart, this);
            GF.CoreEvent.Subscribe<SceneLoadCompleteEvent>(OnSceneLoadComplete, this);
            GF.CoreEvent.Subscribe<SceneSwitchCompleteEvent>(OnSceneSwitchComplete, this);

            CameraStackHandler.RebuildStack();
        }

        protected override void OnShutdown()
        {
            GF.CoreEvent.UnsubscribeAll(this);
        }

        public void RegisterSetup(string sceneName, SceneSetup setup)
        {
            _setups[sceneName] = setup;
        }

        private void OnSceneSwitchStart(SceneSwitchStartEvent evt)
        {
            CameraStackHandler.PrepareForSceneSwitch();
        }

        private void OnSceneLoadComplete(SceneLoadCompleteEvent evt)
        {
            CameraStackHandler.RebuildStack();
        }

        private void OnSceneSwitchComplete(SceneSwitchCompleteEvent evt)
        {
            _currentSetup?.Exit();
            _currentSetup = null;
            CurrentSceneName = evt.ToSceneName;

            if (_setups.TryGetValue(evt.ToSceneName, out var setup))
            {
                _currentSetup = setup;
                _currentSetup.Enter();
            }
        }
    }
}
