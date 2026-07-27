using Amazon;
using Amazon.Runtime;
using Amazon.S3;
using Amazon.S3.Model;
using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using StudioSystemSDK.Domain;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using UniRx;
using UnityEngine;

namespace StudioResourceSDK.Domain
{
    public class S3CloudResourceDownloader : IResourceDownloadDomain
    {
        private IAmazonS3 _s3Client = null;
        private IFileSystemDomain _fileSystemDomain = null;

        private string _regionSystemName = "ap-northeast-2";
        private string _bucketName = "weavr-liveapp-assetbundle";

        private string _targetPath = "iOS/0.0.0.1/character";
        private string _cloudFrontBaseUrl = "https://d2vg1d2gp7bnqk.cloudfront.net/";

        private CloudConfigData _configData = null;

        private readonly Subject<byte[]> _onDownloadComplete = new Subject<byte[]>();
        public IObservable<byte[]> OnDownloadComplete => _onDownloadComplete;

        private readonly List<string> _resourceList = new List<string>();
        public IReadOnlyList<string> CurrentResourceList => _resourceList;

        public S3CloudResourceDownloader( IFileSystemDomain fileSystemDomain)
        {
            _fileSystemDomain = fileSystemDomain;

        }

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public async UniTask<bool> InitProcess( CloudConfigData config)
        {
            _configData = config;
            if( string.IsNullOrWhiteSpace( config.AccessKey ) )
            {
                throw new InvalidOperationException( "AWS ACCESS KEY 값이 설정되지 않았습니다." );
            }
            if( string.IsNullOrWhiteSpace( config.SecretAccessKey ) )
            {
                throw new InvalidOperationException( "AWS SECRET ACCESS KEY 값이 설정되지 않았습니다." );
            }
            RegionEndpoint regionEndpoint = RegionEndpoint.GetBySystemName( _regionSystemName );
            var credentials = new BasicAWSCredentials( _configData.AccessKey, _configData.SecretAccessKey );
            _s3Client = new AmazonS3Client( credentials, regionEndpoint );
            Debug.Log( $"Init Complete :: {config.AccessKey.Trim()}, {config.SecretAccessKey}" );
            return true;
        }

        public async UniTask<byte[]> DownloadProcess( string name )
        {
            if( _s3Client == null )
            {
                throw new NullReferenceException( "IAmazonS3 is NULL" );
            }
            if( string.IsNullOrWhiteSpace( _bucketName ) )
            {
                throw new InvalidDataException( "Bucket Name is NULL" );
            }

            var normalizedName = name.Replace('\\', '/').TrimStart('/');
            var normalizedPrefix = _targetPath.Replace('\\', '/').Trim('/');
            var objectKey = normalizedPrefix + "/" + normalizedName;

            try
            {
                var request = new GetObjectRequest
                {
                    BucketName = _bucketName,
                    Key = objectKey
                };

                using( GetObjectResponse response = await _s3Client.GetObjectAsync( request ) )
                {
                    int initialCapacity = response.ContentLength > 0 && response.ContentLength <= int.MaxValue
                        ? (int)response.ContentLength
                        : 0;

                    using( var memoryStream = initialCapacity > 0
                               ? new MemoryStream( initialCapacity )
                               : new MemoryStream() )
                    {
                        await response.ResponseStream.CopyToAsync( memoryStream );
                        byte[] resourceBytes = memoryStream.ToArray();
                        _onDownloadComplete.OnNext( resourceBytes );
                        return resourceBytes;
                    }
                }
            }
            catch( AmazonS3Exception exception )
                when( exception.StatusCode == HttpStatusCode.NotFound ||
                      string.Equals( exception.ErrorCode, "NoSuchKey", StringComparison.Ordinal ) )
            {
                Debug.LogWarning( $"Could Not Find S3 Object. Bucket={_bucketName}, Key={objectKey}" );
                return null;
            }
            catch( Exception exception )
            {
                Debug.LogError( exception.Message );
                return null;
            }
        }

        public async UniTask<bool> CheckExistProcess( string name )
        {
            if( string.IsNullOrWhiteSpace( _cloudFrontBaseUrl ) )
            {
                throw new InvalidDataException( "CloudFront Base URL is NULL" );
            }

            var normalizedTargetPath = string.IsNullOrWhiteSpace( _targetPath )
                ? string.Empty
                : _targetPath.Replace( '\\', '/' ).Trim( '/' );

            var normalizedName = name.Replace( '\\', '/' ).Trim( '/' );

            var relativePath = string.IsNullOrEmpty( normalizedTargetPath )
                ? normalizedName
                : normalizedTargetPath + "/" + normalizedName;

            var pathSegments = relativePath.Split( new[] { '/' }, StringSplitOptions.RemoveEmptyEntries );
            for( int index = 0; index < pathSegments.Length; index++ )
            {
                pathSegments[index] = Uri.EscapeDataString( pathSegments[index] );
            }

            var requestUrl = _cloudFrontBaseUrl.TrimEnd( '/' ) + "/" + string.Join( "/", pathSegments );
            try
            {
                using( var request = new HttpRequestMessage( HttpMethod.Head, requestUrl ) )
                using( HttpResponseMessage response = await _httpClient.SendAsync( request, HttpCompletionOption.ResponseHeadersRead ) )
                {
                    if( response.StatusCode == HttpStatusCode.NotFound )
                    {
                        return false;
                    }

                    if( response.StatusCode == HttpStatusCode.Forbidden )
                    {
                        Debug.LogError( $"CloudFront 403. Not Exist Target Object... URL={requestUrl}" );
                        return false;
                    }

                    if( response.IsSuccessStatusCode == false )
                    {
                        Debug.LogError( $"CloudFront Return Invalid Code : {( int )response.StatusCode}, Reason={response.ReasonPhrase}, URL={requestUrl}" );
                        return false;
                    }

                    return true;
                }
            }
            catch( HttpRequestException exception )
            {
                Debug.LogError( $"CloudFront Faild :: URL={requestUrl}\n{exception}" );
                return false;
            }
            catch( Exception exception )
            {
                Debug.LogError( $"URL={requestUrl}\n{exception}" );
                return false;
            }
        }

        public async UniTask<bool> UpdateObjectList()
        {
            if( _s3Client == null )
            {
                Debug.LogError( "IAmazonS3가 주입되지 않았습니다." );
                return false;
            }

            if( string.IsNullOrWhiteSpace( _bucketName ) )
            {
                throw new InvalidDataException( "Bucket Name is NULL" );
            }

            var normalizedPrefix = string.IsNullOrWhiteSpace(_targetPath)
                ? string.Empty
                : _targetPath.Replace('\\', '/').Trim('/') + "/";

            var updatedResourceList = new List<string>();
            string continuationToken = null;

            try
            {
                do
                {
                    var request = new ListObjectsV2Request
                    {
                        BucketName = _bucketName,
                        Prefix = normalizedPrefix,
                        ContinuationToken = continuationToken
                    };

                    ListObjectsV2Response response = await _s3Client.ListObjectsV2Async(request);

                    if( response.S3Objects != null )
                    {
                        foreach( S3Object s3Object in response.S3Objects )
                        {
                            if( s3Object == null || string.IsNullOrEmpty( s3Object.Key ) )
                            {
                                continue;
                            }

                            // 콘솔에서 폴더처럼 보이도록 만든 0-byte 오브젝트는 목록에서 제외한다.
                            if( s3Object.Key.EndsWith( "/", StringComparison.Ordinal ) )
                            {
                                continue;
                            }

                            var resourceName = s3Object.Key;
                            // 외부에서는 Prefix를 제외한 상대 경로를 사용한다.
                            if( string.IsNullOrEmpty( normalizedPrefix ) == false &&
                                resourceName.StartsWith( normalizedPrefix, StringComparison.Ordinal ) )
                            {
                                resourceName = resourceName.Substring( normalizedPrefix.Length );
                            }
                            if( !string.IsNullOrEmpty( resourceName ) )
                            {
                                updatedResourceList.Add( resourceName );
                            }
                        }
                    }
                    continuationToken = response.NextContinuationToken;
                    if( response.IsTruncated != true )
                    {
                        break;
                    }
                }
                while( string.IsNullOrEmpty( continuationToken ) == false);

                updatedResourceList.Sort( StringComparer.Ordinal );

                // 전체 목록 취득에 성공한 경우에만 현재 목록을 교체
                _resourceList.Clear();
                _resourceList.AddRange( updatedResourceList );

                return true;
            }
            catch( Exception exception )
            {
                Debug.LogError( exception.Message );
                return false;
            }
        }
    }
}
