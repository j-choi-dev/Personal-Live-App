using StudioResourceSDK.Domain;

namespace StudioResourceSDK.Application
{
    /// 현재는 단순히 domain을 model에 노출하지 않기 위한 패스스루에 가까운 application레이어 클래스
    public class ResourcePickContext : IResourcePickContext
    {
        private readonly IResourcePickDomain _pickDomain;

        public ResourcePickContext(IResourcePickDomain pickDomain)
        {
            _pickDomain = pickDomain;
        }

        public (ResourceType type, string id)? Pick(ViewportPoint point)
        {
            //특정 타입 제외나 타입에 따른 피킹 로직을 다르게 처리하는 등
            //피킹 규칙이 생기면 이곳에 추가 @OH 26.11.03
            return _pickDomain.Pick(point);
        }
    }
}
