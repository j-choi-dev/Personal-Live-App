using StudioResourceSDK.Domain;
using UnityEngine;
using UniRx;
using System;

namespace StudioResourceSDK.Infrastructure
{
    public class PropResource : MonoBehaviour, IProp, ITransformData
    {
        [SerializeField] private Transform _transformPivot = null;  //프리팹에서 TransfomrPivot 연결

        //---------------------------------------------------------------------------------------ITransformData
        public Transform TransformPivot => _transformPivot;
        public bool CanTransform => _transformPivot != null;
        private Subject<float> _onPositionXChanged = new Subject<float>();
        private Subject<float> _onPositionYChanged = new Subject<float>();
        private Subject<float> _onPositionZChanged = new Subject<float>();
        private Subject<float> _onRotationXChanged = new Subject<float>();
        private Subject<float> _onRotationYChanged = new Subject<float>();
        private Subject<float> _onRotationZChanged = new Subject<float>();
        private Subject<float> _onScaleXChanged = new Subject<float>();
        private Subject<float> _onScaleYChanged = new Subject<float>();
        private Subject<float> _onScaleZChanged = new Subject<float>();

        public IObservable<float> OnPositionXChanged => _onPositionXChanged;
        public IObservable<float> OnPositionYChanged => _onPositionYChanged;
        public IObservable<float> OnPositionZChanged => _onPositionZChanged;
        public IObservable<float> OnRotationXChanged => _onRotationXChanged;
        public IObservable<float> OnRotationYChanged => _onRotationYChanged;
        public IObservable<float> OnRotationZChanged => _onRotationZChanged;
        public IObservable<float> OnScaleXChanged => _onScaleXChanged;
        public IObservable<float> OnScaleYChanged => _onScaleYChanged;
        public IObservable<float> OnScaleZChanged => _onScaleZChanged;

        public float PositionX => _transformPivot.localPosition.x;
        public float PositionY => _transformPivot.localPosition.y;
        public float PositionZ => _transformPivot.localPosition.z;
        public float RotationX => _transformPivot.localEulerAngles.x;
        public float RotationY => _transformPivot.localEulerAngles.y;
        public float RotationZ => _transformPivot.localEulerAngles.z;
        public float ScaleX => _transformPivot.localScale.x;
        public float ScaleY => _transformPivot.localScale.y;
        public float ScaleZ => _transformPivot.localScale.z;

        //---------------------------------------------------------------------------------------/ITransformData

        public string ID { get; private set; }

        public void SetID(string id)
            => ID = id;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            Debug.Log($"Prop :: {ID} Init");

            if (CanTransform)
            {
                Debug.LogError($"[PropResource] _transformPivot이 연결되지 않아 Transform 조작이 불가합니다. Name={name}", this);
            }
        }

        //-----------------------------------------------------------------Transform methods
        //setter들은 실제로 값이 바뀌었을 때만 이벤트 발행하도록 했음
        public void SetPositionX(float positionX)
        {
            Vector3 position = _transformPivot.localPosition;
            if (Mathf.Approximately(position.x, positionX))
            {
                return;
            }

            position.x = positionX;
            _transformPivot.localPosition = position;
            _onPositionXChanged.OnNext(positionX);
        }
        public void SetPositionY(float positionY)
        {
            Vector3 position = _transformPivot.localPosition;
            if (Mathf.Approximately(position.y, positionY))
            {
                return;
            }

            position.y = positionY;
            _transformPivot.localPosition = position;
            _onPositionYChanged.OnNext(positionY);
        }
        public void SetPositionZ(float positionZ)
        {
            Vector3 position = _transformPivot.localPosition;
            if (Mathf.Approximately(position.z, positionZ))
            {
                return;
            }

            position.z = positionZ;
            _transformPivot.localPosition = position;
            _onPositionZChanged.OnNext(positionZ);
        }
        public void SetRotationX(float rotationX)
        {
            Vector3 rotation = _transformPivot.localEulerAngles;
            if (Mathf.Approximately(Mathf.DeltaAngle(rotation.x, rotationX), 0.0f))
            {
                return;
            }

            rotation.x = rotationX;
            _transformPivot.localRotation = Quaternion.Euler(rotation);
            _onRotationXChanged.OnNext(rotationX);
        }
        public void SetRotationY(float rotationY)
        {
            Vector3 rotation = _transformPivot.localEulerAngles;
            if (Mathf.Approximately(Mathf.DeltaAngle(rotation.y, rotationY), 0.0f))
            {
                return;
            }

            rotation.y = rotationY;
            _transformPivot.localRotation = Quaternion.Euler(rotation);
            _onRotationYChanged.OnNext(rotationY);
        }
        public void SetRotationZ(float rotationZ)
        {
            Vector3 rotation = _transformPivot.localEulerAngles;
            if (Mathf.Approximately(Mathf.DeltaAngle(rotation.z, rotationZ), 0.0f))
            {
                return;
            }

            rotation.z = rotationZ;
            _transformPivot.localRotation = Quaternion.Euler(rotation);
            _onRotationZChanged.OnNext(rotationZ);
        }
        public void SetScaleX(float scaleX)
        {
            Vector3 scale = _transformPivot.localScale;
            if (Mathf.Approximately(scale.x, scaleX))
            {
                return;
            }

            scale.x = scaleX;
            _transformPivot.localScale = scale;
            _onScaleXChanged.OnNext(scaleX);
        }
        public void SetScaleY(float scaleY)
        {
            Vector3 scale = _transformPivot.localScale;
            if (Mathf.Approximately(scale.y, scaleY))
            {
                return;
            }

            scale.y = scaleY;
            _transformPivot.localScale = scale;
            _onScaleYChanged.OnNext(scaleY);
        }
        public void SetScaleZ(float scaleZ)
        {
            Vector3 scale = _transformPivot.localScale;
            if (Mathf.Approximately(scale.z, scaleZ))
            {
                return;
            }

            scale.z = scaleZ;
            _transformPivot.localScale = scale;
            _onScaleZChanged.OnNext(scaleZ);
        }
        //-----------------------------------------------------------------/Transform methods

    }
}
