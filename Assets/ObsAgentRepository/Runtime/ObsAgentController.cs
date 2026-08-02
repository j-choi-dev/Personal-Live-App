#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ObsAgent
{
    public sealed class ObsAgentController : MonoBehaviour
    {
        [Header("Agent Server")]
        [SerializeField] private TMP_InputField listenPortInput;
        [SerializeField] private Toggle allowLanClientsToggle;
        [SerializeField] private Toggle autoStartServerToggle;
        [SerializeField] private TMP_InputField agentTokenInput;

        [Header("OBS Process")]
        [SerializeField] private TMP_InputField obsExecutablePathInput;
        [SerializeField] private TMP_InputField profileNameInput;
        [SerializeField] private TMP_InputField sceneCollectionInput;
        [SerializeField] private TMP_InputField defaultSceneInput;
        [SerializeField] private Toggle minimizeToTrayToggle;

        [Header("OBS WebSocket")]
        [SerializeField] private TMP_InputField obsWebSocketPortInput;
        [SerializeField] private TMP_InputField obsWebSocketPasswordInput;

        [Header("Launch Options")]
        [SerializeField] private Toggle setSceneAfterLaunchToggle;
        [SerializeField] private Toggle startRecordingAfterLaunchToggle;
        [SerializeField] private Toggle startStreamingAfterLaunchToggle;

        [Header("Main Buttons")]
        [SerializeField] private Button saveButton;
        [SerializeField] private Button generateTokenButton;
        [SerializeField] private Button startServerButton;
        [SerializeField] private Button stopServerButton;
        [SerializeField] private Button launchObsButton;
        [SerializeField] private Button testObsButton;

        [Header("OBS Command Buttons")]
        [SerializeField] private Button setSceneButton;
        [SerializeField] private Button startRecordButton;
        [SerializeField] private Button stopRecordButton;
        [SerializeField] private Button startStreamButton;
        [SerializeField] private Button stopStreamButton;
        [SerializeField] private Button quitButton;

        [Header("Status")]
        [SerializeField] private TMP_Text serverStatusText;
        [SerializeField] private TMP_Text obsStatusText;
        [SerializeField] private TMP_Text addressText;
        [SerializeField] private TMP_Text configPathText;
        [SerializeField] private TMP_Text logText;

        private readonly object _configLock = new object();
        private readonly ConcurrentQueue<string> _pendingLogs =
            new ConcurrentQueue<string>();

        private readonly StringBuilder _logBuffer =
            new StringBuilder();

        private ObsAgentConfiguration _currentConfig;
        private ObsAgentOperations _operations;
        private ObsAgentHttpServer _server;
        private CancellationTokenSource _lifetimeCancellation;

        private float _nextStatusUpdateTime;

        private void Awake()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 15;

            _lifetimeCancellation =
                new CancellationTokenSource();

            _currentConfig =
                ObsAgentConfigStore.Load();

            _operations = new ObsAgentOperations(
                GetConfigSnapshot,
                EnqueueLog );

            _server = new ObsAgentHttpServer(
                GetConfigSnapshot,
                _operations,
                EnqueueLog );

            ApplyConfigToUi( _currentConfig );
            RegisterButtonEvents();

            if( configPathText != null )
            {
                configPathText.text =
                    ObsAgentConfigStore.ConfigPath;
            }

            EnqueueLog( "OBS Agent UI가 초기화되었습니다." );
        }

        private void Start()
        {
            if( _currentConfig.autoStartServer )
            {
                StartAgentServer();
            }

            RefreshStatusUi();
        }

        private void Update()
        {
            FlushLogs();

            if( Time.unscaledTime >= _nextStatusUpdateTime )
            {
                _nextStatusUpdateTime =
                    Time.unscaledTime + 0.5f;

                RefreshStatusUi();
            }
        }

        private void RegisterButtonEvents()
        {
            saveButton.onClick.AddListener( SaveSettings );
            generateTokenButton.onClick.AddListener( GenerateToken );

            startServerButton.onClick.AddListener( StartAgentServer );
            stopServerButton.onClick.AddListener( StopAgentServer );

            launchObsButton.onClick.AddListener(
                () => RunOperation(
                    token => _operations.LaunchObsAsync( token ) ) );

            testObsButton.onClick.AddListener(
                () => RunOperation(
                    token => _operations.TestConnectionAsync( token ) ) );

            setSceneButton.onClick.AddListener(
                () =>
                {
                    ApplyUiToConfig( false );

                    string sceneName =
                        defaultSceneInput.text.Trim();

                    RunOperation(
                        token => _operations.SetSceneAsync(
                            sceneName,
                            token ) );
                } );

            startRecordButton.onClick.AddListener(
                () => RunOperation(
                    token => _operations.StartRecordAsync( token ) ) );

            stopRecordButton.onClick.AddListener(
                () => RunOperation(
                    token => _operations.StopRecordAsync( token ) ) );

            startStreamButton.onClick.AddListener(
                () => RunOperation(
                    token => _operations.StartStreamAsync( token ) ) );

            stopStreamButton.onClick.AddListener(
                () => RunOperation(
                    token => _operations.StopStreamAsync( token ) ) );

            quitButton.onClick.AddListener( QuitApplication );
        }

        private async void RunOperation(
            Func<CancellationToken, Task<AgentApiResponse>> operation )
        {
            try
            {
                ApplyUiToConfig( false );
                SetCommandButtonsInteractable( false );

                AgentApiResponse response =
                    await operation(
                        _lifetimeCancellation.Token);

                EnqueueLog(
                    response.success
                        ? $"성공: {response.message}"
                        : $"실패: {response.message}" );
            }
            catch( OperationCanceledException )
            {
                EnqueueLog( "작업이 취소되었습니다." );
            }
            catch( Exception exception )
            {
                EnqueueLog( $"작업 예외: {exception.Message}" );
            }
            finally
            {
                SetCommandButtonsInteractable( true );
                RefreshStatusUi();
            }
        }

        private void SaveSettings()
        {
            try
            {
                ApplyUiToConfig( true );
                EnqueueLog( "설정을 저장했습니다." );
                RefreshStatusUi();
            }
            catch( Exception exception )
            {
                EnqueueLog( $"설정 저장 실패: {exception.Message}" );
            }
        }

        private void StartAgentServer()
        {
            try
            {
                if( _server.IsRunning )
                {
                    EnqueueLog( "Agent 서버가 이미 실행 중입니다." );
                    return;
                }

                ApplyUiToConfig( true );
                _server.Start();
                RefreshStatusUi();
            }
            catch( Exception exception )
            {
                EnqueueLog( $"Agent 서버 시작 실패: {exception.Message}" );
            }
        }

        private void StopAgentServer()
        {
            _server.Stop();
            RefreshStatusUi();
        }

        private void GenerateToken()
        {
            byte[] bytes = new byte[32];

            using( RandomNumberGenerator random =
                   RandomNumberGenerator.Create() )
            {
                random.GetBytes( bytes );
            }

            string token = Convert
                .ToBase64String(bytes)
                .TrimEnd('=')
                .Replace('+', '-')
                .Replace('/', '_');

            agentTokenInput.SetTextWithoutNotify( token );

            EnqueueLog(
                "새 Agent Token을 생성했습니다. " +
                "iPhone 앱에도 동일한 값을 설정하세요." );
        }

        private void ApplyUiToConfig( bool saveToDisk )
        {
            var config = new ObsAgentConfiguration
            {
                listenPort = ParsePort(
                    listenPortInput.text,
                    "Agent Port"),

                allowLanClients =
                    allowLanClientsToggle.isOn,

                autoStartServer =
                    autoStartServerToggle.isOn,

                agentToken =
                    agentTokenInput.text.Trim(),

                obsExecutablePath =
                    obsExecutablePathInput.text.Trim(),

                profileName =
                    profileNameInput.text.Trim(),

                sceneCollectionName =
                    sceneCollectionInput.text.Trim(),

                defaultSceneName =
                    defaultSceneInput.text.Trim(),

                minimizeToTray =
                    minimizeToTrayToggle.isOn,

                obsWebSocketPort = ParsePort(
                    obsWebSocketPortInput.text,
                    "OBS WebSocket Port"),

                obsWebSocketPassword =
                    obsWebSocketPasswordInput.text,

                setSceneAfterLaunch =
                    setSceneAfterLaunchToggle.isOn,

                startRecordingAfterLaunch =
                    startRecordingAfterLaunchToggle.isOn,

                startStreamingAfterLaunch =
                    startStreamingAfterLaunchToggle.isOn
            };

            ValidateConfiguration( config );

            lock( _configLock )
            {
                _currentConfig = config;
            }

            if( saveToDisk )
            {
                ObsAgentConfigStore.Save( config );
            }
        }

        private void ApplyConfigToUi(
            ObsAgentConfiguration config )
        {
            listenPortInput.SetTextWithoutNotify(
                config.listenPort.ToString() );

            allowLanClientsToggle.SetIsOnWithoutNotify(
                config.allowLanClients );

            autoStartServerToggle.SetIsOnWithoutNotify(
                config.autoStartServer );

            agentTokenInput.SetTextWithoutNotify(
                config.agentToken );

            obsExecutablePathInput.SetTextWithoutNotify(
                config.obsExecutablePath );

            profileNameInput.SetTextWithoutNotify(
                config.profileName );

            sceneCollectionInput.SetTextWithoutNotify(
                config.sceneCollectionName );

            defaultSceneInput.SetTextWithoutNotify(
                config.defaultSceneName );

            minimizeToTrayToggle.SetIsOnWithoutNotify(
                config.minimizeToTray );

            obsWebSocketPortInput.SetTextWithoutNotify(
                config.obsWebSocketPort.ToString() );

            obsWebSocketPasswordInput.SetTextWithoutNotify(
                config.obsWebSocketPassword );

            setSceneAfterLaunchToggle.SetIsOnWithoutNotify(
                config.setSceneAfterLaunch );

            startRecordingAfterLaunchToggle.SetIsOnWithoutNotify(
                config.startRecordingAfterLaunch );

            startStreamingAfterLaunchToggle.SetIsOnWithoutNotify(
                config.startStreamingAfterLaunch );
        }

        private ObsAgentConfiguration GetConfigSnapshot()
        {
            lock( _configLock )
            {
                return _currentConfig.Clone();
            }
        }

        private void ValidateConfiguration(
            ObsAgentConfiguration config )
        {
            if( config.listenPort < 1024 ||
                config.listenPort > 65535 )
            {
                throw new InvalidOperationException(
                    "Agent Port는 1024~65535 범위여야 합니다." );
            }

            if( config.obsWebSocketPort < 1 ||
                config.obsWebSocketPort > 65535 )
            {
                throw new InvalidOperationException(
                    "OBS WebSocket Port가 올바르지 않습니다." );
            }

            if( string.IsNullOrWhiteSpace( config.agentToken ) ||
                config.agentToken.Length < 16 )
            {
                throw new InvalidOperationException(
                    "Agent Token은 16자 이상이어야 합니다." );
            }

            if( string.IsNullOrWhiteSpace(
                    config.obsExecutablePath ) )
            {
                throw new InvalidOperationException(
                    "OBS 실행 파일 경로를 입력하세요." );
            }
        }

        private static int ParsePort(
            string value,
            string fieldName )
        {
            if( !int.TryParse( value, out int port ) )
            {
                throw new InvalidOperationException(
                    $"{fieldName}가 숫자가 아닙니다." );
            }

            return port;
        }

        private void RefreshStatusUi()
        {
            ObsAgentConfiguration config =
                GetConfigSnapshot();

            bool serverRunning =
                _server != null &&
                _server.IsRunning;

            bool obsRunning =
                _operations != null &&
                _operations.IsObsRunning();

            serverStatusText.text =
                serverRunning
                    ? "Agent Server: RUNNING"
                    : "Agent Server: STOPPED";

            obsStatusText.text =
                obsRunning
                    ? "OBS: RUNNING"
                    : "OBS: STOPPED";

            startServerButton.interactable =
                !serverRunning;

            stopServerButton.interactable =
                serverRunning;

            addressText.text =
                BuildAddressText( config );
        }

        private static string BuildAddressText(
            ObsAgentConfiguration config )
        {
            if( !config.allowLanClients )
            {
                return $"http://127.0.0.1:{config.listenPort}";
            }

            List<string> addresses =
                GetLocalIpv4Addresses();

            if( addresses.Count == 0 )
            {
                return $"http://<PC-IP>:{config.listenPort}";
            }

            var builder = new StringBuilder();

            foreach( string address in addresses )
            {
                if( builder.Length > 0 )
                {
                    builder.AppendLine();
                }

                builder.Append( "http://" );
                builder.Append( address );
                builder.Append( ':' );
                builder.Append( config.listenPort );
            }

            return builder.ToString();
        }

        private static List<string> GetLocalIpv4Addresses()
        {
            var result = new List<string>();

            try
            {
                NetworkInterface[] interfaces =
                    NetworkInterface.GetAllNetworkInterfaces();

                foreach( NetworkInterface networkInterface
                         in interfaces )
                {
                    if( networkInterface.OperationalStatus !=
                        OperationalStatus.Up )
                    {
                        continue;
                    }

                    if( networkInterface.NetworkInterfaceType ==
                        NetworkInterfaceType.Loopback )
                    {
                        continue;
                    }

                    IPInterfaceProperties properties =
                        networkInterface.GetIPProperties();

                    foreach( UnicastIPAddressInformation address
                             in properties.UnicastAddresses )
                    {
                        if( address.Address.AddressFamily !=
                            AddressFamily.InterNetwork )
                        {
                            continue;
                        }

                        if( IPAddress.IsLoopback( address.Address ) )
                        {
                            continue;
                        }

                        string text = address.Address.ToString();

                        if( !result.Contains( text ) )
                        {
                            result.Add( text );
                        }
                    }
                }
            }
            catch
            {
                // 주소 표시 실패는 Agent 동작에 치명적이지 않다.
            }

            return result;
        }

        private void EnqueueLog( string message )
        {
            string formatted =
                $"[{DateTime.Now:HH:mm:ss}] {message}";

            _pendingLogs.Enqueue( formatted );
        }

        private void FlushLogs()
        {
            bool changed = false;

            while( _pendingLogs.TryDequeue(
                       out string message ) )
            {
                _logBuffer.AppendLine( message );
                changed = true;
            }

            if( !changed )
            {
                return;
            }

            const int maxCharacters = 16000;

            if( _logBuffer.Length > maxCharacters )
            {
                _logBuffer.Remove(
                    0,
                    _logBuffer.Length - maxCharacters );
            }

            logText.text = _logBuffer.ToString();
        }

        private void SetCommandButtonsInteractable(
            bool interactable )
        {
            launchObsButton.interactable = interactable;
            testObsButton.interactable = interactable;
            setSceneButton.interactable = interactable;
            startRecordButton.interactable = interactable;
            stopRecordButton.interactable = interactable;
            startStreamButton.interactable = interactable;
            stopStreamButton.interactable = interactable;
        }

        private void QuitApplication()
        {
            _server?.Stop();
            Application.Quit();
        }

        private void OnApplicationQuit()
        {
            try
            {
                _lifetimeCancellation?.Cancel();
                _server?.Stop();
            }
            finally
            {
                _lifetimeCancellation?.Dispose();
                _lifetimeCancellation = null;
            }
        }
    }
}

#endif