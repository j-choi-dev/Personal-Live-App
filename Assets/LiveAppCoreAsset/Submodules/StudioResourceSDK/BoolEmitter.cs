using UnityEngine;

namespace StudioResourceSDK.Domain
{
    [DisallowMultipleComponent]
    public class BoolEmitter : MonoBehaviour
    {
        [SerializeField] private GameObject targetObject;

        public GameObject TargetObject => targetObject;

        public void SetActive(bool value)
        {
            if (targetObject == null)
            {
                Debug.LogWarning($"BoolEmitter TargetObject가 없습니다. Object={name}", this);
                return;
            }

            targetObject.SetActive(value);
        }

#if UNITY_EDITOR
        public void SetEditorTarget(GameObject target)
        {
            targetObject = target;
        }
#endif
    }
}