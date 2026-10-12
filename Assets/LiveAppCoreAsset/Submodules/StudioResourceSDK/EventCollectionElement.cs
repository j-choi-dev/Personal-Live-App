using UnityEngine;

namespace StudioResourceSDK.Domain
{
    public enum EventDataType
    {
        None,
        Trigger,
        Bool,
        Int,
        Float,
        String
    }

    public abstract class EventCollectionElement : MonoBehaviour
    {
        [SerializeField] private int eventID;
        [SerializeField] private string eventDisplayName;

        public int EventID => eventID;
        public string EventDisplayName => eventDisplayName;

        public abstract EventDataType DataType { get; }

        public virtual bool Invoke()
        {
            return false;
        }

        public virtual bool Invoke(bool value)
        {
            return false;
        }

        public virtual bool Invoke(int value)
        {
            return false;
        }

        public virtual bool Invoke(float value)
        {
            return false;
        }

        public virtual bool Invoke(string value)
        {
            return false;
        }

        public virtual bool TryGetBoolValue(out bool value)
        {
            value = default;
            return false;
        }

        public virtual bool TryGetIntValue(out int value)
        {
            value = default;
            return false;
        }

        public virtual bool TryGetFloatValue(out float value)
        {
            value = default;
            return false;
        }

        public virtual bool TryGetStringValue(out string value)
        {
            value = default;
            return false;
        }

#if UNITY_EDITOR
        public void SetEditorIdentity(int id, string displayName)
        {
            eventID = id;
            eventDisplayName = displayName;
        }
#endif
    }
}