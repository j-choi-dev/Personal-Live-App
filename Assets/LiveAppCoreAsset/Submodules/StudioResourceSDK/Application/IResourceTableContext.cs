using Cysharp.Threading.Tasks;
using StudioResourceSDK.Domain;
using System;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public interface IResourceTableContext
    {
        // Google Sheet 접속
        // 파싱
        // 리소스 리스트로 반환
        IObservable<ResourceTableData> OnCharacterListChanged { get; }
        IObservable<ResourceTableData> OnStageListChanged { get; }
        IObservable<ResourceTableData> OnPropListChanged { get; }

        UniTask<bool> InitProcess();
        UniTask<bool> LoadResourceTableProcess( ResourceType tyoe );
    }
}
