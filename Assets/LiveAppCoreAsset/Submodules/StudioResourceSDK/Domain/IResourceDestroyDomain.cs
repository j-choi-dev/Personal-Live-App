namespace StudioResourceSDK.Domain
{
    /// <summary>
    /// 실제로 오브젝트를 destroy할 infra의 domain
    /// </summary>
    public interface IResourceDestroyDomain
    {
        /// <summary>
        /// 실제로 오브젝트를 destroy
        /// </summary>
        /// <param name="target">삭제할 obj</param>
        void Destroy(object target);
    }
}
