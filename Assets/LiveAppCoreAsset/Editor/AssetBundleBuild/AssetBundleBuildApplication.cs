using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.Domain;
using UnityEditor;

namespace LiveAppCore.Editor.Application
{
    public class AssetBundleBuildApplication
    {
        private IAssetBundleBuildDomain _domain;
        public AssetBundleBuildApplication( IAssetBundleBuildDomain domain )
        {
            _domain = domain;
        }

        public async UniTask<bool> ExecuteAssetBundleBuild( BuildTargetGroup platform )
        {
            var result = await _domain.PreProcess( platform );
            if(result == false)
            {
                return false;
            }
            result = await _domain.BuildProcess( platform );
            if(result == false)
            {
                return false;
            }
            return await _domain.PostProcess( platform );
        }
    }
}
