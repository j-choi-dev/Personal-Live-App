using StudioCharacterSDK.Domain;
using System.Collections.Generic;
using System.Linq;
using UniRx;

namespace StudioResourceSDK.Domain
{
    public class SceneResourceList : ISceneResourceListDomain
    {
        private List<ICharacter> _characterList = new List<ICharacter>();
        public IReadOnlyList<ICharacter> CharacterList => _characterList;


        private Subject<IReadOnlyList<ICharacter>> _onChangedCharacterList = new Subject<IReadOnlyList<ICharacter>>();
        public System.IObservable<IReadOnlyList<ICharacter>> OnChangedCharacterList => _onChangedCharacterList;

        public ICharacter CurrentSelectedCharacter { get; private set; }

        private Subject<ICharacter> _onCurrentCharacterChanged = new Subject<ICharacter>();
        public System.IObservable<ICharacter> OnCurrentCharacterChanged => _onCurrentCharacterChanged;

        public SceneResourceList()
        {
            UnityEngine.Debug.Log( "SceneResourceList.ctor" );
        }

        public void AddCharacter( ICharacter character )
        {
            _characterList.Add( character );
            _onChangedCharacterList.OnNext( _characterList );
            UnityEngine.Debug.Log( _characterList.Count );
            SetCurrentSelectedCharacter( character.ID );
        }

        public bool IsExist( ResourceType resourceType, string id )
        {
            return _characterList.Exists( arg => arg.ID.Equals( id ) );
        }

        public void RemoveCharacter( string id )
        {
            var target = _characterList.FirstOrDefault( arg => arg.ID.Equals( id ) );
            _characterList.Remove( target );
            _onChangedCharacterList.OnNext( _characterList );
        }

        public void SetCurrentSelectedCharacter( string id )
        {
            var target = _characterList.FirstOrDefault( arg => arg.ID.Equals( id ) );
            if( target == null )
            {
                ResetCurrentSelectedCharacter();
                return;
            }
            CurrentSelectedCharacter = target;
            _onCurrentCharacterChanged.OnNext( CurrentSelectedCharacter );
            UnityEngine.Debug.Log( $"CurrentSelectedCharacter = {CurrentSelectedCharacter}" );
        }

        public void ResetCurrentSelectedCharacter()
        {
            CurrentSelectedCharacter = null;
            _onCurrentCharacterChanged.OnNext( CurrentSelectedCharacter );
            UnityEngine.Debug.Log( $"CurrentSelectedCharacter = NULL" );
        }
    }
}
