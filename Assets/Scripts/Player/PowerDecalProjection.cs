using UnityEngine;
using UnityEngine.Rendering.Universal;
using Powers;

namespace Player
{
    public class PowerDecalProjection : MonoBehaviour
    {
        private DecalProjector _decalProjector;
        private PlayerController _playerController;
        [SerializeField] private Camera _camera;

        private float _range;

        private void Start()
        {
            _decalProjector = GetComponent<DecalProjector>();
            _playerController = GetComponentInParent<PlayerController>();
            
            SetUpDecal(_playerController.GetCurrentPower(), Vector3.up);
        }
        
        private void Update()
        {
            DecalMovement();
        }
        
        private void SetUpDecal(Power currentPower, Vector3 forward)
        {
            _range = currentPower.Range * 2f;
            _decalProjector.size = new Vector3(_range, _range, _range);
            _decalProjector.transform.forward = forward;
        }

        private void DecalMovement()
        {
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                _decalProjector.transform.position = hit.point;
            }
        }
    }
}