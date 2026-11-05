using StudioResourceSDK.Domain;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// 화면 좌표로 씬의 리소스를 찾는 app레이어 유스케이스
    /// 조회만 하고 선택 상태는 저장하지 않음(UIModel이 관리함)
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
        /// 대상 리소스에 선택 표시 UI를 띄움.
        /// </summary>
        /// <param name="type">리소스 타입</param>
        /// <param name="id">오브젝트 ID</param>
        void ShowHighlight(ResourceType type, string id);

        /// <summary>
        /// 선택 표시 UI를 숨김 
        /// </summary>
        void HideHighlight();
    }
}
