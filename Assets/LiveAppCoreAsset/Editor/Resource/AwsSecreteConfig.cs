using UnityEngine;

namespace LiveAppCore.Editor.Infrastructure
{
    [CreateAssetMenu( fileName = "AwsSecreteConfig", menuName = "LiveAppCore/AWS/AwsSecretConfig" )]
    public class AwsSecreteConfig : ScriptableObject
    {
        [SerializeField]private string _accessKey;
        [SerializeField]private string _secreteAccessKey;
        public string AccessKey { get; }
        public string SecreteAccessKey { get; }
    }
}
