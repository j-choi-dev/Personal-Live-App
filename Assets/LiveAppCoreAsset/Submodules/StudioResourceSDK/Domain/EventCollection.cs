using UnityEngine;

namespace StudioResourceSDK.Domain
{
    [DisallowMultipleComponent]
    public class EventCollection : MonoBehaviour
    {
        [SerializeField] private int id;

        public int ID => id;
        public bool IsActive => gameObject.activeSelf;

        public virtual void SetActive(bool value)
        {
            gameObject.SetActive(value);
        }

#if UNITY_EDITOR
        public void SetEditorID(int value)
        {
            id = value;
        }
#endif
    }
}