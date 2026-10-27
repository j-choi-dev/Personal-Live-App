using LiveAppUI.Domain;
using StudioResourceSDK.Application;
using System;
using System.Collections.Generic;
using UniRx;
using DomainResourceType = StudioResourceSDK.Domain.ResourceType;

namespace LiveAppUI.Model
{
    public class TransformInspectorModel : ITransformInspectorModel
    {
        private readonly ITransformInspectorContext _context;
        private readonly Subject<TransformInspectorData> _onTargetChanged =
            new Subject<TransformInspectorData>();
        public IObservable<TransformInspectorData> OnTargetChanged => _onTargetChanged;

        private bool _hasTarget = false;
        private DomainResourceType _targetType;
        private string _targetId;

        public TransformInspectorModel(ITransformInspectorContext context)
        {
            _context = context;
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

            _onTargetChanged.OnNext(TransformInspectorData.Empty);
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
        private TransformInspectorData BuildData()
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

            return new TransformInspectorData(true, channels);
        }

        public void DIspose()
        {
            _onTargetChanged.OnCompleted();
            _onTargetChanged.Dispose();
        }
    }
}
