using UnityEngine;
using UnityEngine.Events;

namespace StudioResourceSDK.Domain
{
    public class ToggleEvent : EventCollectionElement
    {
        [SerializeField] private bool _defaultValue;
        [SerializeField] private UnityEvent<bool> onValueChanged = new UnityEvent<bool>();

        private bool _currentValue;

        public override EventDataType DataType => EventDataType.Bool;
        public UnityEvent<bool> OnValueChanged => onValueChanged;

        private void Awake()
        {
            _currentValue = _defaultValue;
        }

        private void Start()
        {
            Invoke(_currentValue);
        }

        public override bool Invoke(bool value)
        {
            _currentValue = value;
            onValueChanged.Invoke(value);
            return true;
        }

        public override bool TryGetBoolValue(out bool value)
        {
            value = _currentValue;
            return true;
        }
    }
}