using System;
using System.Collections.Generic;

namespace LiveAppUI.Model
{
    /// <summary>
    /// 삭제 비즈니스 로직과 ui간의 model
    /// </summary>
    public interface IResourceRemoveModel
    {
        IObservable<IReadOnlyList<(ResourceType type, string id, string displayName)>> OnSceneResourceListChanged { get; }

        void FilterRemoveResource(ResourceType resourceType);

        void RemoveResourcesFromScene(IReadOnlyList<(ResourceType type, string id)> targets);

    }
}
