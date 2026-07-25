using Amazon.S3;
using Amazon.S3.Model;
using Cysharp.Threading.Tasks;
using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using UniRx;
using UnityEngine;
using Zenject;

namespace StudioResourceSDK.Domain
{
    public class S3CloudResourceDownloader : IResourceDownloadDomain
    {
        private IAmazonS3 _s3Client = null;
        private string _bucketName = null;
        private string _objectPrefix = string.Empty;
        private string _cloudFrontBaseUrl = null;

        private readonly Subject<byte[]> _onDownloadComplete = new Subject<byte[]>();
        public IObservable<byte[]> OnDownloadComplete => _onDownloadComplete;

        private readonly List<string> _resourceList = new List<string>();
        public IReadOnlyList<string> CurrentResourceList => _resourceList;

        public S3CloudResourceDownloader( IAmazonS3 s3Client )
        {
            _s3Client = s3Client;
        }

        private static readonly HttpClient _httpClient = new HttpClient
        {
            Timeout = TimeSpan.FromSeconds(10)
        };

        public async UniTask<byte[]> DownloadProcess( string name )
        {
            if( _s3Client == null )
            {
                Debug.LogError( "[S3CloudResourceDownloader] IAmazonS3가 주입되지 않았습니다." );
                return null;
            }

            if( string.IsNullOrWhiteSpace( _bucketName ) )
            {
                Debug.LogError( "[S3CloudResourceDownloader] S3 버킷 이름이 설정되지 않았습니다." );
                return null;
            }

            if( string.IsNullOrWhiteSpace( name ) )
            {
                Debug.LogWarning( "[S3CloudResourceDownloader] 다운로드할 오브젝트 이름이 비어 있습니다." );
                return null;
            }

            string normalizedName = name.Replace('\\', '/').TrimStart('/');
            string normalizedPrefix = string.IsNullOrWhiteSpace(_objectPrefix)
                ? string.Empty
                : _objectPrefix.Replace('\\', '/').Trim('/');

            string objectKey = string.IsNullOrEmpty(normalizedPrefix)
                ? normalizedName
                : normalizedPrefix + "/" + normalizedName;

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
                Debug.LogWarning( $"[S3CloudResourceDownloader] S3 오브젝트를 찾을 수 없습니다. " +
                    $"Bucket={_bucketName}, Key={objectKey}" );
                return null;
            }
            catch( Exception exception )
            {
                Debug.LogError( $"[S3CloudResourceDownloader] 다운로드에 실패했습니다. " +
                    $"Bucket={_bucketName}, Key={objectKey}\n{exception}" );
                return null;
            }
        }

        public async UniTask<bool> CheckExistProcess( string name )
        {
            if( string.IsNullOrWhiteSpace( _cloudFrontBaseUrl ) )
            {
                Debug.LogError( "[S3CloudResourceDownloader] CloudFront Base URL이 설정되지 않았습니다." );

                return false;
            }

            if( string.IsNullOrWhiteSpace( name ) )
            {
                return false;
            }

            string normalizedName = name .Replace( '\\', '/' ) .TrimStart( '/' );

            string[] pathSegments = normalizedName.Split( '/' );

            for( int index = 0; index < pathSegments.Length; index++ )
            {
                pathSegments[index] = Uri.EscapeDataString( pathSegments[index] );
            }

            string requestUrl = _cloudFrontBaseUrl.TrimEnd( '/' ) + "/" + string.Join( "/", pathSegments );

            try
            {
                using( var request = new HttpRequestMessage(
                           HttpMethod.Head,
                           requestUrl ) )
                using( HttpResponseMessage response =
                           await _httpClient.SendAsync(
                               request,
                               HttpCompletionOption.ResponseHeadersRead ) )
                {
                    if( response.StatusCode == HttpStatusCode.NotFound )
                    {
                        return false;
                    }

                    if( response.StatusCode == HttpStatusCode.Forbidden )
                    {
                        Debug.LogWarning(
                            $"[S3CloudResourceDownloader] CloudFront가 403을 반환했습니다. " +
                            $"오브젝트 부재 또는 Viewer 접근 권한 설정을 확인하세요. " +
                            $"URL={requestUrl}" );

                        return false;
                    }

                    if( !response.IsSuccessStatusCode )
                    {
                        Debug.LogWarning(
                            $"[S3CloudResourceDownloader] CloudFront가 정상적이지 않은 " +
                            $"상태 코드를 반환했습니다. " +
                            $"StatusCode={( int )response.StatusCode} " +
                            $"Reason={response.ReasonPhrase} " +
                            $"URL={requestUrl}" );

                        return false;
                    }

                    return true;
                }
            }
            catch( HttpRequestException exception )
            {
                Debug.LogWarning(
                    $"[S3CloudResourceDownloader] CloudFront 요청에 실패했습니다. " +
                    $"URL={requestUrl}\n{exception}" );

                return false;
            }
            catch( Exception exception )
            {
                Debug.LogWarning(
                    $"[S3CloudResourceDownloader] CloudFront 존재 확인 중 " +
                    $"예외가 발생했습니다. URL={requestUrl}\n{exception}" );

                return false;
            }
        }

        public async UniTask<bool> UpdateObjectList()
        {
            if( _s3Client == null )
            {
                Debug.LogError( "[S3CloudResourceDownloader] IAmazonS3가 주입되지 않았습니다." );
                return false;
            }

            if( string.IsNullOrWhiteSpace( _bucketName ) )
            {
                Debug.LogError( "[S3CloudResourceDownloader] S3 버킷 이름이 설정되지 않았습니다." );
                return false;
            }

            string normalizedPrefix = string.IsNullOrWhiteSpace(_objectPrefix)
                ? string.Empty
                : _objectPrefix.Replace('\\', '/').Trim('/') + "/";

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

                            string resourceName = s3Object.Key;

                            // 외부에서는 Prefix를 제외한 상대 경로를 사용한다.
                            if( !string.IsNullOrEmpty( normalizedPrefix ) &&
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
                while( !string.IsNullOrEmpty( continuationToken ) );

                updatedResourceList.Sort( StringComparer.Ordinal );

                // 전체 목록 취득에 성공한 경우에만 현재 목록을 교체
                _resourceList.Clear();
                _resourceList.AddRange( updatedResourceList );

                return true;
            }
            catch( Exception exception )
            {
                Debug.LogError( "[S3CloudResourceDownloader] S3 오브젝트 목록 갱신에 실패했습니다. " +
                    $"Bucket={_bucketName}, Prefix={normalizedPrefix}\n{exception}" );
                return false;
            }
        }
    }
}
