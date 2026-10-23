using StudioCharacterSDK.Domain;
using StudioCharacterSDK.Infrastructure;
using StudioResourceSDK.Domain;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace StudioResourceSDK.Infrastructure
{
    public class SceneResourceTransformer : IResourceTransformDomain
    {
        //------------------------------------inner class----------------------
        /// <summary>
        /// 조작 대상 하나에 대한 모든 정보
        /// 이것도 ResourceTransformType.cs에 정의할까 고민하다가
        /// 해당 구현체에서만 사용할 생각이라서 inner class로 숨김
        /// 추후 다른 resource 영역에서 쓸 일이 생기면 그 때 이전할 예정 @OH 26.10.21
        /// </summary>
        private class TransformState
        {
            public readonly Transform Target;
            public readonly float PositionScale;
            public readonly Vector3 BasePosition;
            public readonly Quaternion BaseRotation;
            /// <summary>
            /// (속성, 축) 튜플에 대한 값 저장 dict 
            /// </summary>
            private readonly Dictionary<(TransformProperty, TransformAxis), float> _value
                = new Dictionary<(TransformProperty, TransformAxis), float>();
            public TransformState( Transform target, float positionScale)
            {
                Target = target;
                PositionScale = positionScale;
                BasePosition = target.localPosition;
                BaseRotation = target.localRotation;
            }

            public float GetValue( TransformProperty prop, TransformAxis axis)
            {
                return _value.TryGetValue((prop, axis), out float value) ? value : 0.0f;
            }
            public void SetValue(TransformProperty prop, TransformAxis axis, float value)
            {
                _value[(prop, axis)] = value;
            }
        }//------------------------------------inner class----------------------
       
        /// <summary>
        /// 표시 단위 -> 로컬 단위 변환 계수 (slider value 50 -> 로컬 적용 value 0.5)
        /// </summary>
        private const float _characterPositionScale = 0.01f;
        private const float _propPositionScale = 0.01f;
        private readonly ISceneResourceListDomain _listDomain;
        /// <summary>
        /// transform 별 상태 저장 dict
        /// </summary>
        private readonly Dictionary<Transform, TransformState> _states =
            new Dictionary<Transform, TransformState>();

        public SceneResourceTransformer(ISceneResourceListDomain listdomain)
        {
            _listDomain = listdomain;
        }

        private static bool IsSupported(ResourceType type, TransformProperty prop,TransformAxis axis)
        {
            bool isPosition;
            bool isRotation;
            switch (type)
            {
                ///<summary>
                /// 우선 캐릭터와 기믹prop 오브젝트의 작동 방식을 동일하게 구현
                /// 추후 다른 작동 방식이나 background or 다른 case 추가 시 여기서 @OH 26.10.23
                ///</summary>
                case ResourceType.Character:
                case ResourceType.Prop:
                     isPosition = prop == TransformProperty.Position
                        && (axis == TransformAxis.X || axis == TransformAxis.Y);

                    isRotation = prop == TransformProperty.Rotation
                        && axis == TransformAxis.Z;
                    return isPosition || isRotation;
                default:
                    return false;
            }
        }

        private static void ApplyPosition(TransformState state)
        {
            Vector3 offset = new Vector3(
                state.GetValue(TransformProperty.Position, TransformAxis.X),
                state.GetValue(TransformProperty.Position, TransformAxis.Y),
                state.GetValue(TransformProperty.Position, TransformAxis.Z)
            ) * state.PositionScale;

            state.Target.localPosition = state.BasePosition + offset;
        }

        private static void ApplyRotation(TransformState state)
        {
            Quaternion offset = Quaternion.Euler(
                state.GetValue(TransformProperty.Rotation, TransformAxis.X),
                state.GetValue(TransformProperty.Rotation, TransformAxis.Y),
                state.GetValue(TransformProperty.Rotation, TransformAxis.Z) );

            state.Target.localRotation = state.BaseRotation * offset;
        }

        /// <summary>
        /// 지원 여부 체크 -> 대상 찾음 -> 상태 가져옴
        /// </summary>
        private bool TryGetState(ResourceType type, string id, TransformProperty prop, TransformAxis axis
            , out TransformState state)
        {
            state = null;

            if (IsSupported(type, prop, axis) == false)
            {
                return false;
            }

            if (TryResolveTarget(type, id, out Transform target, out float positionScale) == false)
            {
                return false;
            }

            state = GetOrCreateState(target, positionScale);
            return true;
        }

        /// <summary>
        /// id로 실제 조작할 transform과 scale계수를 찾음
        /// </summary>
        private bool TryResolveTarget(ResourceType type, string id, out Transform target, out float positionScale)
        {
            target = null;
            positionScale = 1.0f;

            switch(type)
            {
                case ResourceType.Character:
                    return TryResolveCharacter(id, out target, out positionScale);

                case ResourceType.Prop:
                    return TryResolveProp(id, out target, out positionScale);

                default:
                    return false;
            }
        }
        
        private bool TryResolveCharacter(string id, out Transform target, out float positionScale)
        {
            target = null;
            positionScale = _characterPositionScale;

            ICharacter character = _listDomain.CharacterList.FirstOrDefault(arg => arg.ID == id);
            if(character == null)
            {
                Debug.LogWarning($"[SceneResourceTransformer] 캐릭터를 찾지 못했습니다.");
                return false;
            }

            StudioAvatar avatar = character as StudioAvatar;
            if( avatar == null)
            {
                Debug.LogWarning($"[SceneResourceTransformer] StudioAvatar가 아니거나 파괴된 캐릭터입니다. ID={id}");
                return false;
            }

            if (avatar.Avatar == null)
            {
                Debug.LogWarning($"[SceneResourceTransformer] CubismModel이 없습니다. ID={id}");
                return false;
            }

            target = avatar.Avatar.transform;
            return true;

        }

        private bool TryResolveProp(string id, out Transform target, out float positionScale)
        {
            target = null;
            positionScale = _propPositionScale;

            IProp prop = _listDomain.PropList.FirstOrDefault(arg => arg.ID == id);
            if(prop == null)
            {
                Debug.LogWarning($"[SceneResourceTransformer] Prop을 찾지 못했습니다. ID=[{id}]");
                return false;
            }

            Component comp = prop as Component;
            if(comp == null)
            {
                Debug.LogWarning($"[SceneResourceTransformer] Component가 아니거나 파괴된 Prop입니다. ID=[{id}]");
                return false;
            }

            target = comp.transform;
            return true;
        }

        private TransformState GetOrCreateState(Transform target, float positionScale)
        {
            if(_states.TryGetValue(target, out TransformState state))
            {
                return state;
            }

            RemoveDestroyedStates();

            state = new TransformState(target, positionScale);
            _states.Add(target, state);
            return state;
        }

        private void RemoveDestroyedStates()
        {
            List<Transform> destroyed = _states.Keys.Where(key => key == null).ToList();

            foreach( Transform key in destroyed)
            {
                _states.Remove(key);
            }

        }

        

        //----------------------------------implements---------------------------------
        public bool TryGetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, out float value)
        {
            value = 0.0f;

            if( TryGetState(type, id, prop, axis, out TransformState state) == false)
            {
                return false;
            }

            value = state.GetValue(prop, axis);
            return true;
        }

        public bool SetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, float value)
        {
            if( float.IsNaN(value) || float.IsInfinity(value) )
            {
                Debug.LogWarning($"[SceneResourceTransformer] 값이 올바르지 않습니다. ID={id}, {prop}.{axis}={value}");
                return false;
            }

            if (TryGetState(type, id, prop, axis, out TransformState state) == false)
            {
                return false;
            }

            switch (prop)
            {
                case TransformProperty.Position:
                    state.SetValue(prop, axis, value);
                    ApplyPosition(state);
                    return true;

                case TransformProperty.Rotation:
                    state.SetValue(prop, axis, value);
                    ApplyRotation(state);
                    return true;

                case TransformProperty.Scale:
                default:
                    return false;
            
            }
        }
        //----------------------------------implements---------------------------------
    }
}
