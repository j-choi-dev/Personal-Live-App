using Cysharp.Threading.Tasks;
using LiveAppCore.Editor.Domain;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEditor;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace LiveAppCore.Editor.Infrastructure
{
    public class iOSRomBuilder : IRomBuildDomain
    {
        public async UniTask<bool> BuildProcess( BuildTargetGroup platform )
        {
            throw new System.NotImplementedException();
        }

        public async UniTask<bool> PostProcess( BuildTargetGroup platform )
        {
            throw new System.NotImplementedException();
        }

        public async UniTask<bool> PreProcess( BuildTargetGroup platform )
        {
            throw new System.NotImplementedException();
        }
    }
}
