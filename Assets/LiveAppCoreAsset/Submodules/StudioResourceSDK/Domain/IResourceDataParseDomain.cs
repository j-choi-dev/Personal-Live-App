using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioSystemSDK.Domain
{
    /// <summary>
    /// 리소스 데이터 파싱 관련 Interface
    /// </summary>
    public interface IResourceDataParseDomain
    {
        /// <summary>
        /// Character List Changed
        /// </summary>
        IObservable<IReadOnlyCollection<CharacterResourceItem>> OnCharacterListChanged { get; }
        /// <summary>
        /// Stage List Changed
        /// </summary>
        IObservable<IReadOnlyCollection<StageResourceItem>> OnStageListChanged { get; }
        /// <summary>
        /// Prop List Changed
        /// </summary>
        IObservable<IReadOnlyCollection<PropResourceItem>> OnPropListChanged { get; }

        /// <summary>
        /// Character List 파싱
        /// </summary>
        IReadOnlyCollection<CharacterResourceItem> ParseCharacterData( string rawData );
        /// <summary>
        /// Stage List 파싱
        /// </summary>
        IReadOnlyCollection<StageResourceItem> ParseStageData( string rawData );
        /// <summary>
        /// Prop List 파싱
        /// </summary>
        IReadOnlyCollection<PropResourceItem> ParsePropData( string rawData );
    }
}
