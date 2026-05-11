using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;
using JulyArch;
using JulyCore;

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
            if (_viewRoot != null)
            {
                ParticleFeedbackManager.PlayEraTransitionEffect(_viewRoot);
            }
            GF.Log($"Era transition animation for era {_newEra}");
            await UniTask.Delay(2000, cancellationToken: ct);
            GF.Log($"Era transitioned to {_newEra}");
        }
    }
}
