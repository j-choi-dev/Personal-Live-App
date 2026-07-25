using Cysharp.Threading.Tasks;
using LiveAppUI;
using StudioCharacterSDK.Domain;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace StudioResourceSDK.Application
{
    public class ResourceLoadContext : IResourceLoadContext
    {
        private IResourceDownloadDomain _resourceLoadDomain;

        private Subject<IReadOnlyList<ICharacter>> _onCharacterListChanged = new Subject<IReadOnlyList<ICharacter>>();
        public IObservable<IReadOnlyList<ICharacter>> OnCharacterListChanged => _onCharacterListChanged;

        private Subject<ICharacter> _onLoadCharacter = new Subject<ICharacter>();
        public IObservable<ICharacter> OnLoadCharacter => _onLoadCharacter;

        public ResourceLoadContext( IResourceDownloadDomain resourceLoadDomain)
        {
            _resourceLoadDomain = resourceLoadDomain; 
        }

        public UniTask<bool> LoadResource( Domain.ResourceType resourceType, 
            ServerType serverType, 
            IReadOnlyList<string> resourceId )
        {
            throw new NotImplementedException();
        }
    }
}
