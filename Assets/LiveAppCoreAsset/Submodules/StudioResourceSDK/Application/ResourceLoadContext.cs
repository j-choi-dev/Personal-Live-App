using Cysharp.Threading.Tasks;
using LiveAppCore;
using LiveAppUI;
using StudioCharacterSDK.Domain;
using StudioCommonSDK.Domain;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;
using Zenject;

namespace StudioResourceSDK.Application
{
    public class ResourceLoadContext : IResourceLoadContext
    {
        private IResourceDownloadDomain _resourceLoadDomain;
        private ISpawnPivotTransform _objectPivot;
        private ISpawnPivotTransform _backGroundPivot;
        private ISceneResourceListDomain _sceneResourceListDomain;

        private Subject<IReadOnlyList<ICharacter>> _onCharacterListChanged = new Subject<IReadOnlyList<ICharacter>>();
        public IObservable<IReadOnlyList<ICharacter>> OnCharacterListChanged => _onCharacterListChanged;

        private Subject<ICharacter> _onLoadCharacter = new Subject<ICharacter>();
        public IObservable<ICharacter> OnLoadCharacter => _onLoadCharacter;

        public ResourceLoadContext( IResourceDownloadDomain resourceLoadDomain,
            [Inject(Id = SpawnPivotId.Object)] ISpawnPivotTransform objectPivot,
            [Inject( Id = SpawnPivotId.Sprite )] ISpawnPivotTransform spritePivot,
            ISceneResourceListDomain sceneResourceListDomain)
        {
            _resourceLoadDomain = resourceLoadDomain;
            _objectPivot = objectPivot;
            _backGroundPivot = spritePivot;
            _sceneResourceListDomain = sceneResourceListDomain;
        }

        public async UniTask<bool> LoadResource( Domain.ResourceType resourceType, 
            ServerType serverType, 
            IReadOnlyList<string> resourceIds )
        {
            Debug.Log( $"LoadResource :: {resourceType}, {serverType}, {resourceIds[0]}" );
            var isAllSucceeded = true;
            for( var i = 0; i < resourceIds.Count; i++ )
            {
                var resourceId = resourceIds[i];
                var targetId = $"{resourceType.ToString().ToLower()}/{resourceId.ToLower()}.ab";
                var data = await _resourceLoadDomain.DownloadProcess( targetId );
                if( data == null )
                {
                    Debug.LogError( $"AssetBundle Download Process Failed ... ResourceId={resourceId}" );
                    isAllSucceeded = false;
                    continue;
                }
                GameObject prefab = data as GameObject;
                await UniTask.SwitchToMainThread();

                switch( resourceType )
                {
                    case Domain.ResourceType.Character:
                        GameObject characterRawObj = UnityEngine.Object.Instantiate( prefab, Vector3.zero, Quaternion.identity, _objectPivot.Transform );
                        characterRawObj.transform.localScale = Vector3.one;
                        var character = characterRawObj.GetComponent<ICharacter>();
                        character.SetID( resourceId );
                        _sceneResourceListDomain.AddCharacter( character );
                        break;
                    case Domain.ResourceType.BackGround:
                        GameObject bgRawObj = UnityEngine.Object.Instantiate( prefab, Vector3.zero, Quaternion.identity, _backGroundPivot.Transform );
                        bgRawObj.transform.localScale = Vector3.one;
                        var bg = bgRawObj.GetComponent<IBackground>();
                        bg.SetID( resourceId );
                        _sceneResourceListDomain.AddBackGround( bg );
                        break;
                }
            }
            await UniTask.NextFrame();
            return isAllSucceeded;
        }
    }
}
