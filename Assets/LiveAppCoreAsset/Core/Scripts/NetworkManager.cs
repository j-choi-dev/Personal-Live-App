using StudioNetworkSDK.Application;
using UnityEngine;
using Zenject;

namespace LiveAppCore
{
    public class NetworkManager : MonoBehaviour
    {
        private INetworkRequestApplication _networkApplication;

        [Inject]
        public void Initialize( INetworkRequestApplication networkApplication )
        {
            _networkApplication = networkApplication;
        }

        private async void Awake()
        {
            await _networkApplication.Initialize();
        }
    }
}
