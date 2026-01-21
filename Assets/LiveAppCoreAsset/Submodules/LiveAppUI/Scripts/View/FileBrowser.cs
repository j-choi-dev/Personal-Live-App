using Cysharp.Threading.Tasks;
using SFB;
using System.IO;
using LiveAppCore.Domain;
using LiveAppCore.Preference;

namespace LiveAppUI.View
{
    public class FileBrowser : IFileBrowser
    {
        PreferenceParser m_Parser = new PreferenceParser( "FileBrowser" );

        private string GenPrefKey( string title )
        {
            return $"FileBrowser/{title}";
        }

        public UniTask<string> GetLoadPath( string title, string defaultPath, string[] extensions )
        {
            if( string.IsNullOrEmpty( defaultPath ) )
            {
                defaultPath = GenPrefKey( title );
            }

            return UniTask.Run( () =>
            {
                var dirName = Path.GetDirectoryName( defaultPath );
                var files = StandaloneFileBrowser.OpenFilePanel( title, dirName, ConvertToFilter( extensions ), false );
                if( files != null && files.Length == 1 )
                {
                    var path = files[0].Replace( "\\", "/" );
                    m_Parser.SaveString( GenPrefKey( title ), path );
                    return path;
                }
                else
                {
                    return null;
                }
            } );
        }

        public UniTask<string[]> GetLoadPathList( string title, string defaultPath, string[] extensions )
        {
            if( string.IsNullOrEmpty( defaultPath ) )
            {
                defaultPath = GenPrefKey( title );
            }

            return UniTask.Run( () =>
            {
                var dirName = Path.GetDirectoryName( defaultPath );
                var files = StandaloneFileBrowser.OpenFilePanel( title, dirName, ConvertToFilter( extensions ), true );
                for( int i = 0; i < files.Length; i++ )
                {
                    files[i] = files[i].Replace( "\\", "/" );
                }

                return files;
            } );
        }

        public UniTask<string> GetFolderPath( string title, string defaultPath )
        {
            if( string.IsNullOrEmpty( defaultPath ) )
            {
                defaultPath = m_Parser.GetString( GenPrefKey( title ), defaultPath );
            }

            return UniTask.Run( async () =>
            {
                string[] path;
                if( string.IsNullOrEmpty( defaultPath ) )
                {
                    path = StandaloneFileBrowser.OpenFolderPanel( title, defaultPath, false );
                }
                else
                {
                    var dirName = Path.GetDirectoryName( defaultPath );
                    path = StandaloneFileBrowser.OpenFolderPanel( title, dirName, false );
                }

                if( path == null || path.Length == 0 || string.IsNullOrEmpty( path[0] ) )
                {
                    return null;
                }

                path[0] = path[0].Replace( "\\", "/" );
                await m_Parser.SaveString( GenPrefKey( title ), path[0] );
                return path[0];
            } );
        }

        public UniTask<string> GetSavePath( string title, string defaultPath, string[] extensions )
        {
            if( string.IsNullOrEmpty( defaultPath ) )
            {
                defaultPath = m_Parser.GetString( GenPrefKey( title ), defaultPath );
            }

            return UniTask.Run( async () =>
            {
                string path;
                if( string.IsNullOrEmpty( defaultPath ) )
                {
                    path = StandaloneFileBrowser.SaveFilePanel( title, "", "", ConvertToFilter( extensions ) );
                }
                else
                {
                    var dirName = Path.GetDirectoryName( defaultPath );
                    var fileName = Path.GetFileName( defaultPath );
                    path = StandaloneFileBrowser.SaveFilePanel( title, dirName, fileName, ConvertToFilter( extensions ) );
                }

                if( string.IsNullOrEmpty( path ) )
                {
                    return null;
                }

                path = path.Replace( "\\", "/" );
                var saveDirName = Path.GetDirectoryName( path );
                await m_Parser.SaveString( GenPrefKey( title ), saveDirName );
                return path;
            } );
        }

        private ExtensionFilter[] ConvertToFilter( string[] extensions )
        {
            return new[] { new ExtensionFilter( string.Join( ", ", extensions ), extensions ) };
        }
    }
}
