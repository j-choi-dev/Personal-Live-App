using Cysharp.Threading.Tasks;
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

        /// <remark>완료 시점에 OnLoadCharacter 이벤트로 통지</remark>
        UniTask<UnityEngine.Object> DownloadProcess( string name );
    }
}
