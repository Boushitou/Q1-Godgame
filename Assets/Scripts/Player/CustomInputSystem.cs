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
            if (_keyMapping.ContainsKey(action))
            {
                return Input.GetKeyDown(_keyMapping[action]);
            }
        
            return false;
        }

        public static float GetAxis(string action)
        {
            if (_axisMapping.ContainsKey(action))
            {
                return Input.GetAxis(_axisMapping[action]);
            }

            return 0f;
        }
    }  
}
