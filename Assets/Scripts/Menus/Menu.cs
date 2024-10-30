using System;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace Menus
{
    public abstract class Menu : MonoBehaviour
    {
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
    }  
}
