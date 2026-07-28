using Cysharp.Threading.Tasks;
using LiveAppUI;
using StudioCharacterSDK.Domain;
using StudioCommonSDK.Domain;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public class ResourceLoadContext : IResourceLoadContext
    {
        private IResourceDownloadDomain _resourceLoadDomain;
        private ISpawnPivotTransform _spawnPivot;
        private ISceneResourceListDomain _sceneResourceListDomain;

        private Subject<IReadOnlyList<ICharacter>> _onCharacterListChanged = new Subject<IReadOnlyList<ICharacter>>();
        public IObservable<IReadOnlyList<ICharacter>> OnCharacterListChanged => _onCharacterListChanged;

        private Subject<ICharacter> _onLoadCharacter = new Subject<ICharacter>();
        public IObservable<ICharacter> OnLoadCharacter => _onLoadCharacter;

        public ResourceLoadContext( IResourceDownloadDomain resourceLoadDomain,
            ISpawnPivotTransform spawnPivot,
            ISceneResourceListDomain sceneResourceListDomain)
        {
            _resourceLoadDomain = resourceLoadDomain;
            _spawnPivot = spawnPivot;
            _sceneResourceListDomain = sceneResourceListDomain;
        }

        public async UniTask<bool> LoadResource( Domain.ResourceType resourceType, 
            ServerType serverType, 
            IReadOnlyList<string> resourceIds )
        {
            var isAllSucceeded = true;
            for( var i = 0; i < resourceIds.Count; i++ )
            {
                var resourceId = resourceIds[i];
                var targetId = resourceId.ToLower() +".ab";
                var data = await _resourceLoadDomain.DownloadProcess( targetId );
                if( data == null )
                {
                    Debug.LogError( $"AssetBundle Download Process Failed ... ResourceId={resourceId}" );
                    isAllSucceeded = false;
                    continue;
                }
                GameObject prefab = data as GameObject;
                await UniTask.SwitchToMainThread();

                GameObject instance = UnityEngine.Object.Instantiate( prefab, Vector3.zero, Quaternion.identity, _spawnPivot.Transform );
                instance.transform.localScale = Vector3.one;
                switch( resourceType )
                {
                    case Domain.ResourceType.Character:
                        var character = instance.GetComponent<ICharacter>();
                        character.SetID( resourceId );
                        _sceneResourceListDomain.AddCharacter( character );
                        break;
                }
            }
            await UniTask.NextFrame();
            return isAllSucceeded;
        }
    }
}
