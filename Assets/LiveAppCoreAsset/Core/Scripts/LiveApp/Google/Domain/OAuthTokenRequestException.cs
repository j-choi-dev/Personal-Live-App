using System;

namespace LiveAppCore.Google.Domain
{
    public sealed class OAuthTokenRequestException : Exception
    {
        public long StatusCode { get; }
        public string ErrorCode { get; }

        public OAuthTokenRequestException(
            long statusCode,
            string errorCode)
            : base(
                $"Google 토큰 요청 실패. HTTP={statusCode}, Error={errorCode}")
        {
            StatusCode = statusCode;
            ErrorCode = errorCode;
        }
    }
}