using StudioResourceSDK.Domain;
using UnityEngine;
using UnityEngine.XR.ARSubsystems;

namespace StudioResourceSDK.Infrastructure
{
    /// <summary>
    /// 화면 좌표로 씬의 리소스를 찾는 infra
    /// </summary>
    public class SceneResourcePicker : IResourcePickDomain
    {

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
