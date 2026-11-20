using System.Collections.Generic;
using MqttEcho.Domain;
using MqttEcho.UseCase;
using UnityEngine;

namespace MqttEcho.Presentation
{
    /// <summary>
    /// 다른 사람의 리소스를 씬에 표시한다.
    /// 받은 위치로 순간이동하지 않고 매 프레임 부드럽게 따라가게 한다 (간단한 보간).
    /// 이 오브젝트들에는 NetworkedResource를 붙이지 않는다 (붙이면 내 것처럼 다시 보내게 됨).
    /// </summary>
    public sealed class RemoteResourcePresenter : MonoBehaviour, IRemoteResourcePresenter
    {
        [Tooltip("비워 두면 색이 다른 큐브")]
        [SerializeField] GameObject remotePrefab;
        [SerializeField] Color remoteColor = new Color(1f, 0.55f, 0.2f);
        [Tooltip("클수록 목표 위치를 빨리 따라감")]
        [SerializeField] float followSharpness = 15f;

        readonly Dictionary<string, Entry> _entries = new Dictionary<string, Entry>();

        public int Count => _entries.Count;

        sealed class Entry
        {
            public Transform Transform;
            public ResourcePose Target;
        }

        public void Spawn(string id, ResourcePose pose)
        {
            if (_entries.TryGetValue(id, out var existing))
            {
                existing.Target = pose; // 이미 있으면 위치만 갱신
                return;
            }

            var go = remotePrefab != null ? Instantiate(remotePrefab) : CreateDefault();
            go.name = $"Remote_{id.Substring(0, Mathf.Min(6, id.Length))}";
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

        GameObject CreateDefault()
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            if (go.TryGetComponent(out Renderer r)) r.material.color = remoteColor;
            return go;
        }
    }
}
