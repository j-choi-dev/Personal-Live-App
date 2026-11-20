using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using Cysharp.Threading.Tasks;
using MQTTnet.Protocol;
using MqttEcho.Domain;
using UniRx;
using UnityEngine;

namespace MqttEcho.Infrastructure
{
    /// <summary>
    /// 방 공유용 MQTT 어댑터. 포트 두 개를 함께 구현한다.
    /// - IResourceSyncGateway: 내 변경 보내기 + 내 변경이 방을 거쳐 돌아온 것(echo)
    /// - IRoomGateway: 방 입장(스냅샷) + 다른 사람의 변경
    ///
    /// 보내는 토픽: {root}/c2s/{clientId}/join, {root}/c2s/{clientId}/resource
    /// 받는 토픽:   {root}/s2c/{clientId}/snapshot, {root}/room/{roomId}/resource
    /// </summary>
    public sealed class MqttRoomSyncGateway : IResourceSyncGateway, IRoomGateway, IDisposable
    {
        static readonly TimeSpan SnapshotTimeout = TimeSpan.FromSeconds(5);

        readonly MqttConnection _connection;
        readonly string _root;
        readonly string _clientId;
        readonly string _joinTopic;
        readonly string _resourceTopic;
        readonly string _snapshotTopic;
        string _roomTopic;

        readonly Subject<ResourceEcho> _onEcho = new Subject<ResourceEcho>();
        readonly Subject<ResourceChange> _onRemoteChange = new Subject<ResourceChange>();
        readonly IDisposable _subscription;
        UniTaskCompletionSource<SnapshotDto> _pendingSnapshot;

        public IObservable<ResourceEcho> OnEcho => _onEcho;
        public IObservable<ResourceChange> OnRemoteChange => _onRemoteChange;
        public IObservable<bool> ConnectionState => _connection.IsConnected;

        public MqttRoomSyncGateway(MqttConnection connection, string topicRoot, string clientId)
        {
            _connection = connection;
            _root = topicRoot;
            _clientId = clientId;
            _joinTopic = $"{topicRoot}/c2s/{clientId}/join";
            _resourceTopic = $"{topicRoot}/c2s/{clientId}/resource";
            _snapshotTopic = $"{topicRoot}/s2c/{clientId}/snapshot";

            _subscription = _connection.OnMessage.Subscribe(HandleInbound);
        }

        public async UniTask<IReadOnlyList<ResourceChange>> JoinAsync(string roomId, CancellationToken ct)
        {
            if (!Regex.IsMatch(roomId ?? "", "^[A-Za-z0-9_-]{1,64}$"))
                throw new ArgumentException($"잘못된 roomId: {roomId}");

            _roomTopic = $"{_root}/room/{roomId}/resource";

            // 순서가 중요하다: 스냅샷과 방 메시지를 받을 구독을 먼저 끝낸 뒤 입장 요청을 보낸다
            await _connection.SubscribeAsync(_snapshotTopic, MqttQualityOfServiceLevel.AtLeastOnce);
            await _connection.SubscribeAsync(_roomTopic, MqttQualityOfServiceLevel.AtLeastOnce);

            var pending = new UniTaskCompletionSource<SnapshotDto>();
            _pendingSnapshot = pending;

            var json = JsonUtility.ToJson(new JoinDto { roomId = roomId });
            await _connection.PublishAsync(_joinTopic, Encoding.UTF8.GetBytes(json), MqttQualityOfServiceLevel.AtLeastOnce, ct);

            var snapshot = await pending.Task.Timeout(SnapshotTimeout).AttachExternalCancellation(ct);

            // 스냅샷 중 내 리소스는 빼고 다른 사람 것만 돌려준다
            var others = new List<ResourceChange>();
            if (snapshot.resources != null)
            {
                foreach (var dto in snapshot.resources)
                    if (dto.owner != _clientId && dto.TryToChange(out var change)) others.Add(change);
            }
            return others;
        }

        public UniTask SendAsync(ResourceChange change, CancellationToken ct)
        {
            var json = JsonUtility.ToJson(ResourceMessageDto.From(change));

            // 생성·삭제: 반드시 도착해야 함 → QoS 1 / 위치 갱신: 최신 값만 의미 있음 → QoS 0
            var qos = change.Op == ResourceOp.Update
                ? MqttQualityOfServiceLevel.AtMostOnce
                : MqttQualityOfServiceLevel.AtLeastOnce;

            return _connection.PublishAsync(_resourceTopic, Encoding.UTF8.GetBytes(json), qos, ct);
        }

        void HandleInbound(MqttInbound inbound)
        {
            if (inbound.Topic == _snapshotTopic) HandleSnapshot(inbound.Payload);
            else if (inbound.Topic == _roomTopic) HandleRoomMessage(inbound.Payload);
        }

        void HandleSnapshot(byte[] payload)
        {
            var dto = Parse<SnapshotDto>(payload);
            if (dto == null) return;
            _pendingSnapshot?.TrySetResult(dto);
            _pendingSnapshot = null;
        }

        void HandleRoomMessage(byte[] payload)
        {
            var dto = Parse<ResourceMessageDto>(payload);
            if (dto == null || !dto.TryToChange(out var change)) return;

            if (dto.owner == _clientId)
            {
                // 내가 보낸 변경이 방을 거쳐 돌아온 것 = echo
                _onEcho.OnNext(new ResourceEcho(change, dto.serverAt, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
            }
            else
            {
                // 다른 사람의 변경
                _onRemoteChange.OnNext(change);
            }
        }

        static T Parse<T>(byte[] payload) where T : class
        {
            try
            {
                return JsonUtility.FromJson<T>(Encoding.UTF8.GetString(payload));
            }
            catch (Exception e)
            {
                Debug.LogWarning($"[MQTT] JSON 파싱 실패: {e.Message}");
                return null;
            }
        }

        public void Dispose()
        {
            _subscription.Dispose();
            _pendingSnapshot?.TrySetCanceled();
            _onEcho.Dispose();
            _onRemoteChange.Dispose();
        }
    }

    // ---- 네트워크로 오가는 JSON 형태 (JsonUtility용 public 필드) ----

    [Serializable]
    internal sealed class JoinDto
    {
        public string roomId;
    }

    [Serializable]
    internal sealed class SnapshotDto
    {
        public string roomId;
        public List<ResourceMessageDto> resources;
    }

    [Serializable]
    internal sealed class ResourceMessageDto
    {
        public string op;       // "create" | "update" | "delete"
        public string id;
        public Vector3 pos;
        public Quaternion rot;
        public Vector3 scl;
        public int seq;
        public long sentAt;     // 클라이언트 전송 시각 (ms)
        public long serverAt;   // 서버가 방에 전달한 시각 (ms)
        public string owner;    // 서버가 붙여 주는 보낸 사람 clientId
        public string asset;    // 리소스 종류 키 (예: "candles")

        public static ResourceMessageDto From(ResourceChange c) => new ResourceMessageDto
        {
            op = c.Op.ToString().ToLowerInvariant(),
            id = c.ResourceId,
            pos = c.Pose.Position,
            rot = c.Pose.Rotation,
            scl = c.Pose.Scale,
            seq = c.Seq,
            sentAt = c.SentAtMs,
            asset = c.AssetKey,
        };

        public bool TryToChange(out ResourceChange change)
        {
            change = default;
            if (string.IsNullOrEmpty(id)) return false;
            if (!Enum.TryParse(op, true, out ResourceOp parsed)) return false;
            // 값이 빠진 메시지(테스트 앱에서 직접 보낸 JSON 등)도 안전하게 처리
            var r = rot.x == 0f && rot.y == 0f && rot.z == 0f && rot.w == 0f ? Quaternion.identity : rot;
            var s = scl == Vector3.zero ? Vector3.one : scl;
            change = new ResourceChange(id, parsed, new ResourcePose(pos, r, s), seq, sentAt, asset);
            return true;
        }
    }
}
