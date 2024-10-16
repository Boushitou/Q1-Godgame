using UnityEngine;

namespace Player
{
  public class PlayerController : MonoBehaviour
  {
      private CameraMovement _cameraMovement;
      // Start is called before the first frame update
      void Start()
      {
          _cameraMovement = GetComponent<CameraMovement>();
      }
  
      // Update is called once per frame
      void Update()
      {
          SpellBarInput();
          PauseInput();
          MovementInput();
          ZoomInput();
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
  }  
}
