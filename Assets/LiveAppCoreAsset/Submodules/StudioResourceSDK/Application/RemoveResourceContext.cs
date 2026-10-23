using StudioResourceSDK.Domain;
using System.Linq;
using UnityEngine;


namespace StudioResourceSDK.Application
{

    public class RemoveResourceContext : IRemoveResourceContext
    {
        private readonly ISceneResourceListDomain _listDomain;
        private readonly ISceneResourceLifecycleContext _lifeCycleContext;
        private readonly IResourceDestroyDomain _destroyDomain;

        public RemoveResourceContext(
            ISceneResourceListDomain listDomain,
            ISceneResourceLifecycleContext lifeCycleContext,
            IResourceDestroyDomain destroyDomain)
        {
            _listDomain = listDomain;
            _lifeCycleContext = lifeCycleContext;
            _destroyDomain = destroyDomain;
        }


        //-------------------implements----------------------
        public bool RemoveResource(ResourceType type, string id)
        {
            switch (type)
            {
                case ResourceType.Character:
                    return RemoveCharacter(id);
                case ResourceType.BackGround:
                    return RemoveBackGroud(id);
                case ResourceType.Prop:
                    return RemoveProp(id);
                default:
                    Debug.LogWarning($"[RemoveResourceContext] 삭제할 수 없는 리소스 타입입니다.");
                    return false;
            }
        }
        //-------------------implements----------------------

        private bool RemoveCharacter(string id)
        {
            var target = _listDomain.CharacterList.FirstOrDefault(arg => arg.ID.Equals(id)); //조회
            if( target == null)
            {
                return false;
            }

            try
            {
                _listDomain.RemoveCharacter(id);     //sceneResource목록에서 해제
            }
            finally
            {
                _lifeCycleContext.RemoveGimmick(id); //gimmick목록에서 해제
                _destroyDomain.Destroy(target);      //obj 삭제
            }
            return true;
        }

        private bool RemoveBackGroud(string id)
        {
            var target = _listDomain.BackGroundList.FirstOrDefault(arg => arg.ID.Equals(id));
            if( target == null)
            {
                return false;
            }

            try
            {
                _listDomain.RemoveBackGround(id);
            }
            finally
            {                                        //@OH 26.10.16
                _lifeCycleContext.RemoveGimmick(id); //생각해보니 배경이나 캐릭터는 removegimmick을 안 해도 될 것 같은데, 혹여나 prop의 id와 동일한 캐릭이나 배경이 있다면..?
                _destroyDomain.Destroy(target);
            }
            return true;
        }

        private bool RemoveProp(string id)
        {
            var target = _listDomain.PropList.FirstOrDefault(arg => arg.ID.Equals(id));
            if( target == null)
            {
                return false;
            }

            try
            {
                _listDomain.RemoveProp(id);
            }
            finally
            {
                _lifeCycleContext.RemoveGimmick(id);
                _destroyDomain.Destroy(target);
            }
            return true;
        }



    }
}
