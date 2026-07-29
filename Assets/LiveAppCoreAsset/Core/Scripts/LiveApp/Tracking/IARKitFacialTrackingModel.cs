namespace LiveApp
{
    public interface IARKitFacialTrackingModel
    {
        bool IsAbleTracking { get; }
        float Intensity { get; }

        void SetAbleTracking( bool isOK );

        void SetIntensity( float intensity );
    }
}