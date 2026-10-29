using LiveAppCore;
using LiveAppUI.Domain;
using LiveAppUI.Presenter;
using System;
using System.Collections.Generic;
using UniRx;
using UnityEngine;

namespace LiveAppUI.View
{
    public class TransformInspectorView : MonoBehaviour, ITransformInspectorView
    {
        [Serializable]
        private class SliderEntry
        {
            public InspectorTransformProperty Property;
            public InspectorTransformAxis Axis;
            public ObservableSlider Slider;
            public GameObject Group; //PositionVector2View 등 그룹 프리팹 인스턴스
        }

        [SerializeField] private List<SliderEntry> _entries = new List<SliderEntry>();
        private readonly Dictionary<(InspectorTransformProperty, InspectorTransformAxis), SliderEntry> _table =
            new Dictionary<(InspectorTransformProperty, InspectorTransformAxis), SliderEntry>();
        private Subject<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)>   _onValueChanged =
            new Subject<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)>();
        public IObservable<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)> OnValueChanged => _onValueChanged;

        private void Awake()
        {
            foreach(SliderEntry entry in _entries)
            {
                if(entry.Slider == null)
                {
                    Debug.LogWarning($"[TransformInspectorView] 슬라이더가 비어 있습니다. {entry.Property}.{entry.Axis}", this);
                    continue;
                }

                var key = (entry.Property, entry.Axis);
                if(_table.ContainsKey(key))
                {
                    Debug.LogWarning($"[TransformInspectorView] 중복 등록입니다. {entry.Property}.{entry.Axis}", this);
                    continue;
                }

                _table.Add(key, entry);

                //구독마다 자기(속성, 축)를 붙여서 하나의 스트림으로 발행
                InspectorTransformProperty prop = entry.Property;
                InspectorTransformAxis axis = entry.Axis;

                entry.Slider.OnValueChanged
                    .Subscribe(value => _onValueChanged.OnNext((prop, axis, value)))
                    .AddTo(this);
            }
            //시작은 대상 없음 상태
            ShowEmpty();
        }

        //------------------------------------implement---------------------------------
        public void ShowChannel(InspectorTransformProperty prop, InspectorTransformAxis axis, ValueRange range, float value)
        {
            if(_table.TryGetValue( ( prop, axis ), out SliderEntry entry ) == false )
            {
                return;
            }

            if (entry.Group != null && entry.Group.activeSelf == false)
            {
                //활성화하면 미뤄졌던 slider들의 awake 실행
                //활성화 후에 그룹 전체를 비워야 awake의 초기화로 인한 값이 덮어씌워지지 않음
                entry.Group.SetActive(true);
                ClearGroup(entry.Group);
            }

            entry.Slider.Interactable = true;
            //범위 먼저 설정 후 값을 설정해야 clamp된 값이 문제 없음
            entry.Slider.SetRangeWithoutNotify(range.min, range.max);
            entry.Slider.SetValueWithoutNotify(value);

        }

        public void ShowEmpty()
        {
            foreach(SliderEntry entry in _table.Values)
            {
                ClearSlider(entry);

                if(entry.Group != null)
                {
                    entry.Group.SetActive(false);
                }
            }
        }
        //------------------------------------implement---------------------------------

        private void ClearGroup(GameObject group)
        {
            foreach(SliderEntry entry in _table.Values)
            {
                if(entry.Group == group)
                {
                    ClearSlider(entry);
                }
            }
        }

        private void ClearSlider(SliderEntry entry)
        {
            entry.Slider.SetIndeterminateValue();
            entry.Slider.Interactable = false;
        }

        private void OnDestroy()
        {
            _onValueChanged.OnCompleted();
            _onValueChanged.Dispose();
        }

    }

}
