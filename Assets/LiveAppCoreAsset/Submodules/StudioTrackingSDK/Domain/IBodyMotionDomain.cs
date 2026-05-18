using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace MotionTracking.Domain
{
    public interface IBodyTrackingDomain
    {
        IObservable<float> OnBodyAngleXChanged { get; }
        IObservable<float> OnBodyAngleYChanged { get; }
        IObservable<float> OnBodyAngleZChanged { get; }
        IObservable<float> OnBodyPositionXChanged { get; }
        IObservable<float> OnBodyPositionYChanged { get; }
        IObservable<float> OnBodyPositionZChanged { get; }
    }
}
