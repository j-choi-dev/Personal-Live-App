using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace LiveAppUI.Model
{
    public interface IResourceListModel
    {
        IObservable <IReadOnlyList<string>> OnCharacterListChanged { get; }

        UniTask InitializeServerConfig();
        UniTask GetResourceList( ResourceType resourceType, ServerType serverType );
        ServerType GetCurrentServerType( ResourceType resourceType );
    }
}
