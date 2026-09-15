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
        private List<IBackground> _backGroundList = new List<IBackground>();
        public IReadOnlyList<IBackground> BackGroundList => _backGroundList;


        private Subject<IReadOnlyList<ICharacter>> _onChangedCharacterList = new Subject<IReadOnlyList<ICharacter>>();
        public System.IObservable<IReadOnlyList<ICharacter>> OnChangedCharacterList => _onChangedCharacterList;

        private Subject<IReadOnlyList<IBackground>> _onChangedBackGroundList = new Subject<IReadOnlyList<IBackground>>();
        public System.IObservable<IReadOnlyList<IBackground>> OnChangedBackGroundList => _onChangedBackGroundList;


        public ICharacter CurrentSelectedCharacter { get; private set; }
        public IBackground CurrentSelectedBackGround { get; private set; }

        private Subject<ICharacter> _onCurrentCharacterChanged = new Subject<ICharacter>();
        public System.IObservable<ICharacter> OnCurrentCharacterChanged => _onCurrentCharacterChanged;

        private Subject<IBackground> _onCurrentBackGroundChanged = new Subject<IBackground>();
        public System.IObservable<IBackground> OnCurrentBackGroundChanged => _onCurrentBackGroundChanged;

        public void AddCharacter( ICharacter character )
        {
            _characterList.Add( character );
            _onChangedCharacterList.OnNext( _characterList );
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

        public void ResetCurrentSelectedCharacter()
        {
            CurrentSelectedCharacter = null;
            _onCurrentCharacterChanged.OnNext( CurrentSelectedCharacter );
            UnityEngine.Debug.Log( $"CurrentSelectedCharacter = NULL" );
        }

        public void ResetCurrentSelectedBackGround()
        {
            CurrentSelectedBackGround = null;
            _onCurrentBackGroundChanged.OnNext( CurrentSelectedBackGround );
            UnityEngine.Debug.Log( $"CurrentSelectedBackGround = NULL" );
        }

        public void AddBackGround( IBackground background )
        {
            _backGroundList.Add( background );
            _onChangedBackGroundList.OnNext( _backGroundList );
            SetCurrentSelectedBackGround( background.ID );
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
        }

        public void SetCurrentSelectedBackGround( string id )
        {
            var target = _backGroundList.FirstOrDefault( arg => arg.ID.Equals( id ) );
            if( target == null )
            {
                ResetCurrentSelectedBackGround();
                return;
            }
            CurrentSelectedBackGround = target;
            _onCurrentBackGroundChanged.OnNext( CurrentSelectedBackGround );
        }
    }
}
