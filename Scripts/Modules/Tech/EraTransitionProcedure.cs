using System.Threading.Tasks;
using UnityEngine;
using JulyArch;

namespace IsleWorks.Procedures
{
    /// <summary>
    /// 时代切换过程，负责处理时代升级的过渡动画。
    /// </summary>
    public class EraTransitionProcedure : Procedure
    {
        private readonly int _newEra;
        private readonly Transform _viewRoot;

        public EraTransitionProcedure(int newEra, Transform viewRoot)
        {
            _newEra = newEra;
            _viewRoot = viewRoot;
        }

        /// <summary>
        /// 执行时代切换动画。
        /// </summary>
        /// <param name="token">任务取消令牌</param>
        /// <returns></returns>
        public override async Task ExecuteAsync(CancellationToken token)
        {
            // 显示过渡画面
            ShowTransitionOverlay();
            await Task.Delay(2000, token); // 假定动画持续 2 秒

            // 执行实际的时代切换逻辑
            Debug.Log($"Era transitioned to {_newEra}");

            // 隐藏过渡画面
            HideTransitionOverlay();
        }

        private void ShowTransitionOverlay()
        {
            var overlay = _viewRoot.Find("EraTransitionOverlay");
            if (overlay)
            {
                overlay.gameObject.SetActive(true);
                Debug.Log("Era transition overlay shown.");
            }
        }

        private void HideTransitionOverlay()
        {
            var overlay = _viewRoot.Find("EraTransitionOverlay");
            if (overlay)
            {
                overlay.gameObject.SetActive(false);
                Debug.Log("Era transition overlay hidden.");
            }
        }
    }
}