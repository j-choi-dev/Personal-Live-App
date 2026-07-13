using Cysharp.Threading.Tasks;
using System.Threading;

namespace LiveAppCore.Google.Domain
{
    public interface INativeSigninDomain
    {
        UniTask<GoogleOAuthToken> RequestAccessTokenAsync( string scope, CancellationToken cancellationToken );

        void SignOut();
    }
}
