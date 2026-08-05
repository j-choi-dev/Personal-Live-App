using System;
using UniRx;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// 메인메뉴 View
    /// </summary>
    public interface IMainMenuView : IViewBase
    {
        /// <summary>
        /// 리소스 버튼 클릿 이벤트
        /// </summary>
        IObservable<Unit> OnResourceButtonClick { get; }
        IObservable<Unit> OnConfigButtonClick { get; }
    }
}
