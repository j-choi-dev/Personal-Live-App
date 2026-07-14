using Cysharp.Threading.Tasks;
using UnityEngine;
using LiveAppCore.Editor.Domain;
using UnityEditor;

namespace LiveAppCore.Editor.Application
{
    /// <summary>
    /// Rom Build 처리 순서를 정의/실행하는 Application층
    /// </summary>
    public class RomBuildContext
    {
        private IRomBuildDomain _domain;

        /// <summary>
        /// 생성자
        /// </summary>
        /// <param name="domain">IF를 상속받은 Platform별 빌드 구현 클래스</param>
        public RomBuildContext( IRomBuildDomain domain )
        {
            _domain = domain;
        }

        /// <summary>
        /// ROM Build 처리를 수행
        /// </summary>
        /// <param name="platform">Build Target Group</param>
        /// <returns>성공/실패</returns>
        public async UniTask<bool> ExecuteRomBuild( BuildTargetGroup platform )
        {
            var result = await _domain.PreProcess( platform );

            Debug.Log( $"[iOSRomBuilder] PreProcess {result}." );
            if( result == false )
            {
                return false;
            }
            result = await _domain.BuildProcess( platform );
            Debug.Log( $"[iOSRomBuilder] BuildProcess {result}." );
            if( result == false )
            {
                return false;
            }
            return await _domain.PostProcess( platform );
        }
    }
}
