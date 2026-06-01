using System.Threading;
using Cysharp.Threading.Tasks;
using JulyArch;
using JulyGame;
using UnityEngine;

namespace CozyYard
{
    public class MainSceneSetup : SceneSetup,ICanGetStore
    {
        public IArchContext GetArchitecture()
        {
            return GameArch.Context;
        }
        
        protected override void OnEnter()
        {
            LoadSceneView<GridView>("GridView", null, CancellationToken.None).Forget();
            LoadSceneView<TimeLightingView>("TimeLightingView", null, CancellationToken.None).Forget();

            SetupCamera();
            OpenWindow(UIWindowId.GameHUD);
            OpenWindow(UIWindowId.TimeHUD);
            OpenWindow(UIWindowId.WeatherHUD);
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;

            if (!cam.TryGetComponent<CameraController>(out var controller))
                controller = cam.gameObject.AddComponent<CameraController>();

            var gridStore = GetArchitecture().GetStore<GridStore>();
            controller.Initialize(gridStore.Width, gridStore.Height);
        }
    }
}