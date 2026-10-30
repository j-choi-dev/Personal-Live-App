using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;

namespace LiveAppUI.Model
{
    /// <summary>
    /// Resource List Model
    /// </summary>
    public interface IResourceListModel
    {
        /// <summary>
        /// 로직에 의한 캐릭터 리스트 변경 이벤트
        /// </summary>
        IObservable <IReadOnlyList<(string id, string displayName)>> OnCharacterListChanged { get; }
        IObservable<IReadOnlyList<(string id, string displayName)>> OnBackStageListChanged { get; }
        IObservable<IReadOnlyList<(string id, string displayName)>> OnPropListChanged { get; }
        /// <summary>
        /// 씬에 '로드 된' 오브젝트 리스트 변경 이벤트
        /// </summary>
        public IObservable<IReadOnlyList<(ResourceType type, string id, string displayName)>> OnSceneResourceListChanged { get; }
        /// <summary>
        /// 현재 선택한 리소스의 서버 타입
        /// </summary>
        /// <param name="resourceType">현재 선택한 리소스 타입</param>
        /// <returns></returns>
        ServerType GetCurrentServerType( ResourceType resourceType );

        bool IsLoading { get; }

        /// <summary>
        /// Server Config 정보 취득을 위한 초기화 작업
        /// </summary>
        /// <returns></returns>
        UniTask<bool> InitializeServerConfig();

        IObservable<ResourceListFailureData> OnLoadFailed { get; }

        /// <summary>
        /// 선택한 리소스 및 서버 타입의 리스트
        /// </summary>
        /// <param name="resourceType">선택한 리소스</param>
        /// <param name="serverType">선택한 서버</param>
        /// <returns></returns>
        UniTask GetResourceList( ResourceType resourceType, ServerType serverType );
        /// <summary>
        /// 현재 선택한 서버 타입을 세팅
        /// </summary>
        /// <param name="resourceType">대상 리소스 타입</param>
        /// <param name="serverType">변경할 서버 타입</param>
        void SetCurrentServerType( ResourceType resourceType, ServerType serverType );

        /// <summary>
        /// 리소스 로딩 프로세스
        /// </summary>
        /// <param name="resourceType">대상 리소스 타입</param>
        /// <param name="serverType">변경할 서버 타입</param>
        /// <param name="list">리소스 ID</param>
        /// <returns></returns>
        UniTask<bool> LoadResourceProcess( ResourceType resourceType, 
            ServerType serverType, 
            IReadOnlyList<string> list );
        /// <summary>
        /// 리소스 삭제 프로세스
        /// </summary>
        /// <param name="targets">삭제할 오브젝트 (type, id)튜플 리스트</param>
        void RemoveResourcesProcess(IReadOnlyList<(ResourceType type, string id)> targets);
        /// <summary>
        /// 타입에 따른 필터링 후 리스트 초기화
        /// </summary>
        /// <param name="resourceType">필터링 할 타입</param>
        void FilterRemoveResource(ResourceType resourceType);

        UniTask ReauthorizeAndGetResourceList( ResourceType resourceType, ServerType serverType);
    }
}
