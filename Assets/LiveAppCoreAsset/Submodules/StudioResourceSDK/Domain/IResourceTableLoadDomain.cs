using Cysharp.Threading.Tasks;
using System.Threading;

namespace StudioResourceSDK.Domain
{
    /// <summary>
    /// 리소스 테이블을 읽어들이는 처리 관련 Interface
    /// </summary>
    public interface IResourceTableLoadDomain
    {
        /// <summary>
        /// 시트 및 지정된 탭이 존재하는지 체크
        /// </summary>
        /// <param name="spreadsheetUrlOrId">구글시트 URL</param>
        /// <param name="tabName">구글 시트 Tab 이름</param>
        /// <param name="token">토큰</param>
        /// <param name="cancellationToken">Cancelation Token</param>
        /// <returns>시트 및 지정된 탭의 존재 여부</returns>
        UniTask<bool> ExistsSheetAndTabAsync(
            string spreadsheetUrlOrId,
            string tabName,
            string token,
            CancellationToken cancellationToken = default
        );

        /// <summary>
        /// 구글 시트의 데이터 취득
        /// </summary>
        /// <param name="token">Token</param>
        /// <param name="spreadsheetUrlOrId">구글시트 URL</param>
        /// <param name="tabName">구글 시트 Tab 이름</param>
        /// <param name="columnDelimiter">열 구분 문자</param>
        /// <param name="rowDelimiter">행 구분 문자</param>
        /// <param name="escapeCellLineBreaks">???</param>
        /// <param name="cancellationToken">Cancelation Token</param>
        /// <returns>구글 시트의 CSV값</returns>
        UniTask<string> LoadVariableRangeAsStringAsync(
            string token,
            string spreadsheetUrlOrId,
            string tabName,
            string columnDelimiter = ",",
            string rowDelimiter = "\n",
            bool escapeCellLineBreaks = true,
            CancellationToken cancellationToken = default
        );
    }
}
