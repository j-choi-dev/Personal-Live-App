using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.Domain;
using UnityEditor;
using UnityEngine;

namespace LiveAppCore.Editor.Application
{
    public class RomBuildApplication
    {
        private IRomBuildDomain _domain;
        public RomBuildApplication( IRomBuildDomain domain )
        {
            _domain = domain;
        }

        public async UniTask<bool> ExecuteRomBuild( BuildTargetGroup platform )
        {
            var result = await _domain.PreProcess( platform );
            if( result == false )
            {
                return false;
            }
            result = await _domain.BuildProcess( platform );
            if( result == false )
            {
                return false;
            }
            return await _domain.PostProcess( platform );
        }
    }
}
