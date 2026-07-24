using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiveAppCore
{
    /// <summary>
    /// Scene 로더 Class
    /// </summary>
    public class SceneLoadController : MonoBehaviour
    {
        private const string SCENE_UI = "LiveAppUI";

        private void Awake()
        {
            LoadSceneProcess();
        }

        private void LoadSceneProcess()
        {
#if UNITY_EDITOR || UNITY_IPHONE
            SceneManager.LoadScene( SCENE_UI, LoadSceneMode.Additive );
#else
            
            Debug.Log("// TODO PC 버전 Scene Load @Choi 26.04.22");
#endif
        }
    }
}