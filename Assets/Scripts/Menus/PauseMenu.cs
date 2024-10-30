using TMPro;
using UnityEngine;
using Data;

namespace Menus
{
    public class PauseMenu : Menu
    {
        [SerializeField] private GameObject _pauseMenu = default;
        [SerializeField] private GameObject _hud = default;
        [SerializeField] private TextMeshProUGUI _seedText = default;

        public bool IsPaused = false;

        public delegate void OnPauseGame(bool isPaused);
        public event OnPauseGame PauseGameEvent;

        private void Start()
        {
            _pauseMenu.SetActive(false); 
            _seedText.text = $"Seed: {GameSettings.Seed}";
        }

        private void OnDisable()
        {
            Time.timeScale = 1f;
            IsPaused = false;
            PauseGameEvent?.Invoke(IsPaused);
        }

        public void PauseGame()
        {
            OpenMenu(_pauseMenu);
            CloseMenu(_hud);
            Time.timeScale = 0f;
            IsPaused = true;
            PauseGameEvent?.Invoke(IsPaused);
        }

        public void UnPauseGame()
        {
            CloseMenu(_pauseMenu);
            OpenMenu(_hud);
            Time.timeScale = 1f;
            IsPaused = false;
            PauseGameEvent?.Invoke(IsPaused);
        }
    }
}