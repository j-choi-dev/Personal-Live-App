namespace StudioResourceSDK.Application
{
    /// <summary>
    /// 삭제 유스케이스에 대한 application
    /// </summary>
    public interface IRemoveResourceContext
    {
        /// <summary>
        /// 씬에 생성된 리소스 삭제 유스케이스
        /// 조회 -> sceneResourceList에서 해제 -> gimmickList 해제 -> destroy
        /// </summary>
        /// <param name="type">StudioResourceSDK.Domain의 enum ResourceType 삭제할 리소스 타입</param>
        /// <param name="id">삭제할 obj id</param>
        /// <returns>삭제 진행되면 true 대상 없으면 false</returns>
        bool RemoveResource(Domain.ResourceType type, string id);
    }
}
