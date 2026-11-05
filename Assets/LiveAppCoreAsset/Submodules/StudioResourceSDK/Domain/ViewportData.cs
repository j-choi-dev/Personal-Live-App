

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

    /// <summary>
    /// 렌더 카메라 기준 Viewport 사각형 구조체(0~1) 좌하단 (0,0) 우상단(1,1)
    /// 화면 밖으로 나간 부분은 0~1을 벗어날 수 있음
    /// Rect 대신 사용하여 엔진 의존성 없애고 의존 가능한 레이어에서 변환해서 사용
    /// </summary>
    public readonly struct ViewportRect
    {
        public float MinX { get; }
        public float MinY { get; }
        public float MaxX { get; }
        public float MaxY { get; }

        public ViewportRect(float minX, float minY, float maxX, float maxY)
        {
            MinX = minX;
            MinY = minY;
            MaxX = maxX;
            MaxY = maxY;
        }

        public override string ToString() => $"({MinX:F3}, {MinY:F3}) ~ ({MaxX:F3}, {MaxY:F3})";
    }

}
