using System.Collections.Generic;
using UnityEngine;
using Live2D.Cubism.Core;
using System.Linq;

namespace StudioCharacterSDK.Domain
{
    public class AvatarParameterPair : MonoBehaviour
    {
        [SerializeField] private List<AvatarParameterPairData> _pair = new List<AvatarParameterPairData>();
        public IReadOnlyList<AvatarParameterPairData> ParameterPairs => _pair.ToList();
        public IReadOnlyList<CubismParameter> Parameters => _pair.Select( pair => pair.parameter ).ToList();

        public void ClearAndInitializePairList( int count )
        {
            _pair.Clear();
            for( int i = 0; i < count; i++ )
            {
                _pair.Add( new AvatarParameterPairData { paramterType = default( AvatarPartsParameter ) } );
            }
        }

        public void SetPairAtIndex( int index, AvatarPartsParameter parameter )
        {
            if( index >= 0 && index < _pair.Count )
            {
                _pair[index].paramterType = parameter;
            }
        }
    }
}
