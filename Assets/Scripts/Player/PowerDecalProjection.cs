using System;
using UnityEngine;
using UnityEngine.Rendering.Universal;
using Powers;

namespace Player
{
    public class PowerDecalProjection : MonoBehaviour
    {
        private DecalProjector _decalProjector;
        private PlayerPower _playerPower;
        [SerializeField] private Camera _camera;

        private float _range;
        
        private void Start()
        {
            _decalProjector = GetComponent<DecalProjector>();
            _playerPower = GetComponentInParent<PlayerPower>();
            
            SetUpDecal(_playerPower.GetCurrentPower());

            _playerPower.PowerChangeEvent += SetUpDecal;
        }

        private void OnDestroy()
        {
            _playerPower.PowerChangeEvent -= SetUpDecal;
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
            RaycastHit hit;
            Ray ray = _camera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out hit))
            {
                _decalProjector.transform.position = hit.point;
            }
        }
    }
}