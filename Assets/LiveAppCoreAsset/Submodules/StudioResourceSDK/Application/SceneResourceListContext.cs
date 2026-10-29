using StudioCharacterSDK.Domain;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using UniRx;

namespace StudioResourceSDK.Application
{
    public class SceneResourceListContext : ISceneResourceListContext, IDisposable
    {
        private ISceneResourceListDomain _listDomain;

        public IReadOnlyList<ICharacter> CharacterList 
            => _listDomain.CharacterList;

        public IReadOnlyList<IBackground> BackGroundList
            => _listDomain.BackGroundList;
        public IReadOnlyList<IProp> PropList
            => _listDomain.PropList;

        public IObservable<IReadOnlyList<ICharacter>> OnChangedCharacterList 
            => _listDomain.OnChangedCharacterList;

        public IObservable<IReadOnlyList<IBackground>> OnChangedBackGroundList
            => _listDomain.OnChangedBackGroundList;
        public IObservable<IReadOnlyList<IProp>> OnChangedPropList
            => _listDomain.OnChangedPropList;

        public IObservable<ICharacter> OnCurrentCharacterChanged => _listDomain.OnCurrentCharacterChanged;

        public ICharacter CurrentSelectedCharacter => _listDomain.CurrentSelectedCharacter;

        private CompositeDisposable _disposables = new CompositeDisposable();

        public SceneResourceListContext( ISceneResourceListDomain listDomain )
        {
            _listDomain = listDomain;
        }

        public void AddCharacter( ICharacter character )
        {
            _listDomain.AddCharacter( character );
        }

        public bool IsExist( ResourceType resourceType, string id )
        {
            return _listDomain.IsExist( resourceType, id );
        }

        public void RemoveResource( ResourceType resourceType, string id )
        {
            switch(resourceType)
            {
                case ResourceType.Character:
                    _listDomain.RemoveCharacter( id );
                    break;
                case ResourceType.BackGround:
                    _listDomain.RemoveBackGround(id);
                    break;
                case ResourceType.Prop:
                    _listDomain.RemoveProp(id);
                    break;
            }
        }

        public void ResetCurrentSelectedResource( ResourceType resourceType )
        {
            switch( resourceType )
            {
                case ResourceType.Character:
                    _listDomain.ResetCurrentSelectedCharacter();
                    break;
            }
        }

        public void SetCurrentSelectedResource( ResourceType resourceType, string id )
        {
            switch( resourceType )
            {
                case ResourceType.Character:
                    _listDomain.SetCurrentSelectedCharacter( id );
                    break;
            }
        }

        public void Dispose()
        {
            _disposables.Dispose();
            _disposables = null;
        }
    }
}
