using StudioNetworkSDK.Domain;
using StudioResourceSDK.Domain;
using System.Collections.Generic;

namespace StudioResourceSDK.Infrastructure
{
    public class ResourceConfigParser : IResourceConfigParseDomain
    {
        public IReadOnlyCollection<ResourceServerData> ResourceServerDatas => throw new System.NotImplementedException();

        public IReadOnlyCollection<ResourceServerData> ParseServerConfigData( string rawData )
        {
            var list = new List<ResourceServerData>();
            var row = rawData.Split('\n');
            for(var i = 0; i < row.Length; i++ )
            {
                var colDatas = row[i].Split( ',' );
                var data = new ResourceServerData();
                data._resourceType = ( ResourceType )System.Enum.Parse(typeof(ResourceType), colDatas[0]);
                data._serverType = ( ServerType )System.Enum.Parse( typeof( ServerType ), colDatas[1] );
                data._tableUrl = colDatas[2];
                data._tableGid = colDatas[3];
                list.Add(data );
            }
            return list;
        }
    }
}
