using Cysharp.Threading.Tasks;
using LiveAppCore.Google.Domain;
using SimpleJSON;
using System;
using System.Collections.Generic;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using UnityEngine;
using UnityEngine.Networking;

namespace StudioResourceSDK.Infrastructure
{
    // TODO 삭제대기? @Choi 27.07.03
    public class GoogleSheetCommon
    {
        internal static async UniTask<bool> ExistsSheetAndTabAsync(
            string spreadsheetUrlOrId,
            string tabName,
            string token,
            CancellationToken cancellationToken
        )
        {
            if( string.IsNullOrWhiteSpace( spreadsheetUrlOrId ) ||
                string.IsNullOrWhiteSpace( tabName ) ||
                string.IsNullOrWhiteSpace( token ) )
            {
                return false;
            }
            try
            {
                string spreadsheetId = ExtractSpreadsheetId(spreadsheetUrlOrId);
                string fields = Uri.EscapeDataString("sheets.properties.title");

                string url = $"{OAuthConstValue.SheetsApiBaseUrl}/{Uri.EscapeDataString(spreadsheetId)}?fields={fields}";

                string json = await GetTextWithBearerAsync( url, token, cancellationToken );

                JSONNode root = JSON.Parse(json);
                JSONArray sheets = root["sheets"].AsArray;

                if( sheets == null )
                    return false;

                foreach( JSONNode sheet in sheets )
                {
                    string title = sheet["properties"]["title"].Value;

                    if( string.Equals( title, tabName, StringComparison.Ordinal ) )
                    {
                        return true;
                    }
                }

                return false;
            }
            catch( OperationCanceledException )
            {
                throw;
            }
            catch( Exception e )
            {
                Debug.LogWarning( $"[GoogleSheetsV4OAuthLoader] Exists check failed. {e.Message}" );
                return false;
            }
        }

        internal static async UniTask<string> LoadVariableRangeAsStringAsync(
            string spreadsheetUrlOrId,
            string tabName,
            string token,
            string columnDelimiter,
            string rowDelimiter,
            bool escapeCellLineBreaks,
            CancellationToken cancellationToken
        )
        {
            if( string.IsNullOrWhiteSpace( spreadsheetUrlOrId ) )
            {
                throw new ArgumentException( "spreadsheetUrlOrId is null or empty." );
            }

            if( string.IsNullOrWhiteSpace( tabName ) )
            {
                throw new ArgumentException( "tabName is null or empty." );
            }

            if( string.IsNullOrWhiteSpace( token ) )
            {
                throw new ArgumentException( "token is null or empty." );
            }

            string spreadsheetId = ExtractSpreadsheetId(spreadsheetUrlOrId);
            string range = ToWholeSheetA1Range(tabName);
            string url = $"{OAuthConstValue.SheetsApiBaseUrl}/{Uri.EscapeDataString(spreadsheetId)}/values/{Uri.EscapeDataString(range)}{OAuthConstValue.Dimension}";

            string json = await GetTextWithBearerAsync( url, token, cancellationToken );

            JSONNode root = JSON.Parse(json);
            JSONArray values = root["values"].AsArray;

            if( values == null || values.Count <= 0 )
            {
                return string.Empty;
            }

            List<List<string>> rows = ConvertValuesToRows(values);

            int rowCount = FindRowCountUntilFirstBlankRow(rows);
            if( rowCount <= 0 )
            {
                return string.Empty;
            }

            int columnCount = FindColumnCountUntilFirstBlankColumn(rows, rowCount);
            if( columnCount <= 0 )
            {
                return string.Empty;
            }

            return BuildDelimitedString(
                rows,
                rowCount,
                columnCount,
                columnDelimiter,
                rowDelimiter,
                escapeCellLineBreaks
            );
        }

        private static async UniTask<string> GetTextWithBearerAsync(
            string url,
            string accessToken,
            CancellationToken cancellationToken
        )
        {
            using UnityWebRequest request = UnityWebRequest.Get(url);
            request.timeout = 30;
            request.SetRequestHeader( "Authorization", $"Bearer {accessToken}" );

            await request.SendWebRequest().ToUniTask( cancellationToken: cancellationToken );

            string body = request.downloadHandler != null
                ? request.downloadHandler.text
                : string.Empty;

            if( request.result != UnityWebRequest.Result.Success )
            {
                throw new Exception(
                    $"HTTP Error: {request.responseCode}, Result: {request.result}, Error: {request.error}, Body: {body}"
                );
            }

            return body;
        }

        private static string ExtractSpreadsheetId( string spreadsheetUrlOrId )
        {
            string value = spreadsheetUrlOrId.Trim();

            if( value.Contains( "docs.google.com", StringComparison.OrdinalIgnoreCase ) == false )
            {
                return value;
            }
            Match match = Regex.Match( value, @"/spreadsheets/d/([a-zA-Z0-9-_]+)", RegexOptions.Compiled );

            if( match.Success == false )
            {
                throw new ArgumentException( $"Invalid Google Spreadsheet URL: {spreadsheetUrlOrId}" );
            }

            return match.Groups[1].Value;
        }

        private static string ToWholeSheetA1Range( string tabName )
        {
            string escapedTabName = tabName.Replace("'", "''");
            return $"'{escapedTabName}'";
        }

        private static List<List<string>> ConvertValuesToRows( JSONArray values )
        {
            var rows = new List<List<string>>(values.Count);

            foreach( JSONNode rowNode in values )
            {
                JSONArray rowArray = rowNode.AsArray;
                var row = new List<string>();

                if( rowArray != null )
                {
                    foreach( JSONNode cellNode in rowArray )
                    {
                        row.Add( cellNode?.Value ?? string.Empty );
                    }
                }
                rows.Add( row );
            }
            return rows;
        }

        private static int FindRowCountUntilFirstBlankRow( List<List<string>> rows )
        {
            for( int rowIndex = 0; rowIndex < rows.Count; rowIndex++ )
            {
                if( IsBlankRow( rows[rowIndex] ) )
                    return rowIndex;
            }

            return rows.Count;
        }

        private static bool IsBlankRow( List<string> row )
        {
            if( row == null || row.Count <= 0 )
            {
                return true;
            }

            for( int i = 0; i < row.Count; i++ )
            {
                if( !string.IsNullOrWhiteSpace( row[i] ) )
                {
                    return false;
                }
            }

            return true;
        }

        private static int FindColumnCountUntilFirstBlankColumn(
            List<List<string>> rows,
            int rowCount
        )
        {
            int maxColumnCount = 0;

            for( int rowIndex = 0; rowIndex < rowCount; rowIndex++ )
            {
                if( rows[rowIndex] != null )
                    maxColumnCount = Math.Max( maxColumnCount, rows[rowIndex].Count );
            }

            for( int columnIndex = 0; columnIndex < maxColumnCount; columnIndex++ )
            {
                bool isBlankColumn = true;

                for( int rowIndex = 0; rowIndex < rowCount; rowIndex++ )
                {
                    string value = GetCellValue(rows, rowIndex, columnIndex);

                    if( !string.IsNullOrWhiteSpace( value ) )
                    {
                        isBlankColumn = false;
                        break;
                    }
                }

                if( isBlankColumn )
                    return columnIndex;
            }

            return maxColumnCount;
        }

        private static string BuildDelimitedString(
            List<List<string>> rows,
            int rowCount,
            int columnCount,
            string columnDelimiter,
            string rowDelimiter,
            bool escapeCellLineBreaks
        )
        {
            var builder = new StringBuilder();

            for( int rowIndex = 0; rowIndex < rowCount; rowIndex++ )
            {
                if( rowIndex > 0 )
                    builder.Append( rowDelimiter );

                for( int columnIndex = 0; columnIndex < columnCount; columnIndex++ )
                {
                    if( columnIndex > 0 )
                        builder.Append( columnDelimiter );

                    string value = GetCellValue(rows, rowIndex, columnIndex);
                    value = NormalizeCellValue( value, escapeCellLineBreaks );

                    builder.Append( value );
                }
            }

            return builder.ToString();
        }

        private static string GetCellValue(
            List<List<string>> rows,
            int rowIndex,
            int columnIndex
        )
        {
            if( rows == null )
                return string.Empty;

            if( rowIndex < 0 || rowIndex >= rows.Count )
                return string.Empty;

            List<string> row = rows[rowIndex];

            if( row == null )
                return string.Empty;

            if( columnIndex < 0 || columnIndex >= row.Count )
                return string.Empty;

            return row[columnIndex] ?? string.Empty;
        }

        private static string NormalizeCellValue( string value, bool escapeCellLineBreaks )
        {
            if( string.IsNullOrEmpty( value ) )
                return string.Empty;

            if( !escapeCellLineBreaks )
                return value;

            return value
                .Replace( "\r\n", "\n" )
                .Replace( "\r", "\n" )
                .Replace( "\n", "\\n" )
                .Replace( "\t", "\\t" );
        }
    }
}
