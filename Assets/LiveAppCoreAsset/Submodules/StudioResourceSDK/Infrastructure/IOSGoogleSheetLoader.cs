using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Infrastructure;
using StudioResourceSDK.Domain;
using System.Threading;

namespace StudioResourceSDK.Infrastructure
{
    /// <summary>
    /// 리소스 테이블을 읽어들이는 처리 관련 iOS 구현체 클래스
    /// </summary>
    public class iOSGoogleSheetLoader : IResourceTableLoadDomain
    {
        public UniTask<bool> ExistsSheetAndTabAsync(
            string spreadsheetUrlOrId,
            string tabName,
            string token,
            CancellationToken cancellationToken = default
        )
        {
            return GoogleSheetsV4OAuthLoaderCore.ExistsSheetAndTabAsync(
                spreadsheetUrlOrId,
                tabName,
                token,
                cancellationToken
            );
        }

        public UniTask<string> LoadVariableRangeAsStringAsync(
            string token,
            string spreadsheetUrlOrId,
            string tabName,
            string columnDelimiter = ",",
            string rowDelimiter = "\n",
            bool escapeCellLineBreaks = true,
            CancellationToken cancellationToken = default
        )
        {
            return GoogleSheetsV4OAuthLoaderCore.LoadVariableRangeAsStringAsync(
                spreadsheetUrlOrId,
                tabName,
                token,
                columnDelimiter,
                rowDelimiter,
                escapeCellLineBreaks,
                cancellationToken
            );
        }
    }
}
