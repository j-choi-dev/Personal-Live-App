using System;
using System.Collections.Generic;
using UnityEngine;

namespace MqttEcho.Presentation
{
    /// <summary>
    /// 리소스 키 ↔ 프리팹 목록. 내가 생성할 때와 상대 리소스를 그릴 때 같은 목록을 쓴다.
    /// 모든 기기가 같은 키를 같은 프리팹으로 해석해야 하므로, 이 에셋 하나를 공유하는 게 핵심.
    /// (나중에 S3 에셋번들로 바꾸면 이 역할을 "키로 번들을 받아 오는 로더"가 대신한다)
    /// </summary>
    [CreateAssetMenu(menuName = "MqttEcho/Resource Catalog", fileName = "ResourceCatalog")]
    public sealed class ResourceCatalog : ScriptableObject
    {
        [Serializable]
        public struct Entry
        {
            [Tooltip("네트워크로 오가는 키. 모든 기기에서 같아야 함 (영문 소문자 권장)")]
            public string key;
            public GameObject prefab;
        }

        [SerializeField] List<Entry> entries = new List<Entry>();

        public IReadOnlyList<Entry> Entries => entries;

        public bool TryGet(string key, out GameObject prefab)
        {
            prefab = null;
            if (string.IsNullOrEmpty(key)) return false;

            foreach (var e in entries)
            {
                if (e.key == key && e.prefab != null)
                {
                    prefab = e.prefab;
                    return true;
                }
            }
            return false;
        }
    }
}
