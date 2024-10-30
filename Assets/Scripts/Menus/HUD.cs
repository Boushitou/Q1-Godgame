using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class HUD : Menu
    {
        [SerializeField] private TextMeshProUGUI _fpsCounter = default;
        [SerializeField] private Image _faithBar = default;
        
        //Framerate counter logic
        private readonly float _updateInterval = 0.5f;
        private float _timeSinceLastUpdate = 0.0f;
        private int _frameCount = 0;
        
        private void Update()
        {
            _timeSinceLastUpdate += Time.unscaledDeltaTime;
            _frameCount++;

            if (_timeSinceLastUpdate >= _updateInterval)
            {
                UpdateFpsCounter();
                _timeSinceLastUpdate = 0.0f;
                _frameCount = 0;
            }
        }

        private void UpdateFpsCounter()
        {
            float fps = _frameCount / _timeSinceLastUpdate;
            _fpsCounter.text = "FPS: " + Mathf.RoundToInt(fps);
        }

        public void UpdateFaithBar(int amount)
        {
            _faithBar.fillAmount = amount / 100f;
        }
    }
}