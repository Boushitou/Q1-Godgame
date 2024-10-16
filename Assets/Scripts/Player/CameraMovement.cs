using System;
using UnityEngine;

namespace Player
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = default;
        [SerializeField] private float _sensitivity = default;

        private float _zoomLevel = 0f;
        private float _zoomPosition = 0f;
        private float _maxZoom = 10f;
        private float _minZoom = -30f;

        private Transform _transform;
        private Vector3 _movementInput ;

        private Transform _cam;

        private void Start()
        {
            _cam = transform.GetChild(0);
            _transform = transform;
        }

        public void Move(float x, float z)
        {
            _movementInput.x = x;
            _movementInput.z = z;

            _transform.position += _speed * Time.deltaTime * _movementInput;
            
            ClipCheck();
        }

        public void Zoom(float zoom)
        {
            _zoomLevel += zoom * _sensitivity;
            
            if (_zoomLevel < _minZoom)
            {
                _zoomLevel = _minZoom;
            }
        
            _zoomPosition = Mathf.MoveTowards(_zoomPosition, _zoomLevel, _speed * Time.deltaTime);
        
            _cam.position = _transform.position + (_cam.forward * _zoomPosition);
        }

        private void ClipCheck()
        {
            Ray ray = new Ray(_transform.position, _cam.forward);

            if (Physics.SphereCast(ray, 3, out RaycastHit hit, 30))
            {
                if (hit.distance < _zoomLevel + 3)
                {
                    _zoomLevel = hit.distance - 3;
                }
            }
        }
    }
  
}