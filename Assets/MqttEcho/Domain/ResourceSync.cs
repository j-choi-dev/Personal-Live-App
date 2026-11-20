using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using UnityEngine;

namespace MqttEcho.Domain
{
    public enum ResourceOp { Create, Update, Delete }

    /// <summary>리소스의 transform 스냅샷 (월드 위치·회전, 로컬 스케일)</summary>
    public readonly struct ResourcePose
    {
        public readonly Vector3 Position;
        public readonly Quaternion Rotation;
        public readonly Vector3 Scale;

        public ResourcePose(Vector3 position, Quaternion rotation, Vector3 scale)
        {
            Position = position;
            Rotation = rotation;
            Scale = scale;
        }

        public static ResourcePose From(Transform t) => new ResourcePose(t.position, t.rotation, t.localScale);

        /// <summary>미세한 떨림까지 보내지 않도록 허용 오차로 비교</summary>
        public bool ApproximatelyEquals(in ResourcePose other,
            float positionEps = 0.001f, float rotationEpsDeg = 0.1f, float scaleEps = 0.001f)
            => (Position - other.Position).sqrMagnitude <= positionEps * positionEps
            && Quaternion.Angle(Rotation, other.Rotation) <= rotationEpsDeg
            && (Scale - other.Scale).sqrMagnitude <= scaleEps * scaleEps;
    }

    /// <summary>서버로 보낼 변경 1건</summary>
    public readonly struct ResourceChange
    {
        public readonly string ResourceId;
        public readonly ResourceOp Op;
        public readonly ResourcePose Pose;
        public readonly int Seq;          // 보낸 순서 번호
        public readonly long SentAtMs;    // 클라이언트 시각 (RTT 계산용)

        public ResourceChange(string resourceId, ResourceOp op, ResourcePose pose, int seq, long sentAtMs)
        {
            ResourceId = resourceId;
            Op = op;
            Pose = pose;
            Seq = seq;
            SentAtMs = sentAtMs;
        }
    }

    /// <summary>서버가 돌려준 echo 1건</summary>
    public readonly struct ResourceEcho
    {
        public readonly ResourceChange Change;
        public readonly long ServerAtMs;
        public readonly long ReceivedAtMs;

        public ResourceEcho(ResourceChange change, long serverAtMs, long receivedAtMs)
        {
            Change = change;
            ServerAtMs = serverAtMs;
            ReceivedAtMs = receivedAtMs;
        }

        /// <summary>같은 클라이언트 시계로 잰 왕복 시간</summary>
        public long RoundTripMs => ReceivedAtMs - Change.SentAtMs;
    }

    /// <summary>
    /// 동기화 통로 추상화. 도메인/유스케이스는 MQTT인지 무엇인지 모른다.
    /// 나중에 UDP 등으로 바꿔도 이 인터페이스 구현만 교체하면 된다.
    /// </summary>
    public interface IResourceSyncGateway
    {
        IObservable<ResourceEcho> OnEcho { get; }
        UniTask SendAsync(ResourceChange change, CancellationToken ct);
    }

    /// <summary>
    /// 방 참여와 다른 사람의 변경 수신을 위한 통로.
    /// 이것도 MQTT인지 모르는 추상화다.
    /// </summary>
    public interface IRoomGateway
    {
        /// <summary>연결 상태 (구독하면 현재 값부터 받음)</summary>
        IObservable<bool> ConnectionState { get; }

        /// <summary>다른 사람이 만든 변경</summary>
        IObservable<ResourceChange> OnRemoteChange { get; }

        /// <summary>방에 입장하고, 방에 이미 있던 다른 사람의 리소스 목록을 받는다</summary>
        UniTask<IReadOnlyList<ResourceChange>> JoinAsync(string roomId, CancellationToken ct);
    }
}
