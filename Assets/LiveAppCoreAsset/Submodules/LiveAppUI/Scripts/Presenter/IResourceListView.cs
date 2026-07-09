using System;
using System.Collections.Generic;
using UniRx;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// 리소스 리스트 View Interface
    /// </summary>
    public interface IResourceListView : IViewBase
    {
        /// <summary>
        /// Server 변경 이벤트
        /// </summary>
        IObservable<int> OnServerChange { get; }
        /// <summary>
        /// 닫기 버튼 클릭 이벤트
        /// </summary>
        IObservable<Unit> OnClickClose { get; }
        /// <summary>
        /// 취소 버튼 클릭
        /// </summary>
        IObservable<Unit> OnClickCancle { get; }
        
        /// <summary>
        /// 현재 선택한 서버의 Dropdown Index
        /// </summary>
        int CurrentServerIndex { get; }

        /// <summary>
        /// List View 타이틀에 표시할 리소스 타입을 세팅
        /// </summary>
        /// <param name="title">리소스 타입</param>
        void SetTitle( string title );
        /// <summary>
        /// 서버 리스트 세팅
        /// </summary>
        /// <param name="servers">서버 리스트</param>
        void SetServerList( IReadOnlyList<string> servers );
        /// <summary>
        /// Dropdown Index를 기준으로 서버명을 Dropdown에 표시
        /// </summary>
        /// <param name="index">Dropdown Index</param>
        void SetServerItem( int index );
        /// <summary>
        /// 리스트뷰에 Resource List를 세팅
        /// </summary>
        /// <param name="list">Resource List</param>
        void SetResourceItemList( IReadOnlyList<(string id, string name)> list );
        /// <summary>
        /// 리스트뷰에 Resource Item을 추가
        /// </summary>
        /// <param name="id">Item ID(=Resource ID)</param>
        /// <param name="name">표시명</param>
        void AddListItem( string id, string name );
        /// <summary>
        /// 리스트View의 Item을 삭제
        /// </summary>
        /// <param name="id"></param>
        void RemoveListItem( string id );

        /// <summary>
        /// 리스트를 초기화
        /// </summary>
        void ClearList();
    }
}
