using System;

namespace LiveAppCore
{
    [System.Serializable]
    public struct ValueRange
    {
        public float min;
        public float max;

        public ValueRange( float min, float max )
        {
            this.min = min;
            this.max = max;
        }

        public static ValueRange Default { get; } = new ValueRange( float.MaxValue, float.MinValue );

        public void ExtendWith( float value )
        {
            if( value < min )
            {
                min = value;
            }
            if( value > max )
            {
                max = value;
            }
        }

        public float Length => max - min;
    }
}
