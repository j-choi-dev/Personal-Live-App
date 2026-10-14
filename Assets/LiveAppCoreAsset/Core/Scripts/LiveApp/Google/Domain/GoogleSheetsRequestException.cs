using System;

namespace LiveAppCore.Google.Domain
{
    public sealed class GoogleSheetsRequestException : Exception
    {
        public long StatusCode { get; }
        public string ErrorBody { get; }

        public GoogleSheetsRequestException(
            long statusCode,
            string errorBody)
            : base($"Google Sheets 요청 실패. HTTP={statusCode}")
        {
            StatusCode = statusCode;
            ErrorBody = errorBody;
        }
    }
}