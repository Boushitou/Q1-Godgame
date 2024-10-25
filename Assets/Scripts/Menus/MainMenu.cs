using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Menus
{
    public class MainMenu : MonoBehaviour
    {
        public void OpenMenu(GameObject menu)
        {
            menu.SetActive(true);
        }
        
        public void CloseMenu(GameObject menu)
        {
            menu.SetActive(false);
        }
        
        public void QuitGame()
        {
            Application.Quit();
            Debug.Log("Application Quit");
        }
    }  
}
