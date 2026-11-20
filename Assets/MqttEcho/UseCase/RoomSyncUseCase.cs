using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MqttEcho.Domain;
using UniRx;
using UnityEngine;

namespace MqttEcho.UseCase
{
    /// <summary>
    /// 다른 사람의 리소스를 화면에 보여 주는 쪽 (Presentation이 구현하는 출력 포트).
    /// 메서드 이름을 Update로 지으면 MonoBehaviour의 Update와 겹치므로 Spawn/Move/Despawn을 쓴다.
    /// </summary>
    public interface IRemoteResourcePresenter
    {
        void Spawn(string id, ResourcePose pose);
        void Move(string id, ResourcePose pose);
        void Despawn(string id);
        void Clear();
    }

    /// <summary>
    /// 방 공유 유스케이스 (오케스트레이션).
    /// 연결될 때마다: 방 입장 → 스냅샷으로 상대 리소스 재구성 → 내 리소스 재전송.
    /// 그 뒤로는 다른 사람의 변경을 화면에 반영한다.
    /// </summary>
    public sealed class RoomSyncUseCase : IDisposable
    {
        readonly IRoomGateway _room;
        readonly ResourceSyncService _resources;
        readonly IRemoteResourcePresenter _presenter;
        readonly CompositeDisposable _disposables = new CompositeDisposable();
        readonly CancellationTokenSource _cts = new CancellationTokenSource();
        readonly Subject<int> _onJoined = new Subject<int>();
        string _roomId;

        /// <summary>방 입장 완료 (값 = 이미 있던 다른 사람 리소스 수)</summary>
        public IObservable<int> OnJoined => _onJoined;

        public RoomSyncUseCase(IRoomGateway room, ResourceSyncService resources, IRemoteResourcePresenter presenter)
        {
            _room = room;
            _resources = resources;
            _presenter = presenter;
        }

        public void Start(string roomId)
        {
            _roomId = roomId;

            _room.OnRemoteChange.Subscribe(Apply).AddTo(_disposables);

            // 처음 연결될 때뿐 아니라 재접속할 때마다 다시 입장한다
            _room.ConnectionState
                .DistinctUntilChanged()
                .Where(connected => connected)
                .Subscribe(_ => JoinAsync(_cts.Token).Forget())
                .AddTo(_disposables);
        }

        async UniTaskVoid JoinAsync(CancellationToken ct)
        {
            try
            {
                var others = await _room.JoinAsync(_roomId, ct);

                // 끊겨 있던 동안의 변화는 알 수 없으므로 화면을 스냅샷 기준으로 다시 만든다
                _presenter.Clear();
                foreach (var c in others) _presenter.Spawn(c.ResourceId, c.Pose);

                // 서버는 끊긴 사람의 리소스를 지웠으므로 내 리소스를 다시 알린다
                await _resources.ResendAllAsync(ct);

                _onJoined.OnNext(others.Count);
            }
            catch (OperationCanceledException) { }
            catch (Exception e)
            {
                Debug.LogWarning($"[Room] 방 입장 실패: {e.Message} (방 공유 버전 서버가 켜져 있는지 확인)");
            }
        }

        void Apply(ResourceChange change)
        {
            switch (change.Op)
            {
                case ResourceOp.Create: _presenter.Spawn(change.ResourceId, change.Pose); break;
                case ResourceOp.Update: _presenter.Move(change.ResourceId, change.Pose); break;
                case ResourceOp.Delete: _presenter.Despawn(change.ResourceId); break;
            }
        }

        public void Dispose()
        {
            _cts.Cancel();
            _cts.Dispose();
            _disposables.Dispose();
            _onJoined.Dispose();
        }
    }
}
