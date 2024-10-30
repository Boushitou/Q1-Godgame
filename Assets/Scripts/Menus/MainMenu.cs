using TMPro;
using UnityEngine;
using Data;

namespace Menus
{
    public class MainMenu : Menu
    {
        [SerializeField] private TMP_InputField _seedInput = default;

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

            LoadScene("GameScene");
        }
        
        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Application Quit");
        }
    }  
}
