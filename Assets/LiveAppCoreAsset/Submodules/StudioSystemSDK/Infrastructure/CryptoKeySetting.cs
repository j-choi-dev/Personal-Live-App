using StudioSystemSDK.Domain;
using UnityEngine;

namespace StudioSystemSDK.Infrastructure
{
    [CreateAssetMenu( fileName = "CryptoKeySetting", menuName = "LiveAppCore/Crypto/Crypto Key Settings" )]
    public class CryptoKeySetting : ScriptableObject, ICryptoKeySettingDomain
    {
        [SerializeField] private string _cyryptoKey;
        public string CryptoKey => _cyryptoKey;
    }
}
