using System;
using System.Collections.Generic;
using System.Text;
using System.Windows.Forms;
using Cysharp.Threading.Tasks;
using StudioResourceSDK.Domain;
using UnityEngine;

namespace StudioResourceSDK.Infrastructure
{
    /// <summary>
    /// ITransformData 구현체들이 공통으로 사용하는 Transform 조작 처리클래스
    /// </summary>
    /// <remarks>데이터 처리만 하고 이벤트 발행은 하지 않음.발행은 구현체의 setter로</remarks>
    public class TransformDataHandler
    {
        private readonly Transform _target;
        public Transform Target => _target;
        /// <summary>
        /// 조작할 대상의 transform이 연결돼 있는 지 판별
        /// </summary>
        public bool CanTransform => _target != null;
        /// <summary>
        /// 회전은 Transform에서 역산하지 않고 각도를 직접 기억하는 방식(360넘김, 축 뒤집힘 방지)
        /// </summary>
        private Vector3 _rotation;
        private bool _isRotationCached = false;


        public TransformDataHandler(Transform target)
        {
            _target = target;
        }

        /// <summary>
        /// 해당 축의 position값 반환
        /// </summary>
        public float GetPosition(TransformAxis axis)
        {
            return CanTransform ? GetAxisValue(_target.localPosition, axis) : 0.0f;
        }

        /// <summary>
        /// 해당 축의 rotation값 반환
        /// </summary>
        public float GetRotation(TransformAxis axis)
        {
            if(CanTransform == false)
            {
                return 0.0f;
            }

            CacheRotationIfNeed();
        
            return GetAxisValue(_rotation, axis);
        }

        /// <summary>
        /// 해당 축의 scale값 반환
        /// </summary>
        public float GetScale(TransformAxis axis)
        {
            return CanTransform ? GetAxisValue(_target.localScale, axis) : 0.0f;
        }

        /// <summary>
        /// 해당 축의 position 적용
        /// </summary>
        /// <returns>조작 불가 or 같은 값이면 false</returns>
        public bool TrySetPosition(TransformAxis axis, float value)
        {
            if(CanTransform == false)
            {
                return false;
            }

            Vector3 pos = _target.localPosition;
            if(Mathf.Approximately(GetAxisValue(pos, axis), value ) )
            {
                return false;
            }

            _target.localPosition = GetVectorChangedWithAxis(pos, axis, value);
            return true;
        }

        /// <summary>
        /// 해당 축의 rotation 적용, 저장해놓은 세 축의 각도로 회전을 다시 계산
        /// </summary>
        /// <returns>조작 불가 or 같은 값이면 false</returns>
        public bool TrySetRotation(TransformAxis axis, float value)
        {
            if(CanTransform == false)
            {
                return false;
            }

            CacheRotationIfNeed();
            if(Mathf.Approximately(GetAxisValue(_rotation, axis), value ) )
            {
                return false;
            }

            _rotation = GetVectorChangedWithAxis(_rotation, axis, value);
            _target.localRotation = Quaternion.Euler(_rotation);     //저장한 세 축 각도로 회전을 다시 계산해서 덮어씀
            return true;
        }

        /// <summary>
        /// 해당 축의 scale 적용
        /// </summary>
        /// <returns>조작 불가 or 같은 값이면 false</returns>
        public bool TrySetScale(TransformAxis axis, float value)
        {
            if(CanTransform == false)
            {
                return false;
            }

            Vector3 sc = _target.localScale;
            if(Mathf.Approximately(GetAxisValue(sc, axis), value ) )
            {
                return false;
            }

            _target.localScale = GetVectorChangedWithAxis(sc, axis, value);
            return true;
        }

        /// <summary>
        /// 처음 한 번만 Transform 각도를 읽어서 시작 기준값으로 사용
        /// </summary>
        private void CacheRotationIfNeed()
        {
            //이미 캐시되었으면 이후 처리x
            if(_isRotationCached)
            {
                return;
            }

            _rotation = _target.localEulerAngles;
            _isRotationCached = true;
        }

        /// <summary>
        /// 축 enum -> 벡터성분으로 반환
        /// </summary>
        private float GetAxisValue(Vector3 vector, TransformAxis axis)
        {
            switch(axis)
            {
                case TransformAxis.X: return vector.x;
                case TransformAxis.Y: return vector.y;
                case TransformAxis.Z: return vector.z;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, "x,y,z 축만 지원");
            }
        }

        /// <summary>
        /// 해당 축의 값만 바꾼 벡터를 반환
        /// </summary>
        /// <remarks> Vector3는 값 타입이라 복사본 수정해서 반환</remarks>
        private Vector3 GetVectorChangedWithAxis(Vector3 vector, TransformAxis axis, float value)
        {
            switch (axis)
            {
                case TransformAxis.X: vector.x = value; break;
                case TransformAxis.Y: vector.y = value; break;
                case TransformAxis.Z: vector.z = value; break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(axis), axis, "x,y,z축만 지원");
            }

            return vector;
        }
    }
}
