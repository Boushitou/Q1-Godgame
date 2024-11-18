using System;
using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Menus
{
    public class HUD : Menu
    {
        [Header("HUD Elements")]
        [SerializeField] private TextMeshProUGUI _fpsCounter = default;
        [SerializeField] private Image _faithBar = default;
        [SerializeField] private Image _powerIcon = default;
        [SerializeField] private GameObject _powersBar = default;
        [SerializeField] private GameObject _hint = default;
        
        [Header("Power Bar Animation")]
        [SerializeField] private Vector3 _powerBarShownPosition = default;
        [SerializeField] private Vector3 _powerBarHiddenPosition = default;
        
        //Framerate counter logic
        private readonly float _updateInterval = 0.5f;
        private float _timeSinceLastUpdate = 0.0f;
        private int _frameCount = 0;
        
        //Power bar animation logic
        private Coroutine _powerBarCoroutine;
        private bool _barShown = false;

        private void Start()
        {
            _powersBar.transform.localPosition = _powerBarShownPosition;
            _powerBarCoroutine = StartCoroutine(AnimatePowerBar(_powerBarHiddenPosition));
            _hint.SetActive(true);
        }

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
        
        public void UpdatePowerIcon(Sprite powerIcon)
        {
            _powerIcon.sprite = powerIcon;
        }

        public void AnimateMenu()
        {
            if (_powerBarCoroutine != null)
                StopCoroutine(_powerBarCoroutine);
            
            _powerBarCoroutine = StartCoroutine(_barShown ? AnimatePowerBar(_powerBarHiddenPosition) : AnimatePowerBar(_powerBarShownPosition));
            
            _barShown = !_barShown;
            _hint.SetActive(false);
        }
        
        private IEnumerator AnimatePowerBar(Vector3 targetPosition)
        {
            float time = 0f;
            float duration = 0.3f;
            Vector3 startPosition = _powersBar.transform.localPosition;

            while (time < duration)
            {
                time += Time.unscaledDeltaTime;
                _powersBar.transform.localPosition = Vector3.Lerp(startPosition, targetPosition, time / duration);
                yield return null;
            }
        }
    }
}