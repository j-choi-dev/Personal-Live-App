using LiveAppUI.Domain;
using StudioResourceSDK.Application;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using UniRx;
using DomainResourceType = StudioResourceSDK.Domain.ResourceType;

namespace LiveAppUI.Model
{
    public class TransformInspectorModel : ITransformInspectorModel, IDisposable
    {
        private ITransformInspectorContext _context;
        private Subject<TransformInspectorUIData> _onTargetChanged =
            new Subject<TransformInspectorUIData>();
        public IObservable<TransformInspectorUIData> OnTargetChanged => _onTargetChanged;

        private Subject<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)> _onValueChanged
            = new Subject<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)>();

        public IObservable<(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)> OnValueChanged => _onValueChanged;

        /// <summary> Context 오프셋 변경 구독 </summary>
        /// <remarks> 대상이 바뀌어도 필터 기준(_targetType, _targetId)만 바뀌므로 한 번만 구독</remarks>
        private IDisposable _offsetSubscription;

        private bool _hasTarget = false;
        private DomainResourceType _targetType;
        private string _targetId;

        public TransformInspectorModel(ITransformInspectorContext context)
        {
            _context = context;

            _offsetSubscription = _context.OnOffsetChanged
                .Where(IsCurrentTarget)
                .Subscribe(data => 
                _onValueChanged.OnNext( (data.Property.ToInspector(), data.Axis.ToInspector(), data.Offset) ) );
        }

        //----------------------implement----------------------------
        public void SetTarget(ResourceType type, string id)
        {
            if( string.IsNullOrEmpty( id ) )
            {
                ClearTarget();
                return;
            }

            _hasTarget = true;
            _targetType = type.ToDomain();
            _targetId = id;

            _onTargetChanged.OnNext( BuildData() );
        }
        public void ClearTarget()
        {
            _hasTarget = false;
            _targetId = null;

            _onTargetChanged.OnNext(TransformInspectorUIData.Empty);
        }
        public void SetValue(InspectorTransformProperty prop, InspectorTransformAxis axis, float value)
        {
            if(_hasTarget == false)
            {
                return;
            }

            bool result = _context.SetValue(_targetType, _targetId, prop.ToDomain(), axis.ToDomain(), value);
            if(result == false)
            {
                UnityEngine.Debug.LogWarning($"[TransformInspectorModel] SetValue 실패. {_targetType} {_targetId} {prop}.{axis}={value}");
            }
        }
        //----------------------implement----------------------------

        /// <summary>
        /// ResourceSDK app계층 데이터를 ui용 데이터로 변환 및 생성
        /// </summary>
        /// <returns>UI용 데이터</returns>
        private TransformInspectorUIData BuildData()
        {
            var channels = new List<TransformInspectorChannelData>();

            foreach(TransformChannelInfo info in _context.GetChannels(_targetType))
            {
                if(_context.TryGetValue(_targetType, _targetId, 
                    info.Property, info.Axis, out float value) == false)
                {
                    continue;
                }

                channels.Add(new TransformInspectorChannelData(
                    info.Property.ToInspector(),
                    info.Axis.ToInspector(),
                    info.Range,
                    value
                    ) );
            }

            return new TransformInspectorUIData(true, channels);
        }

        /// <summary>현재 타겟인지 체크</summary>
        private bool IsCurrentTarget(TransformOffsetChangedData data)
        {
            return _hasTarget
                && data.Type == _targetType
                && data.Id == _targetId;
        }

        public void Dispose()
        {
            //OnNext 발행하는 쪽 구독부터 끊고 나머지 subject 정리
            _offsetSubscription.Dispose();

            _onTargetChanged.OnCompleted();
            _onTargetChanged.Dispose();

            _onValueChanged.OnCompleted();
            _onValueChanged.Dispose();
        }
    }
}
