using StudioNetworkSDK.Application;
using UnityEngine;
using Zenject;

namespace LiveAppCore
{
    public class NetworkController : MonoBehaviour
    {
        private INetworkSendContext _networkApplication;

        [Inject]
        public void Initialize( INetworkSendContext networkApplication )
        {
            _networkApplication = networkApplication;
        }

        private async void Awake()
        {
            await _networkApplication.Initialize();
        }
    }
}
