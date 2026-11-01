using Cysharp.Threading.Tasks;
using LiveAppUI;
using StudioCharacterSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    public interface IResourceLoadContext
    {
        UniTask<bool> LoadResource( Domain.ResourceType resourceType,
            ServerType serverType,
            IReadOnlyList<string> resourceIds );

        bool RemoveResource(Domain.ResourceType type, string id);
    }
}
