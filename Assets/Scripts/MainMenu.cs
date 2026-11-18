using UnityEngine;
using UnityEngine.SceneManagement;

namespace Jsgaona
{
    public class MainMenu : MonoBehaviour
    {
        public void Play()
        {
            SceneManager.LoadScene("Playground");
        }
    }
}