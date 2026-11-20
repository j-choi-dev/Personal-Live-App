using System.Collections.Generic;
using MqttEcho.Domain;
using MqttEcho.UseCase;
using UnityEngine;

namespace MqttEcho.Presentation
{
    /// <summary>
    /// 다른 사람의 리소스를 씬에 표시한다.
    /// - 받은 키(assetKey)로 카탈로그에서 프리팹을 찾아 생성, 없으면 주황 큐브
    /// - 받은 위치로 순간이동하지 않고 매 프레임 부드럽게 따라가게 한다 (간단한 보간)
    /// </summary>
    public sealed class RemoteResourcePresenter : MonoBehaviour, IRemoteResourcePresenter
    {
        [Tooltip("비워 두면 Bootstrap이 자기 카탈로그를 넘겨줌")]
        [SerializeField] ResourceCatalog catalog;
        [Tooltip("카탈로그에 없는 키일 때 쓰는 큐브 색")]
        [SerializeField] Color fallbackColor = new Color(1f, 0.55f, 0.2f);
        [Tooltip("클수록 목표 위치를 빨리 따라감")]
        [SerializeField] float followSharpness = 15f;

        readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();

        public int Count => _entries.Count;

        sealed class Entry
        {
            public Transform Transform;
            public ResourcePose Target;
        }

        /// <summary>조립 시점에 카탈로그를 넘겨받는다 (Inspector에 따로 넣지 않아도 되게)</summary>
        public void Initialize(ResourceCatalog sharedCatalog)
        {
            if (catalog == null) catalog = sharedCatalog;
        }

        public void Spawn(string id, string assetKey, ResourcePose pose)
        {
            if (_entries.TryGetValue(id, out var existing))
            {
                existing.Target = pose; // 이미 있으면 위치만 갱신
                return;
            }

            var go = catalog != null && catalog.TryGet(assetKey, out var prefab)
                ? Instantiate(prefab)
                : CreateFallback();

            // 상대 리소스가 내 것처럼 다시 전송되지 않도록, 프리팹에 동기화 컴포넌트가 있으면 제거
            if (go.TryGetComponent(out NetworkedResource networked)) Destroy(networked);

            go.name = $"Remote_{assetKey}_{id.Substring(0, Mathf.Min(6, id.Length))}";
            go.transform.SetPositionAndRotation(pose.Position, pose.Rotation);
            go.transform.localScale = pose.Scale;
            _entries[id] = new Entry { Transform = go.transform, Target = pose };
        }

        public void Move(string id, ResourcePose pose)
        {
            if (_entries.TryGetValue(id, out var entry)) entry.Target = pose;
        }

        public void Despawn(string id)
        {
            if (!_entries.TryGetValue(id, out var entry)) return;
            if (entry.Transform != null) Destroy(entry.Transform.gameObject);
            _entries.Remove(id);
        }

        public void Clear()
        {
            foreach (var entry in _entries.Values)
                if (entry.Transform != null) Destroy(entry.Transform.gameObject);
            _entries.Clear();
        }

        void Update()
        {
            // 프레임레이트와 무관하게 비슷한 속도로 따라가도록 지수 보간
            var t = 1f - Mathf.Exp(-followSharpness * Time.deltaTime);
            foreach (var entry in _entries.Values)
            {
                if (entry.Transform == null) continue;
                entry.Transform.position = Vector3.Lerp(entry.Transform.position, entry.Target.Position, t);
                entry.Transform.rotation = Quaternion.Slerp(entry.Transform.rotation, entry.Target.Rotation, t);
                entry.Transform.localScale = Vector3.Lerp(entry.Transform.localScale, entry.Target.Scale, t);
            }
        }

        void OnDestroy() => Clear();

        GameObject CreateFallback()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (go.TryGetComponent(out Renderer r)) r.material.color = fallbackColor;
            return go;
        }
    }
}
