namespace LiveAppUI.Presenter
{
    /// <summary>
    /// UI View에 대한 기본/공통 Interface
    /// </summary>
    public interface IViewBase
    {
        /// <summary>
        /// View Active 상태
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// 로그인창 활성화/비활성화
        /// </summary>
        /// <param name="isActive">Active값</param>
        void SetActive( bool isActive );
    }
}
