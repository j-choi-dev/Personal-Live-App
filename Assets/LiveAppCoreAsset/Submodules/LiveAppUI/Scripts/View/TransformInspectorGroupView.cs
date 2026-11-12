using System;
using System.Collections.Generic;
using System.Text;
using Amazon.S3.Model;
using LiveAppCore;
using LiveAppUI.Domain;
using LiveAppUI.Presenter;
using UniRx;
using Unity.VisualScripting;
using UnityEngine;

namespace LiveAppUI.View
{
    /// <summary>
    /// Inspector 슬라이더 그룹
    /// 자식 슬라이더들만 연결해서 UI반영
    /// </summary>
    /// <remarks>PositionVector2View처럼 슬라이더를 갖고 있는 프리팹 루트를 모아놓는 view
    /// 그룹은 꺼진 상태로 시작할 수 있어 Awake대신 부모(<see cref="TransformInspectorView"/>)가 Initialize 호출 </remarks>
    public class TransformInspectorGroupView : MonoBehaviour
    {
        [Serializable]
        private class SliderEntry
        {
            public InspectorTransformProperty Property;
            public InspectorTransformAxis Axis;
            public ObservableSlider Slider;
        }

        /// <summary>이 차이 미만이면 같은 값으로 판단하여 갱신하지 않음</summary>
        private const float _valueSyncTolerance = 0.001f;
        private bool _isInitialized = false;
        [SerializeField] private List<SliderEntry> _entries = new List<SliderEntry>();

        private readonly Dictionary<(InspectorTransformProperty, InspectorTransformAxis), ObservableSlider> _table
            = new Dictionary<(InspectorTransformProperty, InspectorTransformAxis), ObservableSlider>();

        private readonly Subject<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)> _onValueChanged 
            = new Subject<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)>();

        /// <summary>슬라이더 조작으로 값 변경 시 발행</summary>
        public IObservable<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)> OnValueChanged => _onValueChanged;


        /// <summary>
        /// 해당 그룹이 담당하는 (속성, 축) 목록. Initialize 이후에 유효
        /// </summary>
        public IReadOnlyCollection<(InspectorTransformProperty, InspectorTransformAxis)> Keys => _table.Keys;




        public void Initialize()
        {
            if(_isInitialized)
            {
                return;
            }

            _isInitialized = true;

            foreach(SliderEntry entry in _entries)
            {
                if(entry.Slider == null)
                {
                    Debug.LogWarning($"[TransformInspectorGroupView] 슬라이더가 비어 있습니다. {entry.Property}.{entry.Axis}", this);
                    continue;
                }

                var key = (entry.Property, entry.Axis);
                if (_table.ContainsKey(key))
                {
                    Debug.LogWarning($"[TransformInspectorGroupView] 중복 등록입니다. {entry.Property}.{entry.Axis}", this);
                    continue;
                }

                _table.Add(key, entry.Slider);

                InspectorTransformProperty prop = entry.Property;
                InspectorTransformAxis axis = entry.Axis;

                //구독마다 자기(속성, 축)를 붙여서 하나의 스트림으로 발행
                entry.Slider.OnValueChanged
                    .Subscribe(value => _onValueChanged.OnNext((prop, axis, value)))
                    .AddTo(this);
            }
        }

        public void Show(InspectorTransformProperty prop, InspectorTransformAxis axis, ValueRange range, float value)
        {
            if(_table.TryGetValue( (prop,axis), out ObservableSlider slider) == false )
            {
                return;
            }

            if(gameObject.activeSelf == false)
            {
                //활성화하면 미뤄졌던 slider들의 awake 실행
                //활성화 후에 그룹 전체를 비워야 awake의 초기화로 인한 값이 덮어씌워지지 않음
                gameObject.SetActive(true);
                ClearAll();
            }

            slider.Interactable = true;

            //범위 먼저 설정 후 값을 설정해야 clamp된 값이 문제 없음
            slider.SetRangeWithoutNotify(range.min, range.max);
            slider.SetValueWithoutNotify(value);
        }

        public void Hide()
        {
            ClearAll();
            gameObject.SetActive(false);
        }

        public void UpdateValue(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)
        {
            if(_table.TryGetValue((prop, axis), out ObservableSlider slider) == false)
            {
                return;
            }

            //표시 중이 아닌 슬라이더는 갱신하지 않음
            if(slider.Interactable == false)
            {
                return;
            }

            if(Mathf.Abs(slider.Value - value) < _valueSyncTolerance)
            {
                return;
            }

            slider.SetValueWithoutNotify(value);
        }

        private void ClearAll()
        {
            foreach(ObservableSlider slider in _table.Values)
            {
                slider.SetIndeterminateValue();
                slider.Interactable = false;
            }
        }

        private void OnDestroy()
        {
            _onValueChanged.OnCompleted();
            _onValueChanged.Dispose();
        }

    }
}
