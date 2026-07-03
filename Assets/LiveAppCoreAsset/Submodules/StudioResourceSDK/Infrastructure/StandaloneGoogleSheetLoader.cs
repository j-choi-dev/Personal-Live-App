using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using LiveAppCore.Google.Infrastructure;
using StudioResourceSDK.Domain;
using System.Threading;
using Unity.VisualScripting.Antlr3.Runtime;

namespace StudioResourceSDK.Infrastructure
{
    public class StandaloneGoogleSheetLoader : IResourceTableLoadDomain
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
            string columnDelimiter = "\t",
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
