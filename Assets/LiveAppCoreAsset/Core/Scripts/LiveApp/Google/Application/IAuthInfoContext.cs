using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using System;

namespace LiveAppCore.Google.Application
{
    public interface IAuthInfoContext
    {
        string Token { get; }
        IObservable<bool> OnCompleteTokenProcess { get; }
        UniTask<bool> InitilizeAuthProcess();
    }
}
