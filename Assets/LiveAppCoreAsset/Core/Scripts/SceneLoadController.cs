using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiveAppCore
{
    /// <summary>
    /// Scene ·Î´õ Class
    /// </summary>
    public class SceneLoadController : MonoBehaviour
    {
        private const string SCENE_UI = "LiveAppUI";
        private const string SCENE_UI_PC = "LiveAppUI_PC";

        private void Awake()
        {
            LoadSceneProcess();
        }

        private void LoadSceneProcess()
        {
#if UNITY_EDITOR && UNITY_IPHONE
            SceneManager.LoadScene( SCENE_UI, LoadSceneMode.Additive );
#else
            SceneManager.LoadScene( SCENE_UI_PC, LoadSceneMode.Additive );
#endif
        }
    }
}