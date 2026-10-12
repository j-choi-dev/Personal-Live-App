using UnityEngine;
using UnityEngine.Events;

namespace StudioResourceSDK.Domain
{
    public class TransformFloatEvent : EventCollectionElement
    {
        [SerializeField] private UnityEvent<float> onValueChanged = new UnityEvent<float>();

        public override EventDataType DataType => EventDataType.Float;

        public UnityEvent<float> OnValueChanged => onValueChanged;

        public override bool Invoke(float value)
        {
            if (float.IsNaN(value) || float.IsInfinity(value))
            {
                Debug.LogError($"TransformFloatEvent 값이 올바르지 않습니다. EventID={EventID}, Value={value}", this);
                return false;
            }

            onValueChanged.Invoke(value);
            return true;
        }
    }
}