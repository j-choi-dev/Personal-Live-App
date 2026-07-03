using Cysharp.Threading.Tasks;
using System;
using UnityEngine;

namespace LiveAppUI.Model
{
    public interface IOAuthTokenModel
    {
        string Token { get; }
        IObservable<bool> OnCompleteTokenProcess { get; }
        UniTask<bool> InitilizeAuthProcess();
    }
}
