using System;
using System.Collections.Generic;
using MqttEcho.Domain;
using MqttEcho.Infrastructure;
using MqttEcho.UseCase;
using UniRx;
using UnityEngine;

namespace MqttEcho.Presentation
{
    /// <summary>
    /// 조립 지점(Composition Root) + 테스트용 IMGUI.
    /// 객체를 만들고 연결만 하고, 흐름(입장 → 스냅샷 → 재전송)은 RoomSyncUseCase가 맡는다.
    /// 실제 앱에서는 Google 로그인 성공 후 StartSession(googleUserId)를 호출한다.
    /// (클래스 이름은 씬에 붙어 있는 컴포넌트가 끊기지 않도록 그대로 유지)
    /// </summary>
    public sealed class MqttEchoBootstrap : MonoBehaviour
    {
        [Header("Broker")]
        [Tooltip("에디터에서 접속할 주소 (서버와 같은 PC면 127.0.0.1)")]
        [SerializeField] string host = "127.0.0.1";
        [Tooltip("실기기(iPhone 등)에서 접속할 주소 = 서버 PC의 LAN IP (cmd에서 ipconfig로 확인)")]
        [SerializeField] string deviceHost = "192.168.0.10";
        [SerializeField] int port = 1883;
        [SerializeField] string topicRoot = "demo";

        [Header("Room")]
        [SerializeField] string roomId = "room-1";
        [Tooltip("비워 두면 같은 오브젝트에 자동 추가")]
        [SerializeField] RemoteResourcePresenter remotePresenter;

        [Header("Test")]
        [Tooltip("에디터에서 로그인 없이 바로 시작")]
        [SerializeField] bool autoStartInEditor = true;
        [SerializeField] string editorTestUserId = "editor-user";
        [Tooltip("실기기 빌드에서도 로그인 없이 바로 시작 (테스트 빌드용, 실제 앱에서는 끄고 로그인 후 StartSession 호출)")]
        [SerializeField] bool autoStartOnDevice = true;
        [SerializeField] string deviceTestUserId = "iphone-user";
        [Tooltip("비워 두면 기본 큐브 생성")]
        [SerializeField] GameObject resourcePrefab;

        MqttConnection _connection;
        MqttRoomSyncGateway _gateway;
        ResourceSyncService _sync;
        RoomSyncUseCase _room;
        readonly List<NetworkedResource> _spawned = new List<NetworkedResource>();
        readonly Queue<string> _logs = new Queue<string>();
        string _clientId = "-";
        long _lastRttMs = -1;
        bool _autoMove;

        /// <summary>지금 실행 환경에서 접속할 서버 주소</summary>
        string ResolvedHost => Application.isEditor ? host : deviceHost;

        void Start()
        {
            if (Application.isEditor)
            {
                if (autoStartInEditor) StartSession(editorTestUserId);
            }
            else if (autoStartOnDevice)
            {
                StartSession(deviceTestUserId);
            }
        }

        /// <summary>로그인 성공 콜백에서 호출</summary>
        public void StartSession(string userId)
        {
            if (_connection != null) return;

            // 같은 계정으로 여러 기기가 동시에 붙을 수 있으므로 기기·실행마다 고유한 clientId
            _clientId = $"{Sanitize(userId)}-{DeviceTag()}";

            if (remotePresenter == null && !TryGetComponent(out remotePresenter))
                remotePresenter = gameObject.AddComponent<RemoteResourcePresenter>();

            // ---- 조립: 바깥(Infra)에서 안쪽(UseCase)으로 구현을 끼워 넣는다 ----
            _connection = new MqttConnection(new MqttConnectionSettings { Host = ResolvedHost, Port = port, ClientId = _clientId });
            _gateway = new MqttRoomSyncGateway(_connection, topicRoot, _clientId);
            _sync = new ResourceSyncService(_gateway);
            _room = new RoomSyncUseCase(_gateway, _sync, remotePresenter);

            // ---- 화면 로그 ----
            _connection.IsConnected.Skip(1)
                .Subscribe(connected => Log(connected ? "연결됨" : "연결 끊김"))
                .AddTo(this);

            _room.OnJoined
                .Subscribe(n => Log($"{roomId} 입장 (상대 리소스 {n}개)"))
                .AddTo(this);

            _gateway.OnRemoteChange
                .Where(c => c.Op != ResourceOp.Update)
                .Subscribe(c => Log($"상대 {c.Op} {Short(c.ResourceId)}"))
                .AddTo(this);

            _sync.OnEcho
                .Do(e => _lastRttMs = e.RoundTripMs)
                .Where(e => e.Change.Op != ResourceOp.Update) // update echo는 너무 많아서 RTT만 갱신
                .Subscribe(e => Log($"echo {e.Change.Op} {Short(e.Change.ResourceId)} RTT={e.RoundTripMs}ms"))
                .AddTo(this);

            // 씬에 미리 배치된 리소스 등록 (입장 후 재전송 단계에서 서버에 전달됨)
            foreach (var r in FindObjectsByType<NetworkedResource>(FindObjectsSortMode.None)) r.Bind(_sync);

            _room.Start(roomId);
            _connection.Start();
        }

        void Update()
        {
            if (!_autoMove || _spawned.Count == 0) return;
            var target = _spawned[_spawned.Count - 1];
            if (target == null) return;

            var t = Time.time;
            target.transform.position = new Vector3(Mathf.Cos(t) * 2f, 0f, 5f + Mathf.Sin(t) * 2f);
            target.transform.Rotate(0f, 90f * Time.deltaTime, 0f);
        }

        void Spawn()
        {
            var go = resourcePrefab != null ? Instantiate(resourcePrefab) : GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.transform.position = UnityEngine.Random.insideUnitSphere * 2f + Vector3.forward * 5f;

            if (!go.TryGetComponent(out NetworkedResource resource)) resource = go.AddComponent<NetworkedResource>();
            resource.Bind(_sync);
            _spawned.Add(resource);
            Log($"create 전송 {Short(resource.ResourceId)}");
        }

        void DeleteLast()
        {
            if (_spawned.Count == 0) return;
            var resource = _spawned[_spawned.Count - 1];
            _spawned.RemoveAt(_spawned.Count - 1);
            if (resource == null) return;

            Log($"delete 전송 {Short(resource.ResourceId)}");
            Destroy(resource.gameObject); // NetworkedResource.OnDestroy에서 delete 전송
        }

        void OnGUI()
        {
            var scale = Mathf.Max(1f, Screen.height / 720f);
            GUI.matrix = Matrix4x4.Scale(new Vector3(scale, scale, 1f));

            GUILayout.BeginArea(new Rect(10, 10, 460, 720));
            var connected = _connection != null && _connection.IsConnected.Value;
            GUILayout.Label($"상태: {(connected ? "연결됨" : "미연결")}   {ResolvedHost}:{port}");
            GUILayout.Label($"clientId: {_clientId}");
            GUILayout.Label($"방: {roomId}   내 리소스: {_spawned.Count}   상대 리소스: {(remotePresenter != null ? remotePresenter.Count : 0)}   RTT: {(_lastRttMs < 0 ? "-" : _lastRttMs + "ms")}");

            GUI.enabled = _sync != null;
            if (GUILayout.Button("리소스 생성 (create)")) Spawn();
            if (GUILayout.Button("마지막 리소스 삭제 (delete)")) DeleteLast();
            _autoMove = GUILayout.Toggle(_autoMove, " 마지막 리소스 자동 이동 (update)");
            GUI.enabled = true;

            GUILayout.Space(8);
            foreach (var line in _logs) GUILayout.Label(line);
            GUILayout.EndArea();
        }

        void OnDestroy()
        {
            _room?.Dispose();
            _gateway?.Dispose();
            _connection?.Dispose();
        }

        void Log(string message)
        {
            Debug.Log($"[RoomTest] {message}");
            _logs.Enqueue($"{DateTime.Now:HH:mm:ss} {message}");
            while (_logs.Count > 12) _logs.Dequeue();
        }

        static string Short(string id) => string.IsNullOrEmpty(id) ? "-" : id.Substring(0, Math.Min(6, id.Length));

        // 토픽에 들어가므로 MQTT 예약 문자(/ + #)는 제거
        static string Sanitize(string s) => (s ?? "anon").Replace("/", "_").Replace("+", "_").Replace("#", "_");

        static string DeviceTag()
        {
            var device = Sanitize(SystemInfo.deviceUniqueIdentifier);
            var shortDevice = device.Substring(0, Math.Min(6, device.Length));
            return $"{shortDevice}-{Guid.NewGuid().ToString("N").Substring(0, 4)}";
        }
    }
}
