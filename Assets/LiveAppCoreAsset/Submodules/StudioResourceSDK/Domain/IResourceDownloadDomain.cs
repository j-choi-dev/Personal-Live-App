using Cysharp.Threading.Tasks;
using StudioNetworkSDK.Domain;
using System;
using System.Collections.Generic;

namespace StudioResourceSDK.Domain
{
    public interface IResourceDownloadDomain
    {
        IObservable<byte[]> OnDownloadComplete { get; }
        IReadOnlyList<string> CurrentResourceList { get; }

        UniTask<bool> InitProcess( CloudConfigData config );
        UniTask<bool> CheckExistProcess( string name );

        /// <remark>완료 시점에 이벤트로 통지</remark>
        UniTask<bool> UpdateObjectList();

        /// <summary>
        /// Download Process
        /// </summary>
        /// <param name="serverType">서버 타입</param>
        /// <param name="name">다운로드 할 파일 명</param>
        /// <returns>Intantiate된 게임 오브젝트</returns>
        /// <remark>완료 시점에 OnLoadCharacter 이벤트로 통지</remark>
        UniTask<UnityEngine.Object> DownloadProcess( ServerType serverType, string name );
    }
}
