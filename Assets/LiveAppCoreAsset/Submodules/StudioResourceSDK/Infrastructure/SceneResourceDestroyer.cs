using StudioCharacterSDK.Domain;
using StudioResourceSDK.Domain;
using UnityEngine;

namespace StudioResourceSDK.Infrastructure
{
    public class SceneResourceDestroyer : IResourceDestroyDomain
    {
        private static void DestroyObject(Component component, string id)
        {
            // 해당 == 연산자는 c#순수연산자가 아닌 unity의 == 연산자로 이미 파괴된 오브젝트도 null로 판정하여 이중 파괴 방지함
           if(component == null)
            {
                Debug.LogWarning($"[SceneResourceDestroyer] 파괴할 오브젝트를 찾지 못했습니다.");
                return;
            }

            UnityEngine.Object.Destroy(component.gameObject);
        }

        //---------implements-----------
        public void Destroy(ICharacter character)
        {
            DestroyObject(character as Component, character?.ID);
        }
        public void Destroy(IBackground backGround)
        {
            DestroyObject(backGround as Component, backGround?.ID);
        }
        public void Destroy(IProp prop)
        {
            DestroyObject(prop as Component, prop?.ID);
        }
        //---------implements-----------
    }
}
