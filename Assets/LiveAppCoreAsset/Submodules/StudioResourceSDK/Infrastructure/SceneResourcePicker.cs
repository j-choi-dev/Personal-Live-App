using StudioResourceSDK.Domain;
using UnityEngine;
using System.Collections.Generic;
using Live2D.Cubism.Core;
using StudioCharacterSDK.Domain;
using Zenject;
using StudioCommonSDK.Domain;
using LiveAppCore;


namespace StudioResourceSDK.Infrastructure
{
    /// <summary>
    /// 화면 좌표로 씬의 리소스를 찾는 infra
    /// 현재는 캐릭터만 지원
    /// </summary>
    public class SceneResourcePicker : IResourcePickDomain
    {
        private readonly ISpawnPivotTransform _objectTransform;

        public SceneResourcePicker([Inject(Id = SpawnPivotId.Object)] ISpawnPivotTransform objectTransform)
        {
            _objectTransform = objectTransform;
        }


        /// <summary>
        /// 임시로 영역 나눠서 type,id 튜플 반환되는 지 체크 중 
        /// </summary>
        /// <param name="point"></param>
        /// <returns></returns>
        public (ResourceType type, string id)? Pick(ViewportPoint point)
        {
            (ResourceType type, string id)? result;

            if (point.X >= 0.5f)
            {
                result = null;
            }

            else if (point.Y < 0.5f)
            {
                result = (ResourceType.Character, "test-id-A");
            }
            else
            {
                result = (ResourceType.Character, "test-id-B");
            }

            string resultText = result.HasValue ? $"{result.Value.type}, {result.Value.id}" : "None";
            Debug.Log($"[SceneResourcePicker] {point} 결과 : {resultText}");

            return result;
        }
    }
}
