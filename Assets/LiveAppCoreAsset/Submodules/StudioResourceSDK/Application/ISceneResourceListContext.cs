using StudioCharacterSDK.Domain;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    public interface ISceneResourceListContext
    {
        IReadOnlyList<ICharacter> CharacterList { get; }
        IReadOnlyList<IBackground> BackGroundList { get; }
        IReadOnlyList<IProp> PropList { get; }
        IObservable<IReadOnlyList<ICharacter>> OnChangedCharacterList { get; }
        IObservable<IReadOnlyList<IBackground>> OnChangedBackGroundList { get; }
        IObservable<IReadOnlyList<IProp>> OnChangedPropList { get; }
        IObservable<ICharacter> OnCurrentCharacterChanged { get; }

        ICharacter CurrentSelectedCharacter { get; }

        void AddCharacter( ICharacter character );
        void RemoveResource( ResourceType resourceType, string id );
        bool IsExist( ResourceType resourceType, string id );
        void SetCurrentSelectedResource( ResourceType resourceType, string id );
        void ResetCurrentSelectedResource( ResourceType resourceType );
    }
}
