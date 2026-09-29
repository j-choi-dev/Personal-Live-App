using System;
using System.Collections.Generic;

namespace StudioResourceSDK.Domain
{
    public interface ISceneGimmickListDomain
    {
        IReadOnlyList<SceneGimmickInfo> GimmickList { get; }

        IObservable<IReadOnlyList<SceneGimmickInfo>> OnChangedGimmickList { get; }

        bool Add(string resourceID, EventCollection eventCollection);
        bool Remove(string resourceID);

        bool IsExist(string resourceID);

        bool TryGet(string resourceID, out SceneGimmickInfo gimmick);
        bool TryGetEventCollection(string resourceID, out EventCollection eventCollection);
        bool TryGetEventCollection(string resourceID, int collectionID, out EventCollection eventCollection);
    }
}