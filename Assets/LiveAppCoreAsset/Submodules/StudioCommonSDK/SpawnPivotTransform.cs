using StudioCommonSDK.Domain;
using UnityEngine;

namespace StudioCommonSDK.Infrastructure
{
    public class SpawnPivotTransform : MonoBehaviour, ISpawnPivotTransform
    {
        [SerializeField] private Transform _pivotTransform;
        public Transform Transform => _pivotTransform;
    }
}
