using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using StudioResourceSDK.Domain;
using StudioSystemSDK.Domain;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    public class ResourceTableContext : IResourceTableContext
    {
        private IResourceTableLoadDomain _resourceTableLoadDomain;
        private IGoogleAuthTokenDomain _tokenDomain;
        private IResourceDataParseDomain _resourceDataParseDomain;

        public IObservable<IReadOnlyCollection<CharacterResourceItem>> OnCharacterListChanged 
            => _resourceDataParseDomain.OnCharacterListChanged;

        public IObservable<IReadOnlyCollection<StageResourceItem>> OnStageListChanged
            => _resourceDataParseDomain.OnStageListChanged;

        public IObservable<IReadOnlyCollection<PropResourceItem>> OnPropListChanged
            => _resourceDataParseDomain.OnPropListChanged;

        public ResourceTableContext( IResourceTableLoadDomain resourceTableLoadDomain,
            IGoogleAuthTokenDomain tokenDomain,
            IResourceDataParseDomain resourceDataParseDomain )
        {
            _resourceTableLoadDomain = resourceTableLoadDomain;
            _tokenDomain = tokenDomain;
            _resourceDataParseDomain = resourceDataParseDomain;
        }

        public async UniTask<bool> LoadResourceTableProcess( ResourceType type, string serverType, string tableUrl )
        {
            bool exists = await _resourceTableLoadDomain.ExistsSheetAndTabAsync(
                tableUrl,
                serverType,
                _tokenDomain.Token
            );
            if( !exists )
            {
                Debug.LogError(
                    $"Google Sheet or tab not found, or permission denied. Sheet: {tableUrl}, Tab: {serverType}"
                );
                return false;
            }
            string tableText = await _resourceTableLoadDomain.LoadVariableRangeAsStringAsync(
                _tokenDomain.Token,
                tableUrl,
                serverType,
                columnDelimiter: ",",
                rowDelimiter: "\n",
                escapeCellLineBreaks: true
            );
            Debug.Log( $"LoadResourceTableProcess :: {type} / {serverType}" );
            var result = _resourceDataParseDomain.ParseCharacterData( tableText );
            return true;
        }

        public async UniTask<bool> InitProcess()
        {
            return true;
        }
    }
}
