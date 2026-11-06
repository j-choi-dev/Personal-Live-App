using StudioResourceSDK.Domain;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// 화면 좌표로 씬의 리소스를 찾고 선택 표시에 쓸 화면 범위를 조회함
    /// 선택 상태는 저장 안 함
    /// 현재는 단순히 domain을 model에 노출하지 않기 위한 패스스루에 가까운 상태
    /// </summary>
    public interface IResourcePickContext
    {
        /// <summary>
        /// Viewport 좌표에서 맞은 리소스를 찾음
        /// </summary>
        /// <param name="point">렌더 카메라 기준 viewport 좌표(0~1)</param>
        /// <returns>맞은 리소스의 (타입,id)튜플. 없으면 null</returns>
        (ResourceType type, string id)? Pick(ViewportPoint point);

        /// <summary>
        /// 리소스가 화면에서 차지하는 범위를 구함.
        /// </summary>
        /// <param name="type">리소스 타입</param>
        /// <param name="id">리소트 id</param>
        /// <param name="rect">렌더 카메라 기준 viewport 사각형</param>
        /// <returns>리소스를 찾아 범위를 구했으면 true</returns>
        bool TryGetViewportRect(ResourceType type, string id, out ViewportRect rect);
    }
}
