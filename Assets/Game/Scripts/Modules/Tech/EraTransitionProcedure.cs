using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using JulyArch;

namespace IsleWorks.Tech
{
    /// <summary>
    /// 时代切换过程，负责处理时代升级的过渡动画。
    /// </summary>
    public class EraTransitionProcedure : ProcedureBase
    {
        private readonly int _newEra;
        private readonly Transform _viewRoot;

        public EraTransitionProcedure(int newEra, Transform viewRoot)
        {
            _newEra = newEra;
            _viewRoot = viewRoot;
        }

        public override async UniTask ExecuteAsync(CancellationToken ct)
        {
            ShowTransitionOverlay();
            ParticleFeedbackManager.PlayEraTransitionEffect(_viewRoot);
            await UniTask.Delay(2000, cancellationToken: ct);
            Debug.Log($"Era transitioned to {_newEra}");
            HideTransitionOverlay();
        }

        private void ShowTransitionOverlay()
        {
            var overlay = _viewRoot.Find("EraTransitionOverlay");
            if (overlay)
            {
                overlay.gameObject.SetActive(true);
            }
        }

        private void HideTransitionOverlay()
        {
            var overlay = _viewRoot.Find("EraTransitionOverlay");
            if (overlay)
            {
                overlay.gameObject.SetActive(false);
            }
        }
    }
}
