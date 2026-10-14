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
            return await LoadResourceTableCoreAsync( type, serverType, tableUrl, reauthorize: false);
        }
        public async UniTask<bool> ReauthorizeAndLoadResourceTableProcess( ResourceType type, string serverType, string tableUrl)
        {
            return await LoadResourceTableCoreAsync( type, serverType, tableUrl, reauthorize: true);
        }

        private async UniTask<bool> LoadResourceTableCoreAsync( ResourceType type,
            string serverType,
            string tableUrl,
            bool reauthorize)
        {
            try
            {
                var accessToken = reauthorize
                    ? await _tokenDomain.ReauthorizeAsync()
                    : await _tokenDomain.GetAccessTokenAsync();

                return await LoadTableWithTokenAsync(
                    type, serverType, tableUrl, accessToken);
            }
            catch (OperationCanceledException)
            {
                throw;
            }
            catch (GoogleSheetsRequestException e)
            {
                var kind = e.StatusCode == 401 ? ResourceTableFailureKind.Unauthorized : e.StatusCode == 403
                        ? ResourceTableFailureKind.PermissionDenied : ResourceTableFailureKind.Other;

                throw new ResourceTableLoadException( kind, e.StatusCode, e);
            }
            catch (GoogleNetworkException e)
            {
                throw new ResourceTableLoadException( ResourceTableFailureKind.Network, 0, e);
            }
            catch (OAuthAuthorizationDeniedException e)
            {
                throw new ResourceTableLoadException( ResourceTableFailureKind.AuthorizationCancelled, 0, e);
            }
            catch (OAuthBrowserClosedException e)
            {
                throw new ResourceTableLoadException( ResourceTableFailureKind.AuthorizationCancelled, 0, e);
            }
            catch (Exception e)
            {
                throw new ResourceTableLoadException( ResourceTableFailureKind.Other, 0, e);
            }
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

            switch(type)
            {
                case ResourceType.Character:
                    var characterListResult = _resourceDataParseDomain.ParseCharacterData(tableText);
                    return true;
                case ResourceType.BackGround:
                    var stageListResult = _resourceDataParseDomain.ParseStageData(tableText);
                    return true;
                case ResourceType.Prop:
                    var propListResult = _resourceDataParseDomain.ParsePropData(tableText);
                    return true;
                default: 
                    return false;
            }
        }


        public async UniTask<bool> InitProcess()
        {
            return true;
        }

    }
}
