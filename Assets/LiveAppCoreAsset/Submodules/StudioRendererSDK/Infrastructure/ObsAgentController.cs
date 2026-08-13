using Cysharp.Threading.Tasks;
using StudioRendererSDK.Domain;
using System;
using UniRx;
using UnityEngine;
using UnityEngine.Networking;

namespace StudioRendererSDK.Infrastructure
{
    public class ObsAgentController : IObsAgentControlDomain
    {
        private const int DefaultAgentPort = 7443;

        private const string EndpointPreferenceKey = "ObsAgent.Endpoint";

        private const string TokenPreferenceKey = "ObsAgent.Token";
        private int healthTimeoutSeconds = 5;
        private int obsTestTimeoutSeconds = 10;
        private int requestTimeoutSeconds = 15;

        private Subject<string> _onSystemMessageChanged = new Subject<string>();
        public IObservable<string> OnSystemMessageChanged => _onSystemMessageChanged;

        private Subject<string> _onEndPointChanged = new Subject<string>();
        public IObservable<string> OnEndPointChanged => _onEndPointChanged;

        private Subject<string> _onAgentTokenChanged = new Subject<string>();
        public IObservable<string> OnAgentTokenChanged => _onAgentTokenChanged;


        private Subject<bool> _onConnectionChanged = new Subject<bool>();
        public IObservable<bool> OnConnectionChanged => _onConnectionChanged;

        private Subject<bool> _onStreamingChanged = new Subject<bool>();
        public IObservable<bool> OnStreamingChanged => _onStreamingChanged;

        private Subject<bool> _onRecordingChanged = new Subject<bool>();
        public IObservable<bool> OnRecordingChanged => _onRecordingChanged;

        public bool IsConnected { get; private set; }
        public bool IsStreaming { get; private set; }
        public bool IsRecording { get; private set; }

        public string AgentEndpoint { get; private set; }

        public string AgentToken { get; private set; }

        public ObsAgentController()
        {
            LoadSavedSettings();
            _onSystemMessageChanged.OnNext( "[System] Plaease Input OBS Agent Endpoint & Token" );
        }

        public async UniTask<bool> AgentConnectProcess( string endPoint, string token )
        {
            IsConnected = false;
            var msg = string.Empty;
            try
            {
                var normalizedEndPoint = NormalizeEndpoint( endPoint );
                var normalizedToken = token.Trim();

                if( string.IsNullOrWhiteSpace( normalizedToken ) )
                {
                    msg = "[Error] Agent Token is NULL";
                    _onSystemMessageChanged.OnNext( msg );
                    Debug.LogError( msg );
                    throw new InvalidOperationException( msg );
                }

                if( normalizedToken.Length < 16 )
                {
                    msg = "[Error] Agent Token Character Count is Under 16";
                    _onSystemMessageChanged.OnNext( msg );
                    Debug.LogError( msg );
                    throw new InvalidOperationException( msg );
                }
                // 1단계: Agent 서버 도달 여부 확인
                var healthUrl = $"{normalizedEndPoint}/health";
                msg =  $"[SYSTEM] Agent Connecting :: {healthUrl}";
                _onSystemMessageChanged.OnNext( msg );
                using( UnityWebRequest healthRequest = UnityWebRequest.Get( healthUrl ) )
                {
                    healthRequest.timeout = Mathf.Max( 1, healthTimeoutSeconds );
                    await healthRequest.SendWebRequest();

                    if( healthRequest.result != UnityWebRequest.Result.Success )
                    {
                        msg = "[Error] Couldn't Connect OBS Agent\nURL: {healthRequest.url}\nResult: {healthRequest.result}\nHTTP: {healthRequest.responseCode}\nError: {healthRequest.error}";
                        _onSystemMessageChanged.OnNext( msg );
                        Debug.LogError( msg );
                        return false;
                    }

                    AgentApiResponse healthResponse = ParseApiResponse( healthRequest.downloadHandler.text);

                    if( healthResponse != null && healthResponse.success == false )
                    {
                        msg =  $"[Error] Agent Returned Error  :: {healthResponse.message}";
                        _onSystemMessageChanged.OnNext( msg );
                        Debug.LogError( msg );
                        return false;
                    }
                }

                // 2단계: Token + OBS WebSocket 연결 확인
                var obsTestUrl = $"{normalizedEndPoint}/api/obs/test";

                msg =  $"[SYSTEM] Agent Connecting Successed, OBS WebSocket Connecting...";
                _onSystemMessageChanged.OnNext( msg );

                using( var obsTestRequest = new UnityWebRequest( obsTestUrl, UnityWebRequest.kHttpVerbPOST ) )
                {
                    obsTestRequest.uploadHandler = new UploadHandlerRaw( Array.Empty<byte>() );
                    obsTestRequest.downloadHandler = new DownloadHandlerBuffer();

                    obsTestRequest.SetRequestHeader( "Authorization", $"Bearer {normalizedToken}" );
                    obsTestRequest.SetRequestHeader( "Content-Type", "application/json" );

                    obsTestRequest.timeout = Mathf.Max( 1, obsTestTimeoutSeconds );

                    await obsTestRequest.SendWebRequest();

                    AgentApiResponse testResponse = ParseApiResponse( obsTestRequest.downloadHandler.text);

                    if( obsTestRequest.responseCode == 401 )
                    {
                        msg = $"[Error] Agent Token Invalid";
                        _onSystemMessageChanged.OnNext( msg );
                        Debug.LogError( msg );
                        return false;
                    }

                    if( obsTestRequest.result != UnityWebRequest.Result.Success )
                    {
                        var detail = testResponse != null && string.IsNullOrWhiteSpace( testResponse.message) == false
                            ? testResponse.message
                            : obsTestRequest.error;

                        msg = $"[Error] OBS Connecting FAILED :: {detail}";
                        _onSystemMessageChanged.OnNext( msg );
                        Debug.LogError( msg );
                        return false;
                    }

                    if( testResponse == null )
                    {
                        msg = $"[Error] Agent Returned Invalid Response";
                        _onSystemMessageChanged.OnNext( msg );
                        Debug.LogError( msg );
                        return false;
                    }

                    if( testResponse.success == false)
                    {
                        msg = $"[Error] OBS Connecting FAILED :: {testResponse.message}";
                        _onSystemMessageChanged.OnNext( msg );
                        Debug.LogError( msg );
                        return false;
                    }
                }

                AgentEndpoint = normalizedEndPoint;
                AgentToken = normalizedToken;
                IsConnected = true;
                _onConnectionChanged.OnNext( true );

                SaveSettings( normalizedEndPoint, normalizedToken );
                _onEndPointChanged.OnNext( normalizedEndPoint );
                msg = $"[SYSTEM] OBS Agent Connecting SUCCESSED\nEndpoint: {normalizedEndPoint}\nOBS WebSocket Connected";
                _onSystemMessageChanged.OnNext( msg );
                return true;
            }
            catch( Exception exception )
            {
                msg = $"[Error] {exception.Message}";
                _onSystemMessageChanged.OnNext( msg );
                Debug.LogError( msg );
                return false;
            }
        }

        public async UniTask<bool> StartRecordingProcess()
            => await SendCommand( "/api/obs/record/start", "OBS 녹화 시작 요청 중...", "OBS 녹화를 시작했습니다." );
        public async UniTask<bool> StopRecordingProcess()
            => await SendCommand( "/api/obs/record/stop", "OBS 녹화 종료 요청 중...", "OBS 녹화를 종료했습니다." );

        public async UniTask<bool> StartStreamingProcess()
            => await SendCommand( "/api/obs/stream/start", "OBS 스트리밍 시작 요청 중...", "OBS 스트리밍을 시작했습니다." );
        public async UniTask<bool> StopStreamingProcess()
            => await SendCommand( "/api/obs/stream/stop", "OBS 스트리밍 종료 요청 중...", "OBS 스트리밍을 종료했습니다." );

        private async UniTask<bool> SendCommand( string apiPath, string pendingMessage, string successMessage )
        {
            var msg = string.Empty;
            var endpoint = PlayerPrefs.GetString( EndpointPreferenceKey, string.Empty);
            var token = PlayerPrefs.GetString( TokenPreferenceKey, string.Empty);
            if( string.IsNullOrWhiteSpace( endpoint ) )
            {
                msg = "[ERROR] OBS Agent Endpoint is NULL";
                _onSystemMessageChanged.OnNext( msg );
                Debug.LogError( msg );
                return false;
            }

            if( string.IsNullOrWhiteSpace( token ) )
            {
                msg = "[ERROR] OBS Agent Token is NULL";
                _onSystemMessageChanged.OnNext( msg );
                Debug.LogError( msg );
                return false;
            }

            endpoint = endpoint.TrimEnd( '/' );
            var requestUrl = $"{endpoint}{apiPath}";

            msg = $"{pendingMessage}\n{requestUrl}";
            _onSystemMessageChanged.OnNext( msg );

            using( var request = new UnityWebRequest( requestUrl, UnityWebRequest.kHttpVerbPOST ) )
            {
                request.uploadHandler = new UploadHandlerRaw( Array.Empty<byte>() );
                request.downloadHandler = new DownloadHandlerBuffer();
                request.SetRequestHeader( "Authorization", $"Bearer {token}" );
                request.SetRequestHeader( "Content-Type", "application/json" );
                request.timeout = Mathf.Max( 1, requestTimeoutSeconds );
                UnityWebRequestAsyncOperation operation;

                try
                {
                    operation = request.SendWebRequest();
                }
                catch( InvalidOperationException exception )
                {
                    msg = $"HTTP 요청을 시작하지 못했습니다.\n{exception.Message}";
                    _onSystemMessageChanged.OnNext( msg );
                    Debug.LogError( msg );
                    return false;
                }

                await operation;
 
                AgentApiResponse response = ParseApiResponse( request.downloadHandler.text);

                if( request.responseCode == 401 )
                {
                    msg = $"Agent Token이 올바르지 않습니다.";
                    _onSystemMessageChanged.OnNext( msg );
                    Debug.LogError( msg );
                    return false;
                }

                if( request.result != UnityWebRequest.Result.Success )
                {
                    string detail = response != null && string.IsNullOrWhiteSpace( response.message) == false
                            ? response.message
                            : request.error;

                    msg =  $"OBS 스트리밍 명령 실패\nHTTP: {request.responseCode}\n오류: {detail}";
                    _onSystemMessageChanged.OnNext( msg );
                    Debug.LogError( msg );
                    return false;
                }

                if( response == null )
                {
                    msg = "Agent가 올바르지 않은 응답을 반환했습니다.";
                    _onSystemMessageChanged.OnNext( msg );
                    Debug.LogError( msg );
                    return false;
                }

                if( response.success == false )
                {
                    msg = $"OBS 명령 실패\n{response.message}";
                    _onSystemMessageChanged.OnNext( msg );
                    return false;
                }

                msg = $"{successMessage}\nAgent: {endpoint}";
                _onSystemMessageChanged.OnNext( msg );
                return true;
            }
        }
        private void LoadSavedSettings()
        {
            var savedEndpoint = PlayerPrefs.GetString( EndpointPreferenceKey, string.Empty);
            var savedToken = PlayerPrefs.GetString( TokenPreferenceKey, string.Empty);

            _onEndPointChanged.OnNext( savedEndpoint );
            _onAgentTokenChanged.OnNext( savedToken );
        }
        private void SaveSettings( string endpoint, string token )
        {
            PlayerPrefs.SetString( EndpointPreferenceKey, endpoint );
            PlayerPrefs.SetString( TokenPreferenceKey, token );

            PlayerPrefs.Save();
        }

        private string NormalizeEndpoint( string input )
        {
            if( string.IsNullOrWhiteSpace( input ) )
            {
                throw new InvalidOperationException( "Endpoint is Null" );
            }

            var normalized = input.Trim();

            if( normalized.Contains( "://" ) == false )
            {
                normalized = $"http://{normalized}";
            }

            if( !Uri.TryCreate( normalized, UriKind.Absolute, out Uri uri ) )
            {
                throw new InvalidOperationException( "Endpoint is Invalid Type" );
            }

            if( !string.Equals( uri.Scheme, Uri.UriSchemeHttp, StringComparison.OrdinalIgnoreCase ) &&
                !string.Equals( uri.Scheme, Uri.UriSchemeHttps, StringComparison.OrdinalIgnoreCase ) )
            {
                throw new InvalidOperationException( "Endpoint is Not http or https Type" );
            }

            var builder = new UriBuilder(uri);

            // 포트를 입력하지 않은 경우 7443 사용

            int schemeIndex = normalized.IndexOf( "://", StringComparison.Ordinal);
            if( schemeIndex >= 0 )
            {
                normalized = normalized.Substring( schemeIndex + 3 );
            }

            int slashIndex = normalized.IndexOf('/');

            if( slashIndex >= 0 )
            {
                normalized = normalized.Substring( 0, slashIndex );
            }

            // 현재 구성은 IPv4 주소 또는 호스트 이름을 대상으로 한다.
            if( normalized.LastIndexOf( ':' ) < 0 )
            {
                builder.Port = 7443;
            }

            builder.Path = string.Empty;
            builder.Query = string.Empty;
            builder.Fragment = string.Empty;

            return builder.Uri
                .GetLeftPart( UriPartial.Authority )
                .TrimEnd( '/' );
        }

        private AgentApiResponse ParseApiResponse( string json )
        {
            if( string.IsNullOrWhiteSpace( json ) )
            {
                return null;
            }

            try
            {
                return JsonUtility .FromJson<AgentApiResponse>( json );
            }
            catch( Exception )
            {
                return null;
            }
        }
    }
}
