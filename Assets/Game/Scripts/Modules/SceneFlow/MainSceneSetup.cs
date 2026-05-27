using JulyArch;
using JulyGame;
using UnityEngine;

namespace CozyYard
{
    public class MainSceneSetup : SceneSetup
    {
        protected override void OnEnter()
        {
            CreateSceneView<GridView>("[GridView]");
            CreateSceneView<TimeLightingView>("[Lighting]");
            SetupCamera();
            OpenWindow(UIWindowId.GameHUD);
            OpenWindow(UIWindowId.TimeHUD);
            OpenWindow(UIWindowId.WeatherHUD);
        }

        private void SetupCamera()
        {
            var cam = Camera.main;
            if (cam == null) return;

            var controller = cam.gameObject.GetComponent<CameraController>();
            if (controller == null)
                controller = cam.gameObject.AddComponent<CameraController>();

            var gridStore = GameArch.Context.GetStore<GridStore>();
            controller.Initialize(gridStore.Width, gridStore.Height);
        }
    }
}
