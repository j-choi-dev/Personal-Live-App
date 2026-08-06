using UnityEngine;

namespace StudioRendererSDK.Domain
{
    // @Use
    public interface IObsVideoSource
    {
        Texture OutputTexture { get; }

        bool IsReady { get; }
    }
}