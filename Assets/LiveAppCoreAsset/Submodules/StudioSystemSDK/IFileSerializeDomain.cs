using UnityEngine;

namespace StudioSystemSDK.Domain
{
    public interface IFileSerializeDomain
    {
        string SerializeToBinary( string rawMessage );
        string DeserializeToString( string rawMessage );
    }
}
