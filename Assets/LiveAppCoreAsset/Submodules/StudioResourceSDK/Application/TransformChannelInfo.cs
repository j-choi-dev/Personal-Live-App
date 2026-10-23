using LiveAppCore;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using System.Text;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// Inspector에서 조절 가능한 항목 하나
    /// Model에서 해당 데이터를 받아서 어떤 슬라이더 띄우고 범위를 지정할 지 정함
    /// </summary>
    public class TransformChannelInfo
    {
        public TransformProperty Property { get; }
        public TransformAxis Axis { get; }
        public ValueRange Range { get; }

        public TransformChannelInfo(
            TransformProperty property,
            TransformAxis axis,
            ValueRange range)
        {
            Property = property;
            Axis = axis;
            Range = range;
        }
           
    }
}
