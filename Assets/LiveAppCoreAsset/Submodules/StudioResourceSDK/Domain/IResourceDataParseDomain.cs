using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioSystemSDK.Domain
{
    public interface IResourceDataParseDomain
    {
        IObservable<IReadOnlyCollection<CharacterResourceItem>> OnCharacterListChanged { get; }
        IObservable<IReadOnlyCollection<StageResourceItem>> OnStageListChanged { get; }
        IObservable<IReadOnlyCollection<PropResourceItem>> OnPropListChanged { get; }

        IReadOnlyCollection<CharacterResourceItem> ParseCharacterData( string rawData );
        IReadOnlyCollection<StageResourceItem> ParseStageData( string rawData );
        IReadOnlyCollection<PropResourceItem> ParsePropData( string rawData );
    }
}
