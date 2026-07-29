using System;

namespace StudioCharacterSDK.Domain
{
    public readonly struct LipSyncVowelData
    {
        public float A { get; }
        public float E { get; }
        public float I { get; }
        public float O { get; }
        public float U { get; }

        public bool IsSilent =>
            A + E + I + O + U <= 0f;

        private LipSyncVowelData(
            float a,
            float e,
            float i,
            float o,
            float u )
        {
            A = a;
            E = e;
            I = i;
            O = o;
            U = u;
        }

        public static LipSyncVowelData Normalize(
            float a,
            float e,
            float i,
            float o,
            float u )
        {
            a = Math.Max( 0f, a );
            e = Math.Max( 0f, e );
            i = Math.Max( 0f, i );
            o = Math.Max( 0f, o );
            u = Math.Max( 0f, u );

            float sum = a + e + i + o + u;

            if( sum <= 0f )
            {
                return default;
            }

            return new LipSyncVowelData(
                a / sum,
                e / sum,
                i / sum,
                o / sum,
                u / sum );
        }
    }
}