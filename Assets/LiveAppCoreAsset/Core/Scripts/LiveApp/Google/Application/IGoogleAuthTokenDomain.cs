using Cysharp.Threading.Tasks;
using System.Threading;
using UnityEngine;

namespace LiveAppCore.Google.Domain
{
    public interface IGoogleAuthTokenDomain
    {
        string Token { get; }
        void SetAuthValue( GoogleOAuthSettings settings );
        UniTask<string> GetAccessTokenAsync( CancellationToken cancellationToken = default );
    }
}