

namespace StudioResourceSDK.Domain
{
    /// <summary>
    /// 씬에 로드된 리소스의 transform getter setter 인터페이스
    /// 어떤 transform을 어느 단위로 움직일 지는 구현체에서 결정
    /// </summary>
    public interface IResourceTransformDomain
    {
       
        /// <returns>대상이 없거나 정의하지 않는 속성, 축이면 false</returns>
        bool TryGetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, out float value);
        /// <returns>대상이 없거나 정의하지 않는 속성, 축이면 false</returns>
        bool SetValue(ResourceType type, string id, TransformProperty prop, TransformAxis axis, float value);
    }
}
