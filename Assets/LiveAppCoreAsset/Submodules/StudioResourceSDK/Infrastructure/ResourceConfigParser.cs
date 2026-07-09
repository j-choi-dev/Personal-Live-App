using StudioNetworkSDK.Domain;
using StudioResourceSDK.Domain;
using System.Collections.Generic;

namespace StudioResourceSDK.Infrastructure
{
    /// <summary>
    /// Resource Config 정보 데이터를 파싱하는 Interface
    /// </summary>
    public class ResourceConfigParser : IResourceConfigParseDomain
    {
        public IReadOnlyCollection<ResourceServerData> ParseServerConfigData( string rawData )
        {
            var list = new List<ResourceServerData>();
            var row = rawData.Split('\n');
            for(var i = 0; i < row.Length; i++ )
            {
                if( string.IsNullOrWhiteSpace( row[i] ) )
                {
                    break;
                }
                var colDatas = row[i].Split( ',' );
                var data = new ResourceServerData(
                    ( ResourceType )System.Enum.Parse(typeof(ResourceType), colDatas[0]),
                    ( ServerType )System.Enum.Parse( typeof( ServerType ), colDatas[1] ),
                    colDatas[2],
                    colDatas[3]);
                list.Add(data );
            }
            return list;
        }
    }
}
