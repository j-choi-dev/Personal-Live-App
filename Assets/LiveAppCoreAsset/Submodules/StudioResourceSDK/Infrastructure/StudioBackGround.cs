using StudioResourceSDK.Domain;
using UnityEngine;

namespace StudioResourceSDK.Infrastructure
{
    public class StudioBackGround : MonoBehaviour, IBackground
    {
        public string ID { get; private set; }

        public void SetID( string id )
            => ID = id;

        public void Init()
        {
            Debug.Log( $"{ID} :: NotImplementedException" );
        }

        // Start is called once before the first execution of Update after the MonoBehaviour is created
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {

        }
    }
}
