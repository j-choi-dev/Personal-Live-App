using UnityEngine;

namespace StudioResourceSDK.Domain
{
    [DisallowMultipleComponent]
    public class EventCollection : MonoBehaviour
    {
        [SerializeField] private int collectionID;
        [SerializeField] private string displayName;
        [SerializeField] private Transform transformPivot;
        [SerializeField] private EventCollectionList eventCollectionList;

        public int CollectionID => collectionID;
        public string DisplayName => displayName;
        public Transform TransformPivot => transformPivot;
        public EventCollectionList EventList => eventCollectionList;
        public bool IsActive => transformPivot != null ? transformPivot.gameObject.activeSelf : gameObject.activeSelf;

        public virtual void SetActive(bool value)
        {
            if (transformPivot != null)
            {
                transformPivot.gameObject.SetActive(value);
                return;
            }

            gameObject.SetActive(value);
        }

        public void RefreshEvents()
        {
            if (eventCollectionList == null)
            {
                Debug.LogError($"EventCollectionList가 없습니다. CollectionID={collectionID}, Object={name}", this);
                return;
            }

            eventCollectionList.Refresh();
        }

#if UNITY_EDITOR
        public void SetEditorID(int value)
        {
            collectionID = value;
        }

        public void SetEditorIdentity(int id, string name)
        {
            collectionID = id;
            displayName = name;
        }

        public void SetEditorTransformPivot(Transform pivot)
        {
            transformPivot = pivot;
        }

        public void SetEditorList(EventCollectionList list)
        {
            eventCollectionList = list;
        }
#endif
    }
}