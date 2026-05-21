namespace CozyYard
{
    /// <summary>Main 场景：完整游戏玩法界面</summary>
    public class MainSceneSetup : SceneSetup
    {
        protected override void OnEnter()
        {
            CreateSceneView<GridView>("[GridView]");
            OpenWindow(UIWindowId.GameHUD);
            OpenWindow(UIWindowId.TimeHUD);
            OpenWindow(UIWindowId.WeatherHUD);
        }
    }
}
