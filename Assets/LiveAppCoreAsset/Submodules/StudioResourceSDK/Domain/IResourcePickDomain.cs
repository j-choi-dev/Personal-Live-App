

namespace StudioResourceSDK.Domain
{
    /// <summary>
    /// 화면 좌표로 씬의 리소스를 찾고 리소스의 화면 범위를 구함
    /// </summary>
    public interface IResourcePickDomain
    {
        /// <summary>
        /// Viewport 좌표에서 맞은 리소스를 찾음
        /// 씬 수정은 안 함
        /// Try방식으로 리팩하는 게 코드 이해하는 게 나으려나 @OH 26.11.05
        /// </summary>
        /// <param name="point">렌더 카메라 기준 viewport 좌표(0~1)</param>
        /// <returns>맞은 리소스의 (타입,id)튜플. 없으면 null</returns>
        (ResourceType type, string id)? Pick(ViewportPoint point);

        /// <summary>
        /// 리소스가 화면에서 차지하는 범위를 구함
        /// 씬 수정은 안 함
        /// </summary>
        /// <param name="type">리소스 타입</param>
        /// <param name="id">리소스 ID</param>
        /// <param name="rect">렌더 카메라 기준 viewport 사각형</param>
        /// <returns>리소스 찾아서 범위를 구했으면 true</returns>
        bool TryGetViewportRect(ResourceType type, string id, out ViewportRect rect);
    }
}
