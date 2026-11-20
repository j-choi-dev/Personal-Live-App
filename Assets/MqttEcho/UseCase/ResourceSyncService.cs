using System;
using System.Collections.Generic;
using System.Threading;
using Cysharp.Threading.Tasks;
using MqttEcho.Domain;

// 주의: 네임스페이스를 ~.Application으로 지으면 UnityEngine.Application과 충돌하므로 UseCase로 둔다.
namespace MqttEcho.UseCase
{
    /// <summary>
    /// 리소스의 생성·변경·삭제를 "보낼 메시지"로 바꾸는 유스케이스.
    /// 변화가 없는 transform은 걸러서 불필요한 전송을 줄인다.
    /// </summary>
    public sealed class ResourceSyncService
    {
        readonly IResourceSyncGateway _gateway;
        readonly Dictionary<string, ResourcePose> _lastSent = new Dictionary<string, ResourcePose>();
        int _seq;

        public ResourceSyncService(IResourceSyncGateway gateway) => _gateway = gateway;

        public IObservable<ResourceEcho> OnEcho => _gateway.OnEcho;

        public UniTask NotifyCreatedAsync(string id, ResourcePose pose, CancellationToken ct)
        {
            _lastSent[id] = pose;
            return SendAsync(id, ResourceOp.Create, pose, ct);
        }

        public UniTask NotifyMovedAsync(string id, ResourcePose pose, CancellationToken ct)
        {
            if (!_lastSent.TryGetValue(id, out var last)) return UniTask.CompletedTask; // 생성 전이면 무시
            if (last.ApproximatelyEquals(pose)) return UniTask.CompletedTask;        // 변화 없음
            _lastSent[id] = pose;
            return SendAsync(id, ResourceOp.Update, pose, ct);
        }

        public UniTask NotifyDeletedAsync(string id, CancellationToken ct)
        {
            if (!_lastSent.Remove(id)) return UniTask.CompletedTask;
            return SendAsync(id, ResourceOp.Delete, default, ct);
        }

        /// <summary>
        /// 내가 가진 리소스 전체를 create로 다시 보낸다.
        /// 서버는 연결이 끊긴 사람의 리소스를 지우므로, 방에 (재)입장할 때마다 호출해서 복구한다.
        /// </summary>
        public async UniTask ResendAllAsync(CancellationToken ct)
        {
            // 보내는 도중 목록이 바뀌어도 안전하도록 복사본으로 돈다
            var mine = new List<KeyValuePair<string, ResourcePose>>(_lastSent);
            foreach (var kv in mine)
                await SendAsync(kv.Key, ResourceOp.Create, kv.Value, ct);
        }

        UniTask SendAsync(string id, ResourceOp op, ResourcePose pose, CancellationToken ct)
        {
            var change = new ResourceChange(id, op, pose, ++_seq, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
            return _gateway.SendAsync(change, ct);
        }
    }
}
