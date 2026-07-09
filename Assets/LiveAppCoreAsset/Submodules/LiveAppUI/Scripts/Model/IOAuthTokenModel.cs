using Cysharp.Threading.Tasks;
using System;

namespace LiveAppUI.Model
{
    /// <summary>
    /// IOuthToken 취득을 위한 Model Class
    /// </summary>
    public interface IOAuthTokenModel
    {
        /// <summary>
        /// 토큰 취득 완료 이벤트
        /// </summary>
        IObservable<bool> OnCompleteTokenProcess { get; }

        /// <summary>
        /// 토큰 취득을 위한 초기화 작업 Process
        /// </summary>
        /// <returns>취득 성공 여부</returns>
        /// <remarks>Start 타이밍에서 호출 필수</remarks>
        UniTask<bool> InitilizeAuthProcess();
    }
}
