using UnityEngine;
using TerrainGen;

namespace Player
{
    public class CameraMovement : MonoBehaviour
    {
        [SerializeField] private float _speed = default;
        [SerializeField] private float _sensitivity = default;
        [SerializeField] private TerrainGeneration _terrain = default;
        [SerializeField] private float _clipRadius = default;

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

            float bounds = _terrain.GetBounds();
            
            Vector3 previousPosition = _transform.position;
            _transform.position += _speed * Time.deltaTime * _movementInput;
            
            if (_transform.position.x > bounds || _transform.position.x < -bounds || _transform.position.z > bounds || _transform.position.z < -bounds)
            {
                _transform.position = previousPosition;
            }
            
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

            if (Physics.SphereCast(ray, _clipRadius, out RaycastHit hit, 30))
            {
                if (hit.distance < _zoomLevel + _clipRadius)
                {
                    _zoomLevel = hit.distance - _clipRadius;
                }
            }
        }
    }
  
}