using System;
using System.Collections.Generic;
using System.Text;

namespace LiveAppUI.Model
{
    public interface IResourceRemoveModel
    {
        IObservable<IReadOnlyList<(ResourceType type, string id, string displayName)>> OnSceneResourceListChanged { get; }

        void FilterRemoveResource(ResourceType resourceType);

        void RemoveResourcesFromScene(IReadOnlyList<(ResourceType type, string id)> targets);

    }
}
