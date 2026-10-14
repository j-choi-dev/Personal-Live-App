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

            var accessToken = await _tokenDomain.GetAccessTokenAsync();

            return await LoadTableWithTokenAsync( type, serverType, tableUrl, accessToken);

            //if ( string.IsNullOrWhiteSpace( accessToken ) )
            //{
            //    Debug.LogError( "[GoogleSheets] Access token is empty." );
            //    return false;
            //}
            //var isExists = await _resourceTableLoadDomain.ExistsSheetAndTabAsync(
            //    tableUrl,
            //    serverType,
            //    accessToken
            //);
            //if( isExists == false )
            //{
            //    Debug.LogError( $"Google Sheet or tab not found, or permission denied. Sheet: {tableUrl}, Tab: {serverType}" );
            //    return false;
            //}
            //string tableText = await _resourceTableLoadDomain.LoadVariableRangeAsStringAsync(
            //    accessToken,
            //    tableUrl,
            //    serverType,
            //    columnDelimiter: ",",
            //    rowDelimiter: "\n",
            //    escapeCellLineBreaks: true
            //);
            //Debug.Log( $"LoadResourceTableProcess :: {type} / {serverType}" );
            //var result = _resourceDataParseDomain.ParseCharacterData( tableText );
            //return true;
        }
        public async UniTask<bool> ReauthorizeAndLoadResourceTableProcess( ResourceType type, string serverType, string tableUrl)
        {
            var accessToken = await _tokenDomain.ReauthorizeAsync();

            return await LoadTableWithTokenAsync( type, serverType, tableUrl, accessToken );
        }

        private async UniTask<bool> LoadTableWithTokenAsync( ResourceType type, string serverType, string tableUrl, string accessToken)
        {
            if (string.IsNullOrWhiteSpace(accessToken))
            {
                throw new InvalidOperationException( "Google access token is empty." );
            }

            var isExists = await _resourceTableLoadDomain.ExistsSheetAndTabAsync( tableUrl, serverType, accessToken );

            if (isExists == false )
            {
                return false;
            }

            var tableText = await _resourceTableLoadDomain.LoadVariableRangeAsStringAsync(
                    accessToken,
                    tableUrl,
                    serverType,
                    columnDelimiter: ",",
                    rowDelimiter: "\n",
                    escapeCellLineBreaks: true );

            Debug.Log($"LoadResourceTableProcess :: {type} / {serverType}");

            var result = _resourceDataParseDomain.ParseCharacterData(tableText);
            return true;
        }


        public async UniTask<bool> InitProcess()
        {
            return true;
        }

    }
}
