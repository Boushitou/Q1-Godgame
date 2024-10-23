using UnityEngine;
using TerrainModif;
using Powers;

namespace Player
{
  public class PlayerController : MonoBehaviour
  {
      [SerializeField] private Power[] _powers = default;
      [SerializeField] private Power _currentPower = default;
      
      private CameraMovement _cameraMovement;
      private TerrainModification _terrainModification;

      // Start is called before the first frame update
      void Start()
      {
          _cameraMovement = GetComponent<CameraMovement>();
          _terrainModification = GetComponent<TerrainModification>();
      }
  
      // Update is called once per frame
      void Update()
      {
          SpellBarInput();
          PauseInput();
          MovementInput();
          ZoomInput();
          UsePower();
      }
  
      private void SpellBarInput()
      {
          if (CustomInputSystem.GetKeyDown("SpellBar"))
          {
              Debug.Log("SpellBar pressed");
          }
      }
      
      private void PauseInput()
      {
          if (CustomInputSystem.GetKeyDown("Pause"))
          {
              Debug.Log("Pause pressed");
          }
      }
  
      private void MovementInput()
      {
          float movementX = CustomInputSystem.GetAxis("Horizontal");
          float movementY = CustomInputSystem.GetAxis("Vertical");
          
          _cameraMovement.Move(movementX, movementY);
      }
  
      private void ZoomInput()
      {
          float zoom = Input.mouseScrollDelta.y;
          
          _cameraMovement.Zoom(zoom);
      }

      private void UsePower()
      {
          if (Input.GetMouseButtonDown(0))
          {
               _currentPower.Invoke(_terrainModification);
          }
      }

      public void SetPower(Power power)
      {
          _currentPower = power;
      }
  }  
}
