using System;
using System.Collections.Generic;
using UniRx;

namespace LiveAppUI.Presenter
{
    /// <summary>
    /// Server Room View
    /// </summary>
    public interface IRoomModalView
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
        /// View 나가기 이벤트
        /// </summary>
        IObservable<Unit> OnClickExit { get; }
        /// <summary>
        /// Room 입장 이벤트
        /// </summary>
        IObservable<Unit> OnClickEnter { get; }

        /// <summary>
        /// 현재 표시명
        /// </summary>
        string CurrentName { get; }
        /// <summary>
        /// 현재 선택한 Room Index
        /// </summary>
        int CurrenIndex { get; }

        /// <summary>
        /// Room List 세팅
        /// </summary>
        /// <param name="list">Room 리스트</param>
        void SetRoomList( IReadOnlyList<string> list );

        /// <summary>
        /// 로그인창 활성화/비활성화
        /// </summary>
        /// <param name="isActive">Active값</param>
        void SetActive( bool isActive );

        void SetNameWithoutNotify( string name );
    }
}
