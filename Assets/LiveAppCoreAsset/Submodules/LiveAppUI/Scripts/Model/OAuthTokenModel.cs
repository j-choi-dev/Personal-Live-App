using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Application;
using System;

namespace LiveAppUI.Model
{
    public class OAuthTokenModel : IOAuthTokenModel
    {
        private IAuthInfoContext _context;

        public IObservable<bool> OnCompleteTokenProcess => _context.OnCompleteTokenProcess;

        public OAuthTokenModel( IAuthInfoContext context )
        {
            _context = context;
        }

        public async UniTask<bool> InitilizeAuthProcess()
        {
            UnityEngine.Debug.LogError( $"[GoogleAuth:C#1.5] OAuthTokenModel entered. contextType={_context?.GetType().FullName ?? "null"}" );

            return await _context.InitilizeAuthProcess();
        }
    }
}
