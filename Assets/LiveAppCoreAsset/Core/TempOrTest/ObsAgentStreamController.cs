using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace ObsAgent.Client
{
    /// <summary>
    /// 저장된 OBS Agent 연결 정보를 사용해
    /// OBS 스트리밍을 시작하거나 중지한다.
    /// </summary>
    public sealed class ObsAgentStreamController : MonoBehaviour
    {
        private const string EndpointPreferenceKey =
            "ObsAgent.Endpoint";

        private const string TokenPreferenceKey =
            "ObsAgent.Token";

        [Header("Required UI")]
        [SerializeField] private Button startStreamButton;
        [SerializeField] private Button stopStreamButton;

        [SerializeField] private Button startRecordButton;
        [SerializeField] private Button stopRecordButton;

        [SerializeField]
        private TMP_Text streamStatusText;

        [Header("Request")]
        [SerializeField]
        private int requestTimeoutSeconds = 15;

        private Coroutine _requestCoroutine;

        private void Awake()
        {
            ValidateUiReferences();

            startStreamButton.onClick.AddListener(
                StartRecording );

            stopStreamButton.onClick.AddListener(
                StopRecording );

            SetStatus( "OBS 스트리밍 제어 준비 완료" );
        }

        private void OnDestroy()
        {
            if( startStreamButton != null )
            {
                startStreamButton.onClick.RemoveListener(
                    StartStreaming );
            }

            if( stopStreamButton != null )
            {
                stopStreamButton.onClick.RemoveListener(
                    StopStreaming );
            }
        }

        public void StartStreaming()
        {
            StartCommand(
                "/api/obs/stream/start",
                "OBS 스트리밍 시작 요청 중...",
                "OBS 스트리밍을 시작했습니다." );
        }
        public void StartRecording()
        {
            StartCommand(
                "/api/obs/record/start",
                "OBS 녹화 시작 요청 중...",
                "OBS 녹화를 시작했습니다." );
        }

        public void StopRecording()
        {
            StartCommand(
                "/api/obs/record/stop",
                "OBS 녹화 종료 요청 중...",
                "OBS 녹화를 종료했습니다." );
        }

        public void StopStreaming()
        {
            StartCommand(
                "/api/obs/stream/stop",
                "OBS 스트리밍 종료 요청 중...",
                "OBS 스트리밍을 종료했습니다." );
        }

        private void StartCommand(
            string apiPath,
            string pendingMessage,
            string successMessage )
        {
            if( _requestCoroutine != null )
            {
                SetStatus( "이전 OBS 요청이 처리 중입니다." );
                return;
            }

            _requestCoroutine =
                StartCoroutine(
                    SendCommand(
                        apiPath,
                        pendingMessage,
                        successMessage ) );
        }

        private IEnumerator SendCommand(
            string apiPath,
            string pendingMessage,
            string successMessage )
        {
            SetButtonsInteractable( false );

            string endpoint =
                PlayerPrefs.GetString(
                    EndpointPreferenceKey,
                    string.Empty);

            string token =
                PlayerPrefs.GetString(
                    TokenPreferenceKey,
                    string.Empty);

            if( string.IsNullOrWhiteSpace( endpoint ) )
            {
                SetStatus(
                    "저장된 OBS Agent Endpoint가 없습니다.\n" +
                    "먼저 Agent 연결 확인을 실행하세요." );

                FinishRequest();
                yield break;
            }

            if( string.IsNullOrWhiteSpace( token ) )
            {
                SetStatus(
                    "저장된 Agent Token이 없습니다.\n" +
                    "먼저 Agent 연결 확인을 실행하세요." );

                FinishRequest();
                yield break;
            }

            endpoint = endpoint.TrimEnd( '/' );

            string requestUrl =
                $"{endpoint}{apiPath}";

            SetStatus(
                $"{pendingMessage}\n{requestUrl}" );

            using( var request =
                   new UnityWebRequest(
                       requestUrl,
                       UnityWebRequest.kHttpVerbPOST ) )
            {
                request.uploadHandler =
                    new UploadHandlerRaw(
                        Array.Empty<byte>() );

                request.downloadHandler =
                    new DownloadHandlerBuffer();

                request.SetRequestHeader(
                    "Authorization",
                    $"Bearer {token}" );

                request.SetRequestHeader(
                    "Content-Type",
                    "application/json" );

                request.timeout =
                    Mathf.Max(
                        1,
                        requestTimeoutSeconds );

                UnityWebRequestAsyncOperation operation;

                try
                {
                    operation =
                        request.SendWebRequest();
                }
                catch( InvalidOperationException exception )
                {
                    SetStatus(
                        "HTTP 요청을 시작하지 못했습니다.\n" +
                        exception.Message );

                    FinishRequest();
                    yield break;
                }

                yield return operation;

                AgentApiResponse response =
                    ParseResponse(
                        request.downloadHandler.text);

                if( request.responseCode == 401 )
                {
                    SetStatus(
                        "Agent Token이 올바르지 않습니다." );

                    FinishRequest();
                    yield break;
                }

                if( request.result !=
                    UnityWebRequest.Result.Success )
                {
                    string detail =
                        response != null &&
                        !string.IsNullOrWhiteSpace(
                            response.message)
                            ? response.message
                            : request.error;

                    SetStatus(
                        "OBS 스트리밍 명령 실패\n" +
                        $"HTTP: {request.responseCode}\n" +
                        $"오류: {detail}" );

                    FinishRequest();
                    yield break;
                }

                if( response == null )
                {
                    SetStatus(
                        "Agent가 올바르지 않은 응답을 반환했습니다." );

                    FinishRequest();
                    yield break;
                }

                if( !response.success )
                {
                    SetStatus(
                        "OBS 명령 실패\n" +
                        response.message );

                    FinishRequest();
                    yield break;
                }

                SetStatus(
                    $"{successMessage}\n" +
                    $"Agent: {endpoint}" );
            }

            FinishRequest();
        }

        private static AgentApiResponse ParseResponse(
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

        private void FinishRequest()
        {
            SetButtonsInteractable( true );
            _requestCoroutine = null;
        }

        private void SetButtonsInteractable(
            bool interactable )
        {
            startStreamButton.interactable =
                interactable;

            stopStreamButton.interactable =
                interactable;
        }

        private void SetStatus( string message )
        {
            streamStatusText.text = message;

            Debug.Log(
                $"[OBS Stream Control] {message}" );
        }

        private void ValidateUiReferences()
        {
            if( startStreamButton == null )
            {
                throw new MissingReferenceException(
                    "StartStreamButton이 연결되지 않았습니다." );
            }

            if( stopStreamButton == null )
            {
                throw new MissingReferenceException(
                    "StopStreamButton이 연결되지 않았습니다." );
            }

            if( streamStatusText == null )
            {
                throw new MissingReferenceException(
                    "StreamStatusText가 연결되지 않았습니다." );
            }
        }

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