using StudioResourceSDK.Domain;
using UnityEngine;

namespace StudioResourceSDK.Infrastructure
{
    public class SceneResourceDestroyer : IResourceDestroyDomain
    {
        public void Destroy(object target)
        {
            var obj = target as GameObject;
            // 해당 == 연산자는 c#순수연산자가 아닌 unity의 == 연산자로 이미 파괴된 오브젝트도 null로 판정하여 이중 파괴 방지함
            if (obj == null)
            {
                Debug.LogWarning($"[SceneResourceDestroyer] 파괴할 오브젝트를 찾지 못했습니다.");
                return;
            }

            //Destroy는 프레임 끝에 실행되므로 그전까지 렌더링과 update가 돌지 않도록 먼저 비활성화 시킨 후 삭제
            obj.SetActive(false);
            Object.Destroy(obj);
        }
    }
}
