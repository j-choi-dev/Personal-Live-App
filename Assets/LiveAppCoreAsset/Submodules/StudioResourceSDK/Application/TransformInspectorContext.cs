using StudioResourceSDK.Application;
using StudioResourceSDK.Domain;
using System;
using System.Collections.Generic;
using LiveAppCore;
using System.Text;
using UnityEngine;
using Cysharp.Threading.Tasks;
using System.Linq;

namespace StudioResourceSDK.Application
{
    public class TransformInspectorContext : ITransformInspectorContext
    {
        /// <summary>
        /// 지원하지 않는 항목은 미리 만들어 놓은 해당 빈채널로 처리
        /// </summary>
        private static readonly IReadOnlyList<TransformChannelInfo> EmptyChannels = Array.Empty<TransformChannelInfo>();
        private IResourceTransformDomain _transformDomain;
        /// <summary>
        /// 리소스 타입에 따른 슬라이더 정보dict
        /// </summary>
        private readonly Dictionary<ResourceType, IReadOnlyList<TransformChannelInfo>> _channels =
            new Dictionary<ResourceType, IReadOnlyList<TransformChannelInfo>>();


        public TransformInspectorContext(IResourceTransformDomain transformDomain)
        {
            _transformDomain = transformDomain;
            _channels = CreateChannels();
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
            if(TryFindChannel(type, prop, axis, out _) == false)
            {
                return false;
            }

            return _transformDomain.TryGetValue(type, id, prop, axis, out value);
        }
        public bool SetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, float value)
        {
            if(TryFindChannel(type, prop, axis, out TransformChannelInfo channel) == false)
            {
                return false;
            }

            float clamped = Mathf.Clamp(value, channel.Range.min, channel.Range.max);
            return _transformDomain.SetValue(type, id, prop, axis, clamped);
        }
        //--------------implements-------------------------------

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

        
    }
}
