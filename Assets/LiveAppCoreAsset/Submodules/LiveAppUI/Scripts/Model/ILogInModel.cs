using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace LiveAppUI.Model
{
    /// <summary>
    /// Login Model
    /// </summary>
    public interface ILogInModel
    {
        /// <summary>
        /// 서버 리스트
        /// </summary>
        IReadOnlyList<string> ServerList { get; }
        /// <summary>
        /// 방 리스트
        /// </summary>
        IReadOnlyList<string> RoomList { get; }
        /// <summary>
        /// 로그인 결과
        /// </summary>
        IObservable<bool> OnLoginSuccess { get; }
        /// <summary>
        /// 방 입장 결과
        /// </summary>
        IObservable<bool> OnRoomEnterSuccess { get; }

        /// <summary>
        /// 초기화 여부
        /// </summary>
        /// <returns>초기화 성공/실패</returns>
        UniTask<bool> Initialize();

        /// <summary>
        /// 로그인 처리
        /// </summary>
        /// <param name="id">D</param>
        /// <param name="pw">PW</param>
        /// <param name="item">서버 종류</param>
        /// <returns>비동기 처리</returns>

        UniTask LoginProcess( string id, string pw, ServerItem item );

        /// <summary>
        /// 방 입장 결과
        /// </summary>
        /// <param name="index">방 번호</param>
        /// <param name="name">유저 이름</param>
        /// <returns>비동기 처리</returns>
        UniTask RoomEnterProcess( int index, string name );
    }
}
