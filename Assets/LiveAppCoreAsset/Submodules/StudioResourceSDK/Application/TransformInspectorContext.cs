using LiveAppCore;
using LiveAppUI.View;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using System.Linq;
using UniRx;
using UnityEngine;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// 로드된 리소스의 오프셋 변경을 하나의 프로퍼티로 발행(OnOffsetChanged)
    /// Context가 리소스 목록 변경을 따라가며 각 리소스의 ITransformData 이벤트를 구독 및 해제
    /// </summary>
    public class TransformInspectorContext : ITransformInspectorContext
    {
        // 표시 단위 -> 로컬 단위 변환 계수 (slider value 50 -> 로컬 0.5)
        private const float _characterPositionScale = 0.01f;
        private const float _propPositionScale = 0.01f;
        
        /// <summary>
        /// Scale 오프셋은 배율의 지수 (배율 = 2^값, 0 = 원래 크기)
        /// View의 배율 라벨 표시도 같은 밑을 사용하므로 수정 시 함께 수정할 것 <see cref="ObservableExponentSliderUGUI"/>
        /// </summary>
        private const float _scaleExponentBase = 2.0f;

        /// <summary>지원하지 않는 항목은 미리 만들어 놓은 해당 빈채널로 처리</summary>
        private static readonly IReadOnlyList<TransformChannelInfo> EmptyChannels = Array.Empty<TransformChannelInfo>();

        /// <summary>목록 조회 및 로드,삭제 알림용. </summary>
        /// <remarks> 현재는 읽기만 하며 Add,Remove등 함수는 사용하지 않음</remarks>
        private readonly ISceneResourceListDomain _listDomain;

        /// <summary>리소스 타입에 따른 슬라이더 정보dict</summary>
        private readonly Dictionary<ResourceType, IReadOnlyList<TransformChannelInfo>> _channels;

        /// <summary>대상 별 속성들 기준 상태</summary>
        private readonly Dictionary<ITransformData, TransformBase> _bases = new Dictionary<ITransformData, TransformBase>();

        /// <summary>대상 별 리소스 이벤트 구독, 리소스가 목록에서 빠지면 해제</summary>
        private readonly Dictionary<ITransformData, IDisposable> _subscriptions = new Dictionary<ITransformData, IDisposable>();

        /// <summary>리소스 목록 변경 구독 정리용</summary>
        private readonly CompositeDisposable _disposable = new CompositeDisposable();

        private readonly Subject<TransformOffsetChangedData> _onOffsetChanged = new Subject<TransformOffsetChangedData>();
        public IObservable<TransformOffsetChangedData> OnOffsetChanged => _onOffsetChanged;

        public TransformInspectorContext(ISceneResourceListDomain listDomain)
        {
            _listDomain = listDomain;
            _channels = CreateChannels();

            // 리소스가 로드, 삭제 될 때마다 구독 대상을 목록에 맞춤
            Observable.Merge(
                _listDomain.OnChangedCharacterList.AsUnitObservable(),
                _listDomain.OnChangedPropList.AsUnitObservable())
                .Subscribe(_ => SyncSubscriptions())
                .AddTo(_disposable);

            //해당 Context 생성 전에 이미 로드된 리소스도 구독
            SyncSubscriptions();
        }

        //--------------implements-------------------------------
        public IReadOnlyList<TransformChannelInfo> GetChannels(ResourceType type)
        {
            return _channels.TryGetValue(type, out IReadOnlyList<TransformChannelInfo> channels)
                ? channels
                : EmptyChannels;
        }
        public bool TryGetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, out float value)
        {
            value = 0f;

            if(TryFindChannel(type, prop, axis, out _) == false
            || TryResolve(type, id, out ITransformData data) == false )
            {
                return false;
            }

            value = ToOffset(type, data, GetOrCaptureBase(data), prop, axis);
            return true;
        }

        public bool SetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, float value)
        {
            if(float.IsNaN(value) || float.IsInfinity(value))
            {
                return false;
            }

            if(TryFindChannel(type, prop, axis, out TransformChannelInfo channel) == false
            || TryResolve(type, id, out ITransformData data) == false)
            {
                return false;
            }

            float clamped = UnityEngine.Mathf.Clamp(value, channel.Range.min, channel.Range.max);
            ApplyOffset(type, data, GetOrCaptureBase(data), prop, axis, clamped);
            return true;
        }
        //---------------------------------------------/implements

        /// <summary>
        /// 현재 리소스 목록과 구독 목록의 싱크 맞춤
        /// </summary>
        private void SyncSubscriptions()
        {
            var alive = new Dictionary<ITransformData, (ResourceType type, string id)>();

            foreach(var character in _listDomain.CharacterList)
            {
                if(character is ITransformData data && data.CanTransform)
                {
                    alive[data] = (ResourceType.Character, character.ID);
                }
            }

            foreach(var prop in _listDomain.PropList)
            {
                if(prop is ITransformData data && data.CanTransform)
                {
                    alive[data] = (ResourceType.Prop, prop.ID);
                }
            }

            //사라진 리소스가 있다면 구독 해제 및 기준 상태 정리
            foreach(ITransformData key in _subscriptions.Keys.ToList())
            {
                if(alive.ContainsKey(key))
                {
                    continue;
                }

                _subscriptions[key].Dispose();
                _subscriptions.Remove(key);
                _bases.Remove(key);
            }

            //새로 생긴 리소스가 있다면 구독
            foreach( var pair in alive)
            {
                if(_subscriptions.ContainsKey(pair.Key) )
                {
                    continue;
                }

                _subscriptions.Add(pair.Key, SubscribeOffsets(pair.Value.type, pair.Value.id, pair.Key) );
            }
        }

        /// <summary>
        /// 리소스 하나의 채널별(slider에 매핑되는 데이터) 실제 값 변경을 오프셋으로 바꿔 발행
        /// </summary>
        private IDisposable SubscribeOffsets(ResourceType type, string id, ITransformData data)
        {
            var streams = GetChannels(type)
                .Select(channel => ObserveRaw(data, channel.Property, channel.Axis)
                    .Select(_ => new TransformOffsetChangedData(
                        type,
                        id,
                        channel.Property,
                        channel.Axis,
                        ToOffset(type, data, GetOrCaptureBase(data), channel.Property, channel.Axis ) ) ) );

            return Observable.Merge(streams)
                .Subscribe(_onOffsetChanged.OnNext );
        }

        /// <summary>id로 대상 찾기 시도</summary>
        private bool TryResolve(ResourceType type, string id, out ITransformData data)
        {
            switch (type)
            {
                case ResourceType.Character:
                    data = _listDomain.CharacterList.FirstOrDefault(arg => arg.ID == id) as ITransformData;
                    break;
                case ResourceType.Prop:
                    data = _listDomain.PropList.FirstOrDefault(arg => arg.ID == id) as ITransformData;
                    break;
                default:
                    data = null;
                    break;
            }

            if(data.CanTransform == false)
            {
                UnityEngine.Debug.LogWarning($"[TransformInspectorContext] 조작 가능한 상태가 아닙니다. Type={type}, ID={id}");
                data = null;
                return false;
            }

            return true;
        }

        /// <summary>속성들 기준 상태 조회</summary>
        /// <remarks>삭제된 대상 정리는 <see cref="SyncSubscriptions"/></remarks>
        private TransformBase GetOrCaptureBase(ITransformData data)
        {
            if(_bases.TryGetValue(data, out TransformBase tBase) )
            {
                return tBase;
            }

            //조회 안 되면 새로 생성
            tBase = TransformBase.Capture(data);
            _bases.Add(data, tBase);
            return tBase;
        }

        /// <summary>
        /// 실제 값 -> offset으로 변경 
        /// </summary>
        private float ToOffset (ResourceType type, ITransformData data, TransformBase tBase, TransformProperty prop, TransformAxis axis)
        {
            switch(prop)
            {
                case TransformProperty.Position:
                    return (GetRaw(data, prop, axis) - tBase.Get(prop, axis)) / GetPositionScale(type);

                case TransformProperty.Rotation:
                    {
                        float baseValue = tBase.Get(prop, axis);
                        float raw = GetRaw(data, prop, axis);
                        float offset = Mathf.DeltaAngle(baseValue, raw);
                        UnityEngine.Debug.Log($"[RotDebug] base={baseValue}, raw={raw}, offset={offset}");
                        return offset;
                    }
                    //return Mathf.DeltaAngle(tBase.Get(prop, axis), GetRaw(data, prop, axis)); // localEulerAngles는 0~360으로 돌아오므로 각도 -180, 180으로 계산

                case TransformProperty.Scale:
                    {
                        float baseScale = tBase.ScaleX; //현재는 비율 고정이라 x축을 대표로 사용 @OH 26.11.12
                        float currentScale = data.ScaleX;
                        if (baseScale <= 0.0f || currentScale <= 0.0f)
                        {
                            return 0.0f; // 0 이하는 로그 계산 불가
                        }
                        return UnityEngine.Mathf.Log(currentScale / baseScale, _scaleExponentBase);
                    }
                default:
                    return 0.0f;

            }
        }

        /// <summary>
        /// offset -> 실제값 적용
        /// </summary>
        private static void ApplyOffset(ResourceType type, ITransformData data,TransformBase tBase, TransformProperty prop, TransformAxis axis, float offset)
        {
            switch(prop)
            {
                case TransformProperty.Position:
                    SetRaw(data, prop, axis, tBase.Get(prop, axis) + offset * GetPositionScale(type));
                    break;
                case TransformProperty.Rotation:
                    SetRaw(data, prop, axis, tBase.Get(prop, axis) + offset);
                    break;
                case TransformProperty.Scale:
                    {
                        // 세 축에 같은 배율을 곱해 원래 비율 유지
                        // Z까지 스케일링하는 이유: 불균일 스케일로 인한 렌더링 왜곡 방지
                        // Z 변경으로 깊이 순서 문제가 생기면 Z만 기준값(transformBase.ScaleZ)을 유지할 것
                        float multiplier = UnityEngine.Mathf.Pow(_scaleExponentBase, offset);
                        data.SetScaleX(tBase.ScaleX * multiplier);
                        data.SetScaleY(tBase.ScaleY * multiplier);
                        data.SetScaleZ(tBase.ScaleZ * multiplier);
                        break;
                    }
            }    
        }

        /// <summary>type에 따른 position 변경 시의 스케일 배율 값 반환</summary>
        private static float GetPositionScale(ResourceType type)
        {
            switch (type)
            {
                case ResourceType.Character:
                    return _characterPositionScale;
                case ResourceType.Prop:
                    return _propPositionScale;
                default:
                    return 1.0f;
            }

        }

        /// <summary>(속성, 축)에 일치하는 case에 값 get</summary>
        /// <remarks><see cref="ToOffset"/>내부에서 사용 중</remarks>
        private static float GetRaw(ITransformData data, TransformProperty prop, TransformAxis axis)
        {
            switch (prop)
            {
                case TransformProperty.Position:
                    switch (axis)
                    {
                        case TransformAxis.X: return data.PositionX;
                        case TransformAxis.Y: return data.PositionY;
                        case TransformAxis.Z: return data.PositionZ;
                    }
                    break;

                case TransformProperty.Rotation:
                    switch (axis)
                    {
                        case TransformAxis.X: return data.RotationX;
                        case TransformAxis.Y: return data.RotationY;
                        case TransformAxis.Z: return data.RotationZ;
                    }
                    break;

                case TransformProperty.Scale:
                    switch (axis)
                    {
                        case TransformAxis.X:
                        case TransformAxis.Uniform: return data.ScaleX;
                        case TransformAxis.Y: return data.ScaleY;
                        case TransformAxis.Z: return data.ScaleZ;
                    }
                    break;
            }

            return 0f;
        }

        /// <summary>(속성, 축)에 일치하는 case에 값 set</summary>
        /// <remarks><see cref="ApplyOffset"/>내부에서 사용 중</remarks>
        private static void SetRaw(ITransformData data, TransformProperty prop, TransformAxis axis, float value)
        {
            switch (prop)
            {
                case TransformProperty.Position:
                    switch (axis)
                    {
                        case TransformAxis.X: data.SetPositionX(value); return;
                        case TransformAxis.Y: data.SetPositionY(value); return;
                        case TransformAxis.Z: data.SetPositionZ(value); return;
                    }
                    break;
                case TransformProperty.Rotation:
                    switch (axis)
                    {
                        case TransformAxis.X: data.SetRotationX(value); return;
                        case TransformAxis.Y: data.SetRotationY(value); return;
                        case TransformAxis.Z: data.SetRotationZ(value); return;
                    }
                    break;
                case TransformProperty.Scale:
                    switch(axis)
                    {
                        case TransformAxis.X: data.SetScaleX(value); return;
                        case TransformAxis.Y: data.SetScaleY(value); return;
                        case TransformAxis.Z: data.SetScaleZ(value); return;
                    }
                    break;        
            }
            UnityEngine.Debug.LogWarning($"[TransformInspectorContext]적용할 수 없는 case = {prop},{axis}");
        }

        /// <summary>해당 (속성, 축) case의 값 변경 이벤트</summary>
        /// <remarks>현재 Scale Uniform의 경우는 x축 이벤트를 대표로 사용 @OH 26.11.12</remarks>

        private static IObservable<float> ObserveRaw(ITransformData data, TransformProperty prop, TransformAxis axis)
        {
            switch (prop)
            {
                case TransformProperty.Position:
                    switch(axis)
                    {
                        case TransformAxis.X: return data.OnPositionXChanged;
                        case TransformAxis.Y: return data.OnPositionYChanged;
                        case TransformAxis.Z: return data.OnPositionZChanged;
                    }
                    break;

                case TransformProperty.Rotation:
                    switch (axis)
                    {
                        case TransformAxis.X: return data.OnRotationXChanged;
                        case TransformAxis.Y: return data.OnRotationYChanged;
                        case TransformAxis.Z: return data.OnRotationZChanged;
                    }
                    break;

                case TransformProperty.Scale:
                    switch (axis)
                    {
                        case TransformAxis.X:
                        case TransformAxis.Uniform: 
                            return data.OnScaleXChanged;
                        case TransformAxis.Y: return data.OnScaleYChanged;
                        case TransformAxis.Z: return data.OnScaleZChanged;

                    }
                    break;
            }//outer switch

            return Observable.Empty<float>();

        }

        /// <summary>
        /// 프로퍼티에 따른 지원 가능한 목록들 정적으로 미리 정의
        /// </summary>
        private static Dictionary<ResourceType, IReadOnlyList<TransformChannelInfo>> CreateChannels()
        {
            return new Dictionary<ResourceType, IReadOnlyList<TransformChannelInfo>>
            {
                { ResourceType.Character, CreateCharacterChannels() },
                { ResourceType.Prop,      CreatePropChannels() },
            };
        }

        private static IReadOnlyList<TransformChannelInfo> CreateCharacterChannels()
        {
            return new List<TransformChannelInfo>
            {
                Channel(TransformProperty.Position, TransformAxis.X, -50f,50f),
                Channel(TransformProperty.Position, TransformAxis.Y, -100f,100f),
                Channel(TransformProperty.Rotation, TransformAxis.Z, -180f,180f),
                Channel(TransformProperty.Scale, TransformAxis.Uniform, -2f, 2f), //2^-2 ~ 2^2 = 0.25배 ~ 4배
            };
        }

        private static IReadOnlyList<TransformChannelInfo> CreatePropChannels()
        {
            return new List<TransformChannelInfo>
            {
                Channel(TransformProperty.Position, TransformAxis.X, -50f,50f),
                Channel(TransformProperty.Position, TransformAxis.Y, -100f,100f),
                Channel(TransformProperty.Rotation, TransformAxis.Z, -180f,180f),
                Channel(TransformProperty.Scale, TransformAxis.Uniform, -2f, 2f), //2^-2 ~ 2^2 = 0.25배 ~ 4배
            };
        }
        /// <summary>
        /// 항목 하나 생성. 위 create부분에서 정의부가 너무 길어져서 유틸함수로 추가함
        /// </summary>
        private static TransformChannelInfo Channel(TransformProperty property, TransformAxis axis, float min, float max)
        {
            return new TransformChannelInfo(property, axis, new ValueRange(min, max));
        }

        private bool TryFindChannel(ResourceType type, TransformProperty prop, TransformAxis axis, out TransformChannelInfo channel)
        {
            channel = GetChannels(type)
                .FirstOrDefault(arg => arg.Property == prop
                                    && arg.Axis == axis);

            return channel != null;
        }

        public void Dispose()
        {
            _disposable.Dispose();
            foreach (IDisposable sub in _subscriptions.Values)
            {
                sub.Dispose();
            }
            _subscriptions.Clear();
            _onOffsetChanged.Dispose();
        }
    }
}
