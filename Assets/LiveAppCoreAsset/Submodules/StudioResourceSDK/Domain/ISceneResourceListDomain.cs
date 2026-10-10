using StudioCharacterSDK.Domain;
using System;
using System.Collections.Generic;
using StudioResourceSDK.Domain;

namespace StudioResourceSDK.Domain
{
    public interface ISceneResourceListDomain
    {
        IReadOnlyList<ICharacter> CharacterList { get; }
        IReadOnlyList<IBackground> BackGroundList { get; }
        IReadOnlyList<IProp> PropList { get; }
        IObservable<IReadOnlyList<ICharacter>> OnChangedCharacterList { get; }
        IObservable<ICharacter> OnCurrentCharacterChanged { get; }
        IObservable<IReadOnlyList<IBackground>> OnChangedBackGroundList { get; }
        IObservable<IBackground> OnCurrentBackGroundChanged { get; }
        IObservable<IReadOnlyList<IProp>> OnChangedPropList { get; }
        IObservable<IProp> OnCurrentPropChanged { get; }

        ICharacter CurrentSelectedCharacter { get; }
        IBackground CurrentSelectedBackGround { get; }
        IProp CurrentSelectedProp{ get; }

        void AddCharacter(ICharacter character);
        void AddBackGround( IBackground background );
        void AddProp(IProp prop);
        void RemoveCharacter( string id );
        bool IsExist( ResourceType resourceType, string id );
        void SetCurrentSelectedCharacter( string id );
        void ResetCurrentSelectedCharacter();
        void SetCurrentSelectedBackGround(string id);
        void ResetCurrentSelectedBackGround();
        void SetCurrentSelectedProp(string id);
        void ResetCurrentSelectedProp();
    }
}
