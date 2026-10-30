namespace LiveApp
{
    public interface IARKitBodyTrackingModel
    {
        bool IsAbleTracking { get; }
        float Intensity { get; }

        void SetAbleTracking(bool isOK);

        void SetIntensity(float intensity);
    }
}
