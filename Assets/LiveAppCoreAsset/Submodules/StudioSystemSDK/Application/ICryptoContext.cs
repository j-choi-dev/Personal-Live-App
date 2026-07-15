using Cysharp.Threading.Tasks;

namespace StudioSystemSDK.Application
{
    public interface ICryptoContext
    {
        UniTask<string> ConvertToEncryptedData( string rawData, string key );
        UniTask<string> ConvertToDecryptedData( string rawData, string key );
    }
}
