using UnityEngine;


namespace StudioResourceSDK.Domain
{
    [DisallowMultipleComponent]
    public class TransformEmitter : MonoBehaviour
    {
        [SerializeField] private Transform target;

        public Transform Target => target != null ? target : transform;

        public void SetLocalPositionX(float value)
        {
            Transform targetTransform = Target;
            Vector3 position = targetTransform.localPosition;
            position.x = value;
            targetTransform.localPosition = position;
        }

        public void SetLocalPositionY(float value)
        {
            Transform targetTransform = Target;
            Vector3 position = targetTransform.localPosition;
            position.y = value;
            targetTransform.localPosition = position;
        }

        public void SetLocalPositionZ(float value)
        {
            Transform targetTransform = Target;
            Vector3 position = targetTransform.localPosition;
            position.z = value;
            targetTransform.localPosition = position;
        }

        public void SetLocalRotationX(float value)
        {
            Transform targetTransform = Target;
            Vector3 rotation = targetTransform.localEulerAngles;
            rotation.x = value;
            targetTransform.localEulerAngles = rotation;
        }

        public void SetLocalRotationY(float value)
        {
            Transform targetTransform = Target;
            Vector3 rotation = targetTransform.localEulerAngles;
            rotation.y = value;
            targetTransform.localEulerAngles = rotation;
        }

        public void SetLocalRotationZ(float value)
        {
            Transform targetTransform = Target;
            Vector3 rotation = targetTransform.localEulerAngles;
            rotation.z = value;
            targetTransform.localEulerAngles = rotation;
        }

        public void SetLocalScaleX(float value)
        {
            Transform targetTransform = Target;
            Vector3 scale = targetTransform.localScale;
            scale.x = value;
            targetTransform.localScale = scale;
        }

        public void SetLocalScaleY(float value)
        {
            Transform targetTransform = Target;
            Vector3 scale = targetTransform.localScale;
            scale.y = value;
            targetTransform.localScale = scale;
        }

        public void SetLocalScaleZ(float value)
        {
            Transform targetTransform = Target;
            Vector3 scale = targetTransform.localScale;
            scale.z = value;
            targetTransform.localScale = scale;
        }

#if UNITY_EDITOR
        public void SetEditorTarget(Transform targetTransform)
        {
            target = targetTransform;
        }
#endif
    }
}