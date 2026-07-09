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
        /// <summary>
        /// 현재 선택한 리소스의 서버 타입
        /// </summary>
        /// <param name="resourceType">현재 선택한 리소스 타입</param>
        /// <returns></returns>
        ServerType GetCurrentServerType( ResourceType resourceType );

        /// <summary>
        /// Server Config 정보 취득을 위한 초기화 작업
        /// </summary>
        /// <returns></returns>
        UniTask InitializeServerConfig();
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
    }
}
