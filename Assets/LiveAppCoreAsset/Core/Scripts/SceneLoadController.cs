using UnityEngine;
using UnityEngine.SceneManagement;

namespace LiveAppCore
{
    public class SceneLoadController : MonoBehaviour
    {
        private const string SCENE_UI = "LiveAppUI";

        private void Awake()
        {
            SceneManager.LoadScene(SCENE_UI, LoadSceneMode.Additive);
        }
    }
}