using UnityEngine;
using UnityEngine.Events;

namespace StudioResourceSDK.Domain
{
    public class ToggleEvent : EventCollectionElement
    {
        [SerializeField] private UnityEvent<bool> onValueChanged = new UnityEvent<bool>();

        public override EventDataType DataType => EventDataType.Bool;

        public UnityEvent<bool> OnValueChanged => onValueChanged;

        public override bool Invoke(bool value)
        {
            onValueChanged.Invoke(value);
            return true;
        }
    }
}