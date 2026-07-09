using StudioNetworkSDK.Domain;
using System.Collections.Generic;

namespace StudioResourceSDK.Application
{
    /// <summary>
    /// 리소스 서버 정보 취득을 정의한 Application
    /// </summary>
    public interface IResourceServerConfigContext
    {
        /// <summary>
        /// Config 데이터를 읽어들여서 데이터 클래스에 대입하는 절차
        /// </summary>
        /// <param name="rawData">Config</param>
        /// <returns>Server Config 리스트</returns>
        IReadOnlyCollection<ResourceServerData> ParseServerConfigData(string rawData);
    }
}
