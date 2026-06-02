namespace LiveAppUI.Presenter
{
    public interface IViewBase
    {
        bool IsActive { get; }
        void SetActive( bool isActive );
    }
}
