using Menus;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Powers;

namespace Player
{
    public class PowerDecalProjection : MonoBehaviour
    {
        [SerializeField] private Camera _camera;
        
        private DecalProjector _decalProjector;
        private PlayerPower _playerPower;

        private float _range;
        private bool _isShown = true;
        
        private void Start()
        {
            _decalProjector = GetComponent<DecalProjector>();
            _playerPower = GetComponentInParent<PlayerPower>();
            
            SetUpDecal(_playerPower.CurrentPower);

            _playerPower.PowerChangeEvent += SetUpDecal;
            PauseMenu.PauseGameEvent += OnMoveDecal;
            
        }

        private void OnDestroy()
        {
            _playerPower.PowerChangeEvent -= SetUpDecal;
            PauseMenu.PauseGameEvent -= OnMoveDecal;
        }

        private void Update()
        {
            DecalMovement();
        }
        
        private void SetUpDecal(Power currentPower)
        {
            _range = currentPower.Range * 2f;
            _decalProjector.size = new Vector3(_range, _range, _range);
            _decalProjector.transform.forward = Vector3.down;
            
            if (currentPower.DecalTexture != null)
            {
                _decalProjector.material.SetTexture("Base_Map", currentPower.DecalTexture);  
                Debug.Log("Decal texture set");
            }
        }

        private void DecalMovement()
        {
            if (!_isShown)
                return;
            
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                _decalProjector.transform.position = hit.point;
            }
        }

        private void OnMoveDecal(bool isPaused)
        {
            _isShown = !isPaused;
            _decalProjector.enabled = !isPaused;
        }
    }
}