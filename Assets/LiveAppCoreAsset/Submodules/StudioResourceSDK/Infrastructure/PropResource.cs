using StudioResourceSDK.Domain;
using UnityEngine;

namespace StudioResourceSDK.Infrastructure
{
    public class PropResource : MonoBehaviour, IProp
    {

        public string ID { get; private set; }

        public void SetID(string id)
            => ID = id;


        public void Init()
        {
            Debug.Log($"Prop :: {ID} Init");
        }
    }
}
