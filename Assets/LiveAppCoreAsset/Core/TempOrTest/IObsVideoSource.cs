using System;
using UnityEngine;

namespace StudioRendererSDK.Domain
{
    public interface IObsVideoSource
    {
        RenderTexture OutputTexture { get; }

        Vector2Int OutputResolution { get; }

        bool IsReady { get; }

        event Action<RenderTexture> OutputTextureChanged;
    }
}