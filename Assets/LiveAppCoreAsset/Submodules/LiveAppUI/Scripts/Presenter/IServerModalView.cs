using System;
using System.Collections.Generic;
using UniRx;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// Server 로그인 View
    /// </summary>
    public interface IServerModalView
    {
        /// <summary>
        /// View Active 상태
        /// </summary>
        bool IsActive { get; }

        /// <summary>
        /// View 닫기 이벤트
        /// </summary>
        IObservable<Unit> OnClose { get; }
        /// <summary>
        /// Login 클릭 이벤트
        /// </summary>
        IObservable<Unit> OnClicLogin { get; }

        /// <summary>
        /// 현재 Login ID
        /// </summary>
        string CurrentID { get; }
        /// <summary>
        /// 현재 Password
        /// </summary>
        string CurrentPassword { get; }
        /// <summary>
        /// 현재 Dropdown을 통해 선택한 서버 Index 
        /// </summary>
        int CurrentIndex { get; }

        /// <summary>
        /// Server List 세팅
        /// </summary>
        /// <param name="list"></param>
        void SetServerList( IReadOnlyList<string> list );

        /// <summary>
        /// 로그인창 활성화/비활성화
        /// </summary>
        /// <param name="isActive">Active값</param>
        void SetActive( bool isActive );

        void SetServerIdWithoutNotify( string id );
        void SetServerPasswordWithoutNotify( string password );
    }
}
