using Cysharp.Threading.Tasks;
using LiveAppUI;
using StudioCharacterSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    public interface IResourceLoadContext
    {
        IObservable<IReadOnlyList<ICharacter>> OnCharacterListChanged { get; }
        IObservable<ICharacter> OnLoadCharacter { get; }
        UniTask<bool> LoadResource( Domain.ResourceType resourceType,
            ServerType serverType,
            IReadOnlyList<string> resourceId );
    }
}
