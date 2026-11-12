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

        [SerializeField] private List<TransformInspectorGroupView> _groups = new List<TransformInspectorGroupView>();
        private readonly Dictionary<(InspectorTransformProperty, InspectorTransformAxis), TransformInspectorGroupView> _table =
            new Dictionary<(InspectorTransformProperty, InspectorTransformAxis), TransformInspectorGroupView>();
        private Subject<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)>   _onValueChanged =
            new Subject<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)>();
        public IObservable<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)> OnValueChanged => _onValueChanged;

        private void Awake()
        {
            foreach(TransformInspectorGroupView group in _groups)
            {
                if(group == null)
                {
                    Debug.LogWarning($"[TransformInspectorView] 그룹이 비어 있습니다.", this);
                    continue;
                }

                //그룹은 꺼진 상태일 수도 있으니 group의 Awake에 맡기지 않고 여기서 초기화
                group.Initialize();

                foreach(var key in group.Keys)
                {
                    if(_table.ContainsKey(key))
                    {
                        Debug.LogWarning($"[TransformInspectorView] 여러 그룹에 중복 등록입니다. {key.Item1}.{key.Item2}", group);
                        continue;
                    }

                    _table.Add(key, group);
                }

                group.OnValueChanged
                    .Subscribe(_onValueChanged.OnNext)
                    .AddTo(this);
            }
            //시작은 대상 없음 상태
            ShowEmpty();
        }

        //------------------------------------implement---------------------------------

        public void UpdateValue(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)
        {
            if(_table.TryGetValue((prop, axis), out TransformInspectorGroupView group) == false)
            {
                return;
            }

            group.UpdateValue(prop, axis, value);
        }

        public void ShowChannel(InspectorTransformProperty prop, InspectorTransformAxis axis, ValueRange range, float value)
        {
            if(_table.TryGetValue((prop, axis), out TransformInspectorGroupView group) == false)
            {
                return;
            }

            group.Show(prop, axis, range, value);
        }

        public void ShowEmpty()
        {
            foreach(TransformInspectorGroupView group in _groups)
            {
                if(group != null)
                {
                    group.Hide();
                }
            }
        }
        //------------------------------------implement---------------------------------

        private void OnDestroy()
        {
            _onValueChanged.OnCompleted();
            _onValueChanged.Dispose();
        }

    }

}
