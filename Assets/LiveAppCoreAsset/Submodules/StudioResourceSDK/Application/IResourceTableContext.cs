using Cysharp.Threading.Tasks;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// 리소스 테이블 취득 등의 Application
    /// </summary>
    public interface IResourceTableContext
    {
        /// <summary>
        /// Character List Change Event
        /// </summary>
        IObservable<IReadOnlyCollection<CharacterResourceItem>> OnCharacterListChanged { get; }
        /// <summary>
        /// Stage List Change Event
        /// </summary>
        IObservable<IReadOnlyCollection<StageResourceItem>> OnStageListChanged { get; }
        /// <summary>
        /// Prop List Change Event
        /// </summary>
        IObservable<IReadOnlyCollection<PropResourceItem>> OnPropListChanged { get; }

        /// <summary>
        /// 초기화 프로세스
        /// </summary>
        /// <returns>초기화 성공/실패</returns>
        UniTask<bool> InitProcess();
        /// <summary>
        /// 리소스 리스트 취득 프로세스
        /// </summary>
        /// <param name="resourceType">리소스 타입</param>
        /// <param name="serverType">리소스 서버 타입</param>
        /// <param name="tableUrl">테이블 URL(Config로부터 취득)</param>
        /// <returns>리소스 리스트 취득 결과</returns>
        UniTask<bool> LoadResourceTableProcess( ResourceType resourceType, string serverType, string tableUrl );
    }
}
