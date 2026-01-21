using UnityEngine;
using System;
using UniRx;

namespace LiveAppUI.View
{
    public class ObservableVector3Input : MonoBehaviour
    {
        [SerializeField] private ObservableFloatInput X = null;
        [SerializeField] private ObservableFloatInput Y = null;
        [SerializeField] private ObservableFloatInput Z = null;
        [SerializeField] private ObservableButton _copy = null;
        [SerializeField] private ObservableButton _paste = null;

        private Vector3 m_Value = new Vector3();
        public Vector3 Value
        {
            get => m_Value;
            set { SetValueWithoutNotify( value ); m_ValueChanged.OnNext( value ); }
        }

        private Subject<Vector3> m_ValueChanged = new Subject<Vector3>();
        public IObservable<Vector3> OnValueChanged => m_ValueChanged;

        public bool Interactable
        {
            get
            {
                return X.Interactable;
            }
            set
            {
                X.Interactable = value;
                Y.Interactable = value;
                Z.Interactable = value;
                _copy.Interactable = value;
                _paste.Interactable = value;
            }
        }

        private void Awake()
        {
            X.OnValueChanged.AsObservable()
                .Subscribe( newX => Value = new Vector3( newX, Value.y, Value.z ) )
                .AddTo( this );

            Y.OnValueChanged.AsObservable()
                .Subscribe( newY => Value = new Vector3( Value.x, newY, Value.z ) )
                .AddTo( this );

            Z.OnValueChanged.AsObservable()
                .Subscribe( newZ => Value = Value = new Vector3( Value.x, Value.y, newZ ) )
                .AddTo( this );

            _copy.OnClick
                .Subscribe( _ =>
                {
                    var msg = $"{X.Value},{Y.Value},{Z.Value}";
                    GUIUtility.systemCopyBuffer = msg;
                } )
                .AddTo( this );

            _paste.OnClick
                .Subscribe( _ =>
                {
                    var convert = ConvertStringToVector3( GUIUtility.systemCopyBuffer );
                    if( !convert.isValid )
                    {
                        return;
                    }
                    Value = convert.value;
                } )
                .AddTo( this );
        }

        public void SetValueWithoutNotify( Vector3 value )
        {
            m_Value = value;

            X.SetValueWithoutNotify( value.x );
            Y.SetValueWithoutNotify( value.y );
            Z.SetValueWithoutNotify( value.z );
        }

        public void SetIndeterminateValue()
        {
            X.SetIndeterminateValue();
            Y.SetIndeterminateValue();
            Z.SetIndeterminateValue();
        }
        private (bool isValid, Vector3 value) ConvertStringToVector3( string msg )
        {
            var rawValue = msg.Split(',');
            if( rawValue.Length != 3 ||
                !float.TryParse( rawValue[0], out float x ) ||
                !float.TryParse( rawValue[1], out float y ) ||
                !float.TryParse( rawValue[2], out float z ) )
            {
                return (false, default);
            }
            var vec = new Vector3(x, y, z);
            return (true, vec);
        }
    }
}
