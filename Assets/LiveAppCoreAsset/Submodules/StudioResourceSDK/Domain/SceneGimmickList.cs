using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace StudioResourceSDK.Domain
{
    public class SceneGimmickList : ISceneGimmickListDomain
    {
        private readonly List<SceneGimmickInfo> _gimmickList = new List<SceneGimmickInfo>();
        private readonly Dictionary<string, SceneGimmickInfo> _gimmickTable = new Dictionary<string, SceneGimmickInfo>();

        public IReadOnlyList<SceneGimmickInfo> GimmickList => _gimmickList;

        private readonly Subject<IReadOnlyList<SceneGimmickInfo>> _onChangedGimmickList = new Subject<IReadOnlyList<SceneGimmickInfo>>();
        public System.IObservable<IReadOnlyList<SceneGimmickInfo>> OnChangedGimmickList => _onChangedGimmickList;

        public bool Add(string resourceID, EventCollection eventCollection)
        {
            if (string.IsNullOrEmpty(resourceID))
            {
                Debug.LogError("SceneGimmickList Add 실패 :: ResourceID가 없습니다.");
                return false;
            }

            if (eventCollection == null)
            {
                Debug.LogError($"SceneGimmickList Add 실패 :: EventCollection이 없습니다. ResourceID={resourceID}");
                return false;
            }

            if (_gimmickTable.ContainsKey(resourceID))
            {
                Debug.LogWarning($"SceneGimmickList에 이미 등록되어 있습니다. ResourceID={resourceID}");
                return false;
            }

            eventCollection.RefreshEvents();

            SceneGimmickInfo gimmick = new SceneGimmickInfo(resourceID, eventCollection);

            _gimmickList.Add(gimmick);
            _gimmickTable.Add(resourceID, gimmick);

            _onChangedGimmickList.OnNext(_gimmickList);

            return true;
        }

        public bool Remove(string resourceID)
        {
            if (_gimmickTable.TryGetValue(resourceID, out SceneGimmickInfo gimmick) == false)
            {
                return false;
            }

            _gimmickTable.Remove(resourceID);
            _gimmickList.Remove(gimmick);

            _onChangedGimmickList.OnNext(_gimmickList);

            return true;
        }

        public bool IsExist(string resourceID)
        {
            return _gimmickTable.ContainsKey(resourceID);
        }

        public bool TryGet(string resourceID, out SceneGimmickInfo gimmick)
        {
            return _gimmickTable.TryGetValue(resourceID, out gimmick);
        }

        public bool TryGetEventCollection(string resourceID, out EventCollection eventCollection)
        {
            eventCollection = null;

            if (_gimmickTable.TryGetValue(resourceID, out SceneGimmickInfo gimmick) == false)
            {
                return false;
            }

            if (gimmick.EventCollection == null)
            {
                Remove(resourceID);
                return false;
            }

            eventCollection = gimmick.EventCollection;

            return true;
        }

        public bool TryGetEventCollection(string resourceID, int collectionID, out EventCollection eventCollection)
        {
            eventCollection = null;

            if (TryGetEventCollection(resourceID, out EventCollection collection) == false)
            {
                return false;
            }

            if (collection.CollectionID != collectionID)
            {
                Debug.LogWarning($"CollectionID가 일치하지 않습니다. ResourceID={resourceID}, Request={collectionID}, Current={collection.CollectionID}");
                return false;
            }

            eventCollection = collection;

            return true;
        }

        public void Clear()
        {
            _gimmickList.Clear();
            _gimmickTable.Clear();

            _onChangedGimmickList.OnNext(_gimmickList);
        }
    }
}