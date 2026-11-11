using StudioResourceSDK.Domain;
using UnityEngine;
using UniRx;
using System;

namespace StudioResourceSDK.Infrastructure
{
    public class PropResource : MonoBehaviour, IProp, ITransformData
    {
        [SerializeField] private Transform _transformPivot = null;  //프리팹에서 TransfomrPivot 연결
        /// <summary>
        /// // Transform 조작 처리핸들러. Awake에서 한 번만 생성
        /// </summary>
        private TransformDataHandler _transformHandler;

        //---------------------------------------------------------------------------------------ITransformData
        public Transform TransformPivot => _transformPivot;
        private readonly Subject<float> _onPositionXChanged = new Subject<float>();
        private readonly Subject<float> _onPositionYChanged = new Subject<float>();
        private readonly Subject<float> _onPositionZChanged = new Subject<float>();
        private readonly Subject<float> _onRotationXChanged = new Subject<float>();
        private readonly Subject<float> _onRotationYChanged = new Subject<float>();
        private readonly Subject<float> _onRotationZChanged = new Subject<float>();
        private readonly Subject<float> _onScaleXChanged = new Subject<float>();
        private readonly Subject<float> _onScaleYChanged = new Subject<float>();
        private readonly Subject<float> _onScaleZChanged = new Subject<float>();

        public IObservable<float> OnPositionXChanged => _onPositionXChanged;
        public IObservable<float> OnPositionYChanged => _onPositionYChanged;
        public IObservable<float> OnPositionZChanged => _onPositionZChanged;
        public IObservable<float> OnRotationXChanged => _onRotationXChanged;
        public IObservable<float> OnRotationYChanged => _onRotationYChanged;
        public IObservable<float> OnRotationZChanged => _onRotationZChanged;
        public IObservable<float> OnScaleXChanged => _onScaleXChanged;
        public IObservable<float> OnScaleYChanged => _onScaleYChanged;
        public IObservable<float> OnScaleZChanged => _onScaleZChanged;

        public float PositionX => _transformHandler.GetPosition(TransformAxis.X);
        public float PositionY => _transformHandler.GetPosition(TransformAxis.Y);
        public float PositionZ => _transformHandler.GetPosition(TransformAxis.Z);
        public float RotationX => _transformHandler.GetRotation(TransformAxis.X);
        public float RotationY => _transformHandler.GetRotation(TransformAxis.Y);
        public float RotationZ => _transformHandler.GetRotation(TransformAxis.Z);
        public float ScaleX => _transformHandler.GetScale(TransformAxis.X);
        public float ScaleY => _transformHandler.GetScale(TransformAxis.Y);
        public float ScaleZ => _transformHandler.GetScale(TransformAxis.Z);

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

            _transformHandler = new TransformDataHandler(_transformPivot);

            if (_transformPivot == null)
            {
                Debug.LogError($"[PropResource] _transformPivot이 연결되지 않아 Transform 조작이 불가합니다. Name={name}", this);
            }
        }

        //-----------------------------------------------------------------Transform methods
        //setter들은 실제로 값이 바뀌었을 때만 이벤트 발행하도록 했음
        public void SetPositionX(float value)
        {
            if (_transformHandler.TrySetPosition(TransformAxis.X, value) ) 
            { 
                _onPositionXChanged.OnNext(value); 
            }
        }

        public void SetPositionY(float value)
        {
            if (_transformHandler.TrySetPosition(TransformAxis.Y, value) ) 
            { 
                _onPositionYChanged.OnNext(value); 
            }
        }

        public void SetPositionZ(float value)
        {
            if (_transformHandler.TrySetPosition(TransformAxis.Z, value) ) 
            {
                _onPositionZChanged.OnNext(value); 
            }
        }

        public void SetRotationX(float value)
        {
            if (_transformHandler.TrySetRotation(TransformAxis.X, value) ) 
            { 
                _onRotationXChanged.OnNext(value); 
            }
        }

        public void SetRotationY(float value)
        {
            if (_transformHandler.TrySetRotation(TransformAxis.Y, value) ) 
            { 
                _onRotationYChanged.OnNext(value); 
            }
        }

        public void SetRotationZ(float value)
        {
            if (_transformHandler.TrySetRotation(TransformAxis.Z, value) ) 
            { 
                _onRotationZChanged.OnNext(value); 
            }
        }

        public void SetScaleX(float value)
        {
            if (_transformHandler.TrySetScale(TransformAxis.X, value) ) 
            { 
                _onScaleXChanged.OnNext(value); 
            }
        }

        public void SetScaleY(float value)
        {
            if (_transformHandler.TrySetScale(TransformAxis.Y, value) ) 
            { 
                _onScaleYChanged.OnNext(value); 
            }
        }

        public void SetScaleZ(float value)
        {
            if (_transformHandler.TrySetScale(TransformAxis.Z, value) ) 
            { 
                _onScaleZChanged.OnNext(value); 
            }
        }
        //-----------------------------------------------------------------/Transform methods

    }
}
