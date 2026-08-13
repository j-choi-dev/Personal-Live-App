using StudioCharacterSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioResourceSDK.Domain
{
    public interface ISceneResourceListDomain
    {
        IReadOnlyList<ICharacter> CharacterList { get; }
        IObservable<IReadOnlyList<ICharacter>> OnChangedCharacterList { get; }
        IObservable<ICharacter> OnCurrentCharacterChanged { get; }

        ICharacter CurrentSelectedCharacter { get; }

        void AddCharacter(ICharacter character);
        void RemoveCharacter( string id );
        bool IsExist( ResourceType resourceType, string id );
        void SetCurrentSelectedCharacter( string id );
        void ResetCurrentSelectedCharacter();
    }
}
