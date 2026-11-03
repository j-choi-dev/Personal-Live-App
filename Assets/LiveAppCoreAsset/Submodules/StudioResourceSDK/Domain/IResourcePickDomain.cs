using System;
using System.Collections.Generic;
using System.Text;

namespace StudioResourceSDK.Domain
{
    /// <summary>
    /// 화면 좌표로 씬의 리소스를 찾아낼 infra의 domain
    /// </summary>
    public interface IResourcePickDomain
    {
        /// <summary>
        /// Viewport 좌표에서 맞은 리소스를 찾음
        /// </summary>
        /// <param name="point">렌더 카메라 기준 viewport 좌표(0~1)</param>
        /// <returns>맞은 리소스의 (타입,id)튜플. 없으면 null</returns>
        (ResourceType type, string id)? Pick(ViewportPoint point);
    }
}
