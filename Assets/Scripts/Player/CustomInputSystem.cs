using System.Collections.Generic;
using UnityEngine;

namespace Player
{
    public class CustomInputSystem : MonoBehaviour
    {
        private static Dictionary<string, KeyCode> _keyMapping = new Dictionary<string, KeyCode>();
        private static Dictionary<string, string> _axisMapping = new Dictionary<string, string>();

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            InitializeDefaultKeys();
            InitializeDefaultAxis();
        }

        private void InitializeDefaultKeys()
        {
            _keyMapping["SpellBar"] = KeyCode.Space;
            _keyMapping["Pause"] = KeyCode.Escape;
            _keyMapping["Fire"] = KeyCode.Mouse0;
        }
    
        private void InitializeDefaultAxis()
        {
            _axisMapping["Horizontal"] = "Horizontal";
            _axisMapping["Vertical"] = "Vertical";
        }

        public static bool GetKeyDown(string action)
        {
            if (_keyMapping.TryGetValue(action, value: out var value))
            {
                return Input.GetKeyDown(value);
            }
        
            return false;
        }

        public static float GetAxis(string action)
        {
            if (_axisMapping.TryGetValue(action, out var value))
            {
                return Input.GetAxis(value);
            }

            return 0f;
        }
    }  
}
