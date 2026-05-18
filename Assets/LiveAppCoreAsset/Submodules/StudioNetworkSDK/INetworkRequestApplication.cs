using Cysharp.Threading.Tasks;
using System;

namespace StudioNetworkSDK.Application
{
    /// <summary>
    /// 네트워크 통신 중 Send를 담당하는 구현클래스를 위한 Interface
    /// </summary>
    public interface INetworkRequestApplication
    {
        IObservable<bool> OnLoginResult { get; }

        /// <summary>
        /// 초기화 여부
        /// </summary>
        /// <returns>초기화 성공/실패</returns>
        UniTask<bool> Initialize();

        /// <summary>
        /// MqTT 서버 로그인 리퀘스트를 처리
        /// </summary>
        /// <param name="id">User ID</param>
        /// <param name="pw">User Password</param>
        /// <returns>로그인 리퀘스트 송신 결과</returns>
        UniTask<bool> SendLoginRequest( string id, string pw );
        UniTask<bool> SendLoginRequest_Test( string id, string pw );
    }
}
