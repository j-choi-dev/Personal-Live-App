using System;
using UnityEngine;

namespace StudioResourceSDK.Domain
{
    /// <summary>
    /// Transform 기능 interface
    /// </summary>
    /// <remarks>현재는 2D지만 3D확장 시에도 사용하기 위해 모든 축 정의 @OH 26.11.11</remarks>
    public interface ITransformData
    {
        /// <summary>
        /// 조작 대상 Transform으로 read용으로만 사용할 것. write은 setter를 사용할 것.
        /// </summary>
        Transform TransformPivot { get; }

        /// <summary>
        /// Transform 조작 가능 여부 상태 반환
        /// </summary>
        bool CanTransform { get; }

        //Properties
        float PositionX { get; }
        float PositionY { get; }
        float PositionZ { get; }
        float RotationX { get; }
        float RotationY { get; }
        float RotationZ { get; }
        float ScaleX { get; }
        float ScaleY { get; }
        float ScaleZ { get; }

        //Events
        IObservable<float> OnPositionXChanged { get; }
        IObservable<float> OnPositionYChanged { get; }
        IObservable<float> OnPositionZChanged { get; }
        IObservable<float> OnRotationXChanged { get; }
        IObservable<float> OnRotationYChanged { get; }
        IObservable<float> OnRotationZChanged { get; }
        IObservable<float> OnScaleXChanged { get; }
        IObservable<float> OnScaleYChanged { get; }
        IObservable<float> OnScaleZChanged { get; }

        //Setters
        void SetPositionX(float positionX);
        void SetPositionY(float positionY);
        void SetPositionZ(float positionZ);
        void SetRotationX(float rotationX);
        void SetRotationY(float rotationY);
        void SetRotationZ(float rotationZ);
        void SetScaleX(float scaleX);
        void SetScaleY(float scaleY);
        void SetScaleZ(float scaleZ);
    }
}
