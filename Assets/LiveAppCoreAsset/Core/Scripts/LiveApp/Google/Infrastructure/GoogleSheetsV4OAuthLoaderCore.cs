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

namespace LiveAppCore.Google.Infrastructure
{
    /// <summary>
    /// 구글 시트 요소에 대한 접근을 처리하는 공통 로직
    /// </summary>
    internal static class GoogleSheetsV4OAuthLoaderCore
    {
        /// <summary>
        /// 해당 이름의 시트 탭이 존재하는지 체크
        /// </summary>
        /// <param name="spreadsheetUrlOrId">시트 ID</param>
        /// <param name="tabName">탭 이름</param>
        /// <param name="cancellationToken">캔슬 토큰</param>
        /// <returns>시트 유무 여부</returns>
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
                var spreadsheetId = ExtractSpreadsheetId(spreadsheetUrlOrId);
                var fields = Uri.EscapeDataString("sheets.properties.title");
                var url = $"{OAuthConstValue.SheetsApiBaseUrl}/{Uri.EscapeDataString(spreadsheetId)}?fields={fields}";

                string json = await GetTextWithBearerAsync( url, token, cancellationToken );
                Debug.Log( $"json = {json}" );

                var root = JSON.Parse(json);
                var sheets = root["sheets"].AsArray;

                if( sheets == null )
                {
                    return false;
                }

                foreach( JSONNode sheet in sheets )
                {
                    var title = sheet["properties"]["title"].Value;

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
                Debug.LogError( $"[GoogleSheetsV4OAuthLoader] Exists check failed. {e.Message}" );
                return false;
            }
        }

        /// <summary>
        /// 가변적인 범위로 시트 불러오기
        /// </summary>
        /// <param name="spreadsheetUrlOrId">시트 ID</param>
        /// <param name="tabName">탭 이름</param>
        /// <param name="token">토큰 정보</param>
        /// <param name="columnDelimiter">열 구분 문자</param>
        /// <param name="rowDelimiter">행 구분 문자</param>
        /// <param name="escapeCellLineBreaks"></param>
        /// <param name="cancellationToken">캔슬 토큰</param>
        /// <returns></returns>
        /// <exception cref="ArgumentException"></exception>
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
                throw new ArgumentException( "tabName is null or empty." );
            }

            var spreadsheetId = ExtractSpreadsheetId(spreadsheetUrlOrId);
            var range = ToWholeSheetA1Range(tabName);
            var url = $"{OAuthConstValue.SheetsApiBaseUrl}/{Uri.EscapeDataString(spreadsheetId)}/values/{Uri.EscapeDataString(range)}{OAuthConstValue.Dimension}";
            var json = await GetTextWithBearerAsync( url, token, cancellationToken );
            var root = JSON.Parse(json);
            var values = root["values"].AsArray;

            if( values == null || values.Count <= 0 )
            {
                return string.Empty;
            }

            var rows = ConvertValuesToRows(values);

            var rowCount = FindRowCountUntilFirstBlankRow(rows);
            if( rowCount <= 0 )
            {
                return string.Empty;
            }

            var columnCount = FindColumnCountUntilFirstBlankColumn(rows, rowCount);
            if( columnCount <= 0 )
            {
                return string.Empty;
            }

            return BuildDelimitedString( rows, rowCount, columnCount, columnDelimiter, rowDelimiter, escapeCellLineBreaks );
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

            var body = request.downloadHandler != null
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
            var value = spreadsheetUrlOrId.Trim();

            if( !value.Contains( "docs.google.com", StringComparison.OrdinalIgnoreCase ) )
            {
                return value;
            }

            var match = Regex.Match( value, @"/spreadsheets/d/([a-zA-Z0-9-_]+)", RegexOptions.Compiled );

            if( !match.Success )
            {
                throw new ArgumentException( $"Invalid Google Spreadsheet URL: {spreadsheetUrlOrId}" );
            }

            return match.Groups[1].Value;
        }

        private static string ToWholeSheetA1Range( string tabName )
        {
            var escapedTabName = tabName.Replace("'", "''");
            return $"'{escapedTabName}'";
        }

        private static List<List<string>> ConvertValuesToRows( JSONArray values )
        {
            var rows = new List<List<string>>(values.Count);

            foreach( JSONNode rowNode in values )
            {
                var rowArray = rowNode.AsArray;
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
                {
                    return rowIndex;
                }
            }
            return rows.Count;
        }

        private static bool IsBlankRow( List<string> row )
        {
            if( row == null || row.Count <= 0 )
            {
                return false;
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
                {
                    maxColumnCount = Math.Max( maxColumnCount, rows[rowIndex].Count );
                }
            }

            for( int columnIndex = 0; columnIndex < maxColumnCount; columnIndex++ )
            {
                var isBlankColumn = true;
                for( int rowIndex = 0; rowIndex < rowCount; rowIndex++ )
                {
                    var value = GetCellValue(rows, rowIndex, columnIndex);
                    if( string.IsNullOrWhiteSpace( value ) == false )
                    {
                        isBlankColumn = false;
                        break;
                    }
                }

                if( isBlankColumn )
                {
                    return columnIndex;
                }
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
                {
                    builder.Append( rowDelimiter );
                }

                for( int columnIndex = 0; columnIndex < columnCount; columnIndex++ )
                {
                    if( columnIndex > 0 )
                    {
                        builder.Append( columnDelimiter );
                    }
                    var value = GetCellValue(rows, rowIndex, columnIndex);
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
            {
                return string.Empty;
            }

            if( rowIndex < 0 || rowIndex >= rows.Count )
            {
                return string.Empty;
            }

            var row = rows[rowIndex];

            if( row == null )
            {
                return string.Empty;
            }

            if( columnIndex < 0 || columnIndex >= row.Count )
            {
                return string.Empty;
            }

            return row[columnIndex] ?? string.Empty;
        }

        private static string NormalizeCellValue( string value, bool escapeCellLineBreaks )
        {
            if( string.IsNullOrEmpty( value ) )
            {
                return string.Empty;
            }

            if( !escapeCellLineBreaks )
            {
                return value;
            }

            return value
                .Replace( "\r\n", "\n" )
                .Replace( "\r", "\n" )
                .Replace( "\n", "\\n" )
                .Replace( "\t", "\\t" );
        }
    }
}