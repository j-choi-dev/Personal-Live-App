using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ObsAgent.Client
{
    public sealed class ObsAgentConnectionController : MonoBehaviour
    {
        private const int DefaultAgentPort = 7443;

        private const string EndpointPreferenceKey =
            "ObsAgent.Endpoint";

        private const string TokenPreferenceKey =
            "ObsAgent.Token";

        [Header("Connection UI")]
        [SerializeField]
        private TMP_InputField endpointInput;

        [SerializeField]
        private TMP_InputField agentTokenInput;

        [SerializeField]
        private Button connectButton;

        [SerializeField]
        private TMP_Text statusText;

        [Header("Options")]
        [SerializeField]
        private int healthTimeoutSeconds = 5;

        [SerializeField]
        private int obsTestTimeoutSeconds = 10;

        [Tooltip(
            "개발 단계에서는 편하지만 PlayerPrefs는 암호화 저장소가 아닙니다.")]
        [SerializeField]
        private bool saveTokenToPlayerPrefs = true;

        public bool IsConnected { get; private set; }

        public string AgentEndpoint { get; private set; }

        public string AgentToken { get; private set; }

        private Coroutine _connectionCoroutine;

        private void Awake()
        {
            //ValidateUiReferences();
            LoadSavedSettings();

            connectButton.onClick.AddListener(
                OnConnectButtonClicked );

            SetStatus(
                "OBS Agent Endpoint와 Token을 입력하세요." );
        }

        private void OnDestroy()
        {
            if( connectButton != null )
            {
                connectButton.onClick.RemoveListener(
                    OnConnectButtonClicked );
            }
        }

        private void OnConnectButtonClicked()
        {
            if( _connectionCoroutine != null )
            {
                return;
            }

            _connectionCoroutine =
                StartCoroutine( VerifyConnection() );
        }

        private IEnumerator VerifyConnection()
        {
            IsConnected = false;
            SetInteractable( false );

            string endpoint;
            string token;

            try
            {
                endpoint = NormalizeEndpoint(
                    endpointInput.text );

                token = agentTokenInput.text.Trim();

                if( string.IsNullOrWhiteSpace( token ) )
                {
                    throw new InvalidOperationException(
                        "Agent Token을 입력하세요." );
                }

                if( token.Length < 16 )
                {
                    throw new InvalidOperationException(
                        "Agent Token은 16자 이상이어야 합니다." );
                }
            }
            catch( Exception exception )
            {
                SetStatus(
                    $"입력 오류\n{exception.Message}" );

                FinishConnectionAttempt();
                yield break;
            }

            // -------------------------------------------------
            // 1단계: Agent 서버 도달 여부 확인
            // -------------------------------------------------

            string healthUrl =
                $"{endpoint}/health";

            SetStatus(
                $"Agent 연결 확인 중...\n{healthUrl}" );

            using( UnityWebRequest healthRequest =
                   UnityWebRequest.Get( healthUrl ) )
            {
                healthRequest.timeout =
                    Mathf.Max( 1, healthTimeoutSeconds );

                yield return healthRequest.SendWebRequest();

                if( healthRequest.result !=
                    UnityWebRequest.Result.Success )
                {
                    SetStatus(
                        BuildNetworkErrorMessage(
                            "OBS Agent에 연결할 수 없습니다.",
                            healthRequest ) );

                    FinishConnectionAttempt();
                    yield break;
                }

                AgentApiResponse healthResponse =
                    ParseApiResponse(
                        healthRequest.downloadHandler.text);

                if( healthResponse != null &&
                    !healthResponse.success )
                {
                    SetStatus(
                        $"Agent가 오류를 반환했습니다.\n" +
                        healthResponse.message );

                    FinishConnectionAttempt();
                    yield break;
                }
            }

            // -------------------------------------------------
            // 2단계: Token + OBS WebSocket 연결 확인
            // -------------------------------------------------

            string obsTestUrl =
                $"{endpoint}/api/obs/test";

            SetStatus(
                "Agent 연결 성공\n" +
                "OBS WebSocket 연결을 확인하는 중..." );

            using( var obsTestRequest =
                   new UnityWebRequest(
                       obsTestUrl,
                       UnityWebRequest.kHttpVerbPOST ) )
            {
                obsTestRequest.uploadHandler =
                    new UploadHandlerRaw(
                        Array.Empty<byte>() );

                obsTestRequest.downloadHandler =
                    new DownloadHandlerBuffer();

                obsTestRequest.SetRequestHeader(
                    "Authorization",
                    $"Bearer {token}" );

                obsTestRequest.SetRequestHeader(
                    "Content-Type",
                    "application/json" );

                obsTestRequest.timeout =
                    Mathf.Max( 1, obsTestTimeoutSeconds );

                yield return obsTestRequest.SendWebRequest();

                AgentApiResponse testResponse =
                    ParseApiResponse(
                        obsTestRequest.downloadHandler.text);

                if( obsTestRequest.responseCode == 401 )
                {
                    SetStatus(
                        "Agent 연결은 성공했지만 " +
                        "Agent Token이 올바르지 않습니다." );

                    FinishConnectionAttempt();
                    yield break;
                }

                if( obsTestRequest.result !=
                    UnityWebRequest.Result.Success )
                {
                    string detail =
                        testResponse != null &&
                        !string.IsNullOrWhiteSpace(
                            testResponse.message)
                            ? testResponse.message
                            : obsTestRequest.error;

                    SetStatus(
                        "Agent 연결은 성공했습니다.\n" +
                        "하지만 OBS 연결 확인에 실패했습니다.\n" +
                        detail );

                    FinishConnectionAttempt();
                    yield break;
                }

                if( testResponse == null )
                {
                    SetStatus(
                        "Agent가 올바르지 않은 응답을 반환했습니다." );

                    FinishConnectionAttempt();
                    yield break;
                }

                if( !testResponse.success )
                {
                    SetStatus(
                        "OBS 연결 확인 실패\n" +
                        testResponse.message );

                    FinishConnectionAttempt();
                    yield break;
                }
            }

            AgentEndpoint = endpoint;
            AgentToken = token;
            IsConnected = true;

            SaveSettings( endpoint, token );

            endpointInput.SetTextWithoutNotify( endpoint );

            SetStatus(
                "OBS Agent 연결 성공\n" +
                $"Endpoint: {endpoint}\n" +
                "OBS WebSocket 연결도 정상입니다." );

            FinishConnectionAttempt();
        }

        private void FinishConnectionAttempt()
        {
            SetInteractable( true );
            _connectionCoroutine = null;
        }

        private void LoadSavedSettings()
        {
            string savedEndpoint =
                PlayerPrefs.GetString(
                    EndpointPreferenceKey,
                    string.Empty);

            string savedToken =
                PlayerPrefs.GetString(
                    TokenPreferenceKey,
                    string.Empty);

            if( !string.IsNullOrWhiteSpace( savedEndpoint ) )
            {
                endpointInput.SetTextWithoutNotify(
                    savedEndpoint );
            }

            if( saveTokenToPlayerPrefs &&
                !string.IsNullOrWhiteSpace( savedToken ) )
            {
                agentTokenInput.SetTextWithoutNotify(
                    savedToken );
            }
        }

        private void SaveSettings(
            string endpoint,
            string token )
        {
            PlayerPrefs.SetString(
                EndpointPreferenceKey,
                endpoint );

            if( saveTokenToPlayerPrefs )
            {
                PlayerPrefs.SetString(
                    TokenPreferenceKey,
                    token );
            }
            else
            {
                PlayerPrefs.DeleteKey(
                    TokenPreferenceKey );
            }

            PlayerPrefs.Save();
        }

        private static string NormalizeEndpoint(
            string input )
        {
            if( string.IsNullOrWhiteSpace( input ) )
            {
                throw new InvalidOperationException(
                    "Agent Endpoint를 입력하세요." );
            }

            string normalized = input.Trim();

            if( !normalized.Contains( "://" ) )
            {
                normalized =
                    $"http://{normalized}";
            }

            if( !Uri.TryCreate(
                    normalized,
                    UriKind.Absolute,
                    out Uri uri ) )
            {
                throw new InvalidOperationException(
                    "Endpoint 형식이 올바르지 않습니다." );
            }

            if( !string.Equals(
                    uri.Scheme,
                    Uri.UriSchemeHttp,
                    StringComparison.OrdinalIgnoreCase ) &&
                !string.Equals(
                    uri.Scheme,
                    Uri.UriSchemeHttps,
                    StringComparison.OrdinalIgnoreCase ) )
            {
                throw new InvalidOperationException(
                    "Endpoint는 http 또는 https 형식이어야 합니다." );
            }

            var builder = new UriBuilder(uri);

            // 포트를 입력하지 않은 경우 7443 사용
            if( DoesInputOmitPort( normalized ) )
            {
                builder.Port = DefaultAgentPort;
            }

            builder.Path = string.Empty;
            builder.Query = string.Empty;
            builder.Fragment = string.Empty;

            return builder.Uri
                .GetLeftPart( UriPartial.Authority )
                .TrimEnd( '/' );
        }

        private static bool DoesInputOmitPort(
            string endpoint )
        {
            string authority = endpoint;

            int schemeIndex =
                authority.IndexOf(
                    "://",
                    StringComparison.Ordinal);

            if( schemeIndex >= 0 )
            {
                authority =
                    authority.Substring(
                        schemeIndex + 3 );
            }

            int slashIndex =
                authority.IndexOf('/');

            if( slashIndex >= 0 )
            {
                authority =
                    authority.Substring(
                        0,
                        slashIndex );
            }

            // 현재 구성은 IPv4 주소 또는 호스트 이름을 대상으로 한다.
            return authority.LastIndexOf( ':' ) < 0;
        }

        private static AgentApiResponse ParseApiResponse(
            string json )
        {
            if( string.IsNullOrWhiteSpace( json ) )
            {
                return null;
            }

            try
            {
                return JsonUtility
                    .FromJson<AgentApiResponse>( json );
            }
            catch( Exception )
            {
                return null;
            }
        }

        private static string BuildNetworkErrorMessage(
            string title,
            UnityWebRequest request )
        {
            return
                $"{title}\n" +
                $"HTTP: {request.responseCode}\n" +
                $"오류: {request.error}\n\n" +
                "PC IP, Windows 방화벽, Agent 실행 상태와 " +
                "iPhone의 로컬 네트워크 권한을 확인하세요.";
        }

        private void SetInteractable(
            bool interactable )
        {
            endpointInput.interactable =
                interactable;

            agentTokenInput.interactable =
                interactable;

            connectButton.interactable =
                interactable;
        }

        private void SetStatus( string message )
        {
            if( statusText != null )
            {
                statusText.text = message;
            }

            Debug.Log( $"[OBS Agent Client] {message}" );
        }

        //private void ValidateUiReferences()
        //{
        //    if( endpointInput == null )
        //    {
        //        throw new MissingReferenceException(
        //            "EndpointInput이 연결되지 않았습니다." );
        //    }

        //    if( agentTokenInput == null )
        //    {
        //        throw new MissingReferenceException(
        //            "AgentTokenInput이 연결되지 않았습니다." );
        //    }

        //    if( connectButton == null )
        //    {
        //        throw new MissingReferenceException(
        //            "ConnectButton이 연결되지 않았습니다." );
        //    }

        //    if( statusText == null )
        //    {
        //        throw new MissingReferenceException(
        //            "StatusText가 연결되지 않았습니다." );
        //    }
        //}

        //[Serializable]
        //private sealed class AgentApiResponse
        //{
        //    public bool success;
        //    public string message;
        //    public bool obsRunning;
        //    public bool launched;
        //    public string utcTime;
        //}
    }
}