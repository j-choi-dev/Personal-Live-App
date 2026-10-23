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
        private readonly Dictionary<ResourceType, IReadOnlyList<TransformChannelInfo>> _channels =
            new Dictionary<ResourceType, IReadOnlyList<TransformChannelInfo>>
            {
                {
                    ResourceType.Character,
                    new List<TransformChannelInfo>
                    {
                        new TransformChannelInfo(TransformProperty.Position, TransformAxis.X, new ValueRange(-50f, 50f)),
                        new TransformChannelInfo(TransformProperty.Position, TransformAxis.Y, new ValueRange(-50f, 50f))
                    }//list element
                },//dict element
            };//dict


        public TransformInspectorContext(IResourceTransformDomain transformDomain)
        {
            _transformDomain = transformDomain;
        }

        private bool TryFindChannel(ResourceType type, TransformProperty prop, TransformAxis axis, out TransformChannelInfo channel)
        {
            channel = GetChannels(type)
                .FirstOrDefault(arg => arg.Property == prop
                                    && arg.Axis == axis);

            return channel != null;
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
    }
}
