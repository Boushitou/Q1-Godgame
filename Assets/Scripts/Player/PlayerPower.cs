using System;
using UnityEngine;
using Powers;
using Menus;

namespace Player
{
    public class PlayerPower : MonoBehaviour
    {
        [SerializeField] private HUD _hud = default;
        [SerializeField] private Power[] _powers = default;
        [SerializeField] private Power _currentPower = default;
        public Power CurrentPower { get => _currentPower; set => _currentPower = value; }
        
        private readonly int _totalFaith = 100;
        private int _currentFaith;
        
        public delegate void OnPowerChange(Power power);
        public event OnPowerChange PowerChangeEvent;

        private void Start()
        {
            _currentFaith = _totalFaith;
        }

        public void UsePower(TerrainModification terrainModification)
        {
            if (_currentFaith < _currentPower.FaithCost)
                return;
                
            bool powerSuccessful = _currentPower.Invoke(terrainModification);

            if (!powerSuccessful)
                return;
                
            _currentFaith = _currentFaith - _currentPower.FaithCost < 0 ? 0 : _currentFaith - _currentPower.FaithCost;    
                
            _hud.UpdateFaithBar(_currentFaith);
        }
        
        public void SetPower(Power power)
        {
            _currentPower = power;
            PowerChangeEvent?.Invoke(_currentPower);
        }
    }
  
}