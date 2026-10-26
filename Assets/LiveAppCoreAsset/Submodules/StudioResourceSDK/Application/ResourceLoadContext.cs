using Cysharp.Threading.Tasks;
using LiveAppCore;
using LiveAppUI;
using StudioCharacterSDK.Domain;
using StudioCommonSDK.Domain;
using StudioResourceSDK.Domain;
using System;
using System.Linq;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace StudioResourceSDK.Application
{
    public class ResourceLoadContext : IResourceLoadContext
    {
        private IResourceDownloadDomain _resourceLoadDomain;
        private IResourceDestroyDomain _destroyDomain;
        private ISpawnPivotTransform _objectPivot;
        private ISpawnPivotTransform _backGroundPivot;
        private ISceneResourceListDomain _sceneResourceListDomain;
        private ISceneResourceLifecycleContext _sceneResourceLifecycleContext;

        private Subject<IReadOnlyList<ICharacter>> _onCharacterListChanged = new Subject<IReadOnlyList<ICharacter>>();
        public IObservable<IReadOnlyList<ICharacter>> OnCharacterListChanged => _onCharacterListChanged;
       
        private Subject<ICharacter> _onLoadCharacter = new Subject<ICharacter>();
        public IObservable<ICharacter> OnLoadCharacter => _onLoadCharacter;

        public ResourceLoadContext(
            IResourceDownloadDomain resourceLoadDomain,
            IResourceDestroyDomain destoryDomain,
            [Inject(Id = SpawnPivotId.Object)] ISpawnPivotTransform objectPivot,
            [Inject(Id = SpawnPivotId.Sprite)] ISpawnPivotTransform spritePivot,
            ISceneResourceListDomain sceneResourceListDomain,
            ISceneResourceLifecycleContext sceneResourceLifecycleContext)
        {
            _resourceLoadDomain = resourceLoadDomain;
            _destroyDomain = destoryDomain;
            _objectPivot = objectPivot;
            _backGroundPivot = spritePivot;
            _sceneResourceListDomain = sceneResourceListDomain;
            _sceneResourceLifecycleContext = sceneResourceLifecycleContext;
        }

        public async UniTask<bool> LoadResource(Domain.ResourceType resourceType,
            ServerType serverType,
            IReadOnlyList<string> resourceIds)
        {
            Debug.Log($"LoadResource :: {resourceType}, {serverType}, {resourceIds[0]}");
            var isAllSucceeded = true;
            for (var i = 0; i < resourceIds.Count; i++)
            {
                var resourceId = resourceIds[i];
                var targetId = $"{resourceType.ToString().ToLower()}/{resourceId.ToLower()}.ab";
                var data = await _resourceLoadDomain.DownloadProcess(targetId);
                if (data == null)
                {
                    Debug.LogError($"AssetBundle Download Process Failed ... ResourceId={resourceId}");
                    isAllSucceeded = false;
                    continue;
                }
                GameObject prefab = data as GameObject;
                await UniTask.SwitchToMainThread();

                switch (resourceType)
                {
                    case Domain.ResourceType.Character:
                        GameObject characterRawObj = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, _objectPivot.Transform);
                        characterRawObj.transform.localScale = Vector3.one;
                        var character = characterRawObj.GetComponent<ICharacter>();
                        character.SetID(resourceId);

                        _sceneResourceListDomain.AddCharacter(character);
                        if (characterRawObj.GetComponent<EventCollection>() != null)
                        {
                            _sceneResourceLifecycleContext.RegisterGimmick(resourceId, characterRawObj);
                        }
                        _sceneResourceLifecycleContext.RegisterGimmick(resourceId, characterRawObj);
                        break;
                    case Domain.ResourceType.BackGround:
                        GameObject bgRawObj = UnityEngine.Object.Instantiate(prefab, _backGroundPivot.Transform, false);
                        if (bgRawObj.GetComponent<EventCollection>() != null)
                        {
                            _sceneResourceLifecycleContext.RegisterGimmick(resourceId, bgRawObj);
                        }
                        RectTransform bgRect = bgRawObj.GetComponent<RectTransform>();
                        if (bgRect != null)
                        {
                            bgRect.anchoredPosition3D = Vector3.zero;
                            bgRect.localScale = Vector3.one;
                            bgRect.localRotation = Quaternion.identity;
                        }
                        var bg = bgRawObj.GetComponent<IBackground>();
                        bg.SetID(resourceId);
                        _sceneResourceListDomain.AddBackGround(bg);
                        _sceneResourceLifecycleContext.RegisterGimmick(resourceId, bgRawObj);
                        break;
                    case Domain.ResourceType.Prop:
                        GameObject propRawObj = UnityEngine.Object.Instantiate(prefab, Vector3.zero, Quaternion.identity, _objectPivot.Transform);
                        propRawObj.transform.localScale = Vector3.one;
                        var prop = propRawObj.GetComponent<IProp>();
                        prop.SetID(resourceId);

                        _sceneResourceListDomain.AddProp(prop);
                        if (propRawObj.GetComponent<EventCollection>() != null)
                        {
                            _sceneResourceLifecycleContext.RegisterGimmick(resourceId, propRawObj);
                        }
                        _sceneResourceLifecycleContext.RegisterGimmick(resourceId, propRawObj);
                        break;
                }
            }
            await UniTask.NextFrame();
            return isAllSucceeded;
        }

        public bool RemoveResource(Domain.ResourceType type, string id)
        {
            switch (type)
            {
                case Domain.ResourceType.Character:
                    return RemoveCharacter(id);
                case Domain.ResourceType.BackGround:
                    return RemoveBackGroud(id);
                case Domain.ResourceType.Prop:
                    return RemoveProp(id);
                default:
                    Debug.LogWarning($"[RemoveResourceContext] 삭제할 수 없는 리소스 타입입니다.");
                    return false;
            }
        }

        private static bool TryFindInPivot<T>(ISpawnPivotTransform pivot, string id, Func<T, string> getId,
          out T found) where T : class
        {
            found = null;

            if (pivot == null
            || pivot.Transform == null)
            {
                return false;
            }

            foreach (Transform child in pivot.Transform)
            {
                if (child.TryGetComponent(out T component)
                && getId(component) == id)
                {
                    found = component;
                    return true;
                }
            }

            return false;
        }
        private bool RemoveCharacter(string id)
        {
            bool found = TryFindInPivot<ICharacter>(_objectPivot, id, arg => arg.ID, out var target);
            if (found == false)
            {
                Debug.LogWarning($"[ResourceLoadContext] Spawn Pivot에서 캐릭터를 찾지 못해 목록만 정리합니다. ID=[{id}]");
            }

            try
            {
                _sceneResourceListDomain.RemoveCharacter(id);       // 목록에서 해제 (선택 해제 포함)
            }
            finally
            {
                _sceneResourceLifecycleContext.RemoveGimmick(id);  // 기믹 목록에서 해제
                if (found)
                {
                    _destroyDomain.Destroy(target);                 // 오브젝트 파괴
                }
            }
            return found;
        }

        private bool RemoveBackGroud(string id)
        {
            bool found = TryFindInPivot<IBackground>(_backGroundPivot, id, arg => arg.ID, out var target);
            if(found == false)
            {
                Debug.LogWarning($"[ResourceLoadContext] BackGround에서 배경를 찾지 못해 목록만 정리합니다. ID=[{id}]");
            }

            try
            {
                _sceneResourceListDomain.RemoveBackGround(id);
            }
            finally
            {
                _sceneResourceLifecycleContext.RemoveGimmick(id);
                if(found)
                {
                    _destroyDomain.Destroy(target);
                }
            }
            return found;
        }

        private bool RemoveProp(string id)
        {
            bool found = TryFindInPivot<IProp>(_objectPivot, id, arg => arg.ID, out var target);
            if(found == false)
            {
                Debug.LogWarning($"[ResourceLoadContext] Spawn Pivot에서 Prop을 찾지 못해 목록만 정리합니다. ID=[{id}]");
            }

            try
            {
                _sceneResourceListDomain.RemoveProp(id);
            }
            finally
            {
                _sceneResourceLifecycleContext.RemoveGimmick(id);
                if (found)
                {
                    _destroyDomain.Destroy(target);
                }
            }
            return found;
        }



    }
}
