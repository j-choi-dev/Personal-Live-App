

namespace StudioResourceSDK.Domain
{
    /// <summary>
    /// 렌더 카메라 기준 Viewport 좌표 구조체(0~1)
    /// Vector2 대신 사용 나중 유니티엔진 의존이 가능한 레이어에서 변환해서 사용
    /// </summary>
    public readonly struct ViewportPoint
    {
        public float X { get; }
        public float Y { get; }

        public ViewportPoint(float x, float y)
        {
            X = x;
            Y = y;
        }

        public override string ToString() => $"({X:F3}, {Y:F3})";
    }
}
