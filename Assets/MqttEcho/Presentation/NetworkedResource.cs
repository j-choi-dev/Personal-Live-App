using System;
using System.Threading;
using Cysharp.Threading.Tasks;
using MqttEcho.Domain;
using MqttEcho.UseCase;
using UniRx;
using UniRx.Triggers;
using UnityEngine;

namespace MqttEcho.Presentation
{
    /// <summary>
    /// 동기화 대상 리소스에 붙이는 컴포넌트.
    /// Bind 시점 = 생성, transform 변경 = 갱신, OnDestroy = 삭제로 서버에 알린다.
    /// </summary>
    public sealed class NetworkedResource : MonoBehaviour
    {
        /// <summary>카탈로그에 없는 기본 모양(큐브)의 키</summary>
        public const string DefaultAssetKey = "cube";

        [Tooltip("비워 두면 Bind 시 GUID로 자동 생성")]
        [SerializeField] string resourceId;

        [Tooltip("어떤 리소스인지 나타내는 키. ResourceCatalog의 key와 같아야 상대 기기에서 같은 모양으로 보임")]
        [SerializeField] string assetKey;

        [Tooltip("transform 변경을 확인해 보내는 최소 간격(초)")]
        [SerializeField] float sendInterval = 0.1f;

        ResourceSyncService _sync;

        public string ResourceId => resourceId;
        public string AssetKey => assetKey;

        /// <param name="overrideAssetKey">코드로 생성할 때 키를 지정 (비우면 Inspector 값 사용)</param>
        public void Bind(ResourceSyncService sync, string overrideAssetKey = null)
        {
            if (_sync != null) return;
            _sync = sync;
            if (string.IsNullOrEmpty(resourceId)) resourceId = Guid.NewGuid().ToString("N");
            if (!string.IsNullOrEmpty(overrideAssetKey)) assetKey = overrideAssetKey;
            if (string.IsNullOrEmpty(assetKey)) assetKey = DefaultAssetKey;

            _sync.NotifyCreatedAsync(resourceId, assetKey, ResourcePose.From(transform), destroyCancellationToken).Forget();

            // 매 프레임 pose를 읽되, sendInterval마다 "가장 최근 값"만 꺼내 보낸다.
            // (Sample은 마지막 값을 놓치지 않으므로 멈춘 최종 위치도 전송된다)
            this.UpdateAsObservable()
                .Select(_ => ResourcePose.From(transform))
                .Sample(TimeSpan.FromSeconds(sendInterval), Scheduler.MainThreadIgnoreTimeScale)
                .Subscribe(pose => _sync.NotifyMovedAsync(resourceId, pose, destroyCancellationToken).Forget())
                .AddTo(this);
        }

        void OnDestroy()
        {
            // 이 시점엔 destroyCancellationToken이 이미 취소되어 있으므로 None으로 보낸다
            _sync?.NotifyDeletedAsync(resourceId, CancellationToken.None).Forget();
        }
    }
}
