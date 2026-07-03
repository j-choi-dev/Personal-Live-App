using Cysharp.Threading.Tasks;
using System.Threading;

namespace StudioResourceSDK.Domain
{
    public interface IResourceTableLoadDomain
    {
        UniTask<bool> ExistsSheetAndTabAsync(
            string spreadsheetUrlOrId,
            string tabName,
            string token,
            CancellationToken cancellationToken = default
        );

        UniTask<string> LoadVariableRangeAsStringAsync(
            string token,
            string spreadsheetUrlOrId,
            string tabName,
            string columnDelimiter = "\t",
            string rowDelimiter = "\n",
            bool escapeCellLineBreaks = true,
            CancellationToken cancellationToken = default
        );
    }
}
