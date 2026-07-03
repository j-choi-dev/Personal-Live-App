using Cysharp.Threading.Tasks;
using StudioResourceSDK.Domain;
using System;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public class ResourceTableContext : IResourceTableContext
    {
        public IObservable<ResourceTableData> OnCharacterListChanged => throw new NotImplementedException();

        public IObservable<ResourceTableData> OnStageListChanged => throw new NotImplementedException();

        public IObservable<ResourceTableData> OnPropListChanged => throw new NotImplementedException();

        public UniTask<bool> InitProcess()
        {
            throw new NotImplementedException();
        }

        public UniTask<bool> LoadResourceTableProcess( ResourceType tyoe )
        {
            throw new NotImplementedException();
        }
    }
}
