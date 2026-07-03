using UnityEngine;

namespace StudioSystemSDK.Domain
{
    public interface IFileSerializeDomain
    {
        string SerializeToBinary( string rawMessage );
        T DeserializeFromJson<T>( string rawMessage );
    }
}
