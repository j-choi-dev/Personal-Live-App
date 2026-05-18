using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiveAppCore
{
    public class SceneLoadController : MonoBehaviour
    {
        private const string SCENE_UI = "LiveAppUI";

        private void Awake()
        {
            LoadSceneProcess();
        }

        private void LoadSceneProcess()
        {
#if UNITY_EDITOR
            SceneManager.LoadScene( SCENE_UI, LoadSceneMode.Additive );
#else
            // TODO PC ¹öÀü Scene Load @Choi 26.04.22
#endif
        }
    }
}