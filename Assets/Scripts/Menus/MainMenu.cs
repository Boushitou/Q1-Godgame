using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using Data;

namespace Menus
{
    public class MainMenu : MonoBehaviour
    {
        [SerializeField] private TMP_InputField _seedInput = default;
        
        public void OpenMenu(GameObject menu)
        {
            menu.SetActive(true);
        }
        
        public void CloseMenu(GameObject menu)
        {
            menu.SetActive(false);
        }

        public void LoadScene(string sceneName)
        {
            SceneManager.LoadScene(sceneName);
            // TODO : Add loading screen
        }

        public void CreateWorld()
        {
            if (uint.TryParse(_seedInput.text, out uint seed) && _seedInput.gameObject.activeSelf)
            {
                GameSettings.Seed = seed;
            }
            else
            {
                GameSettings.Seed = (uint)Random.Range(0, int.MaxValue);
            }

            SceneManager.LoadScene("GameScene");
        }
        
        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Application Quit");
        }
    }  
}
