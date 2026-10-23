using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAppUI.Domain
{
    ///Inspector에서 조절하는 transform 속성(ui용)
    /// model에서 resourceDomain -> UIDomain으로 convert해서 사용할 예정

    /// <summary>
    /// StudioResourceSDK.Domain의 TransformProperty과 매핑되는 enum
    /// </summary>
    public enum InspectorTransformProperty
    {
        Position,
        Rotation,
        Scale,
    }

    /// <summary>
    /// StudioResourceSDK.Domain의 TransformAxis과 매핑되는 enum
    /// </summary>
    public enum InspectorTransformAxis
    {
        X,
        Y,
        Z,
    }

}
