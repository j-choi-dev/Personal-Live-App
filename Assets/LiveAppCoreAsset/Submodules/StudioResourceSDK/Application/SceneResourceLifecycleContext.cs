using StudioResourceSDK.Domain;
using System;
using UniRx;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public class SceneResourceLifecycleContext : ISceneResourceLifecycleContext, IDisposable
    {
        private ISceneResourceListDomain _sceneResourceList;
        private ISceneGimmickListDomain _sceneGimmickList;

        private readonly Subject<string> _onGimmickRegistered = new Subject<string>();
        public IObservable<string> OnGimmickRegistered => _onGimmickRegistered;

        private readonly Subject<string> _onGimmickRemoved = new Subject<string>();
        public IObservable<string> OnGimmickRemoved => _onGimmickRemoved;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SceneResourceLifecycleContext(
            ISceneResourceListDomain sceneResourceList,
            ISceneGimmickListDomain sceneGimmickList)
        {
            _sceneResourceList = sceneResourceList;
            _sceneGimmickList = sceneGimmickList;

            _sceneResourceList.OnCharacterRemoved
                .Subscribe(c => DestroyResource(c.ID, c as Component))
                .AddTo(_disposables);
            _sceneResourceList.OnBackGroundRemoved
                .Subscribe(c => DestroyResource(c.ID, c as Component))
                .AddTo(_disposables);
            _sceneResourceList.OnPropRemoved
                .Subscribe(c => DestroyResource(c.ID, c as Component))
                .AddTo(_disposables);
        }

        public bool RegisterGimmick(string resourceID, GameObject resourceObject)
        {
            if (resourceObject == null)
            {
                return false;
            }

            EventCollection eventCollection = resourceObject.GetComponent<EventCollection>();

            if (eventCollection == null)
            {
                return false;
            }

            bool result = _sceneGimmickList.Add(resourceID, eventCollection);

            if (result)
            {
                _onGimmickRegistered.OnNext(resourceID);
            }

            return result;
        }

        public void RemoveGimmick(string resourceID)
        {
            bool result = _sceneGimmickList.Remove(resourceID);

            if (result)
            {
                _onGimmickRemoved.OnNext(resourceID);
            }
        }

        private void DestroyResource(string id, Component component)
        {
            //RemoveGimmick(id);

            if (component == null)
            {
                Debug.LogWarning($"[SceneResourceLifecycleContext] 파괴할 오브젝트를 찾지 못했습니다.");
                return;
            }

            //TODO destroy를 여기서 하는 게 맞는 지 더 고민해볼것@OH 26.10.15
            //gimmick도 ISceneGimmickListDomain에서 결과 받고 _onGimmickRemoved로 이벤트를 날리는데
            //삭제하는 destroyer같은 model같은 걸 만들고 거기서 해야할 지 -> 구조 설계에 대해서 질문해볼것
            //우선 프로토타입 수준으로 여기서 삭제해서 remove 이벤트 연동 및 작동 방식 체크
            UnityEngine.Object.Destroy(component.gameObject);
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _onGimmickRegistered.Dispose();
            _onGimmickRemoved.Dispose();
        }
    }
}