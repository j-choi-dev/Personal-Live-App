using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace LiveAppUI.Model
{
    public interface IResourceListModel
    {
        IObservable <IReadOnlyList<(string id, string displayName)>> OnCharacterListChanged { get; }
        ServerType GetCurrentServerType( ResourceType resourceType );

        UniTask InitializeServerConfig();
        UniTask GetResourceList( ResourceType resourceType, ServerType serverType );
        void SetCurrentServerType( ResourceType resourceType, ServerType serverType );
    }
}
