using StudioCharacterSDK.Domain;

namespace StudioResourceSDK.Domain
{
    /// <summary>
    /// 실제로 오브젝트를 destroy할 infra의 domain
    /// </summary>
    public interface IResourceDestroyDomain
    {
        void Destroy(ICharacter character);
        void Destroy(IBackground backGround);
        void Destroy(IProp prop);
    }
}
