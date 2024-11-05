using UnityEngine;
using TerrainModif;
using Powers;
using Menus;

namespace Player
{
    public enum InputState
    {
        InGame,
        InMenu
    }

    [RequireComponent(typeof(CameraMovement), typeof(TerrainModification))]
    public class PlayerController : MonoBehaviour
    {
        [SerializeField] private Power[] _powers = default;
        [SerializeField] private Power _currentPower = default;
        [SerializeField] private PauseMenu _pauseMenu = default;
        [SerializeField] private HUD _hud = default;

        private CameraMovement _cameraMovement;
        private TerrainModification _terrainModification;
        private InputState _inputState = InputState.InGame;

        private readonly int _totalFaith = 100;
        private int _currentFaith;

        // Start is called before the first frame update
        void Start()
        {
            _currentFaith = _totalFaith;
            
            _cameraMovement = GetComponent<CameraMovement>();
            _terrainModification = GetComponent<TerrainModification>();
            _pauseMenu.PauseGameEvent += ChangeInputState;
        }

        private void OnDestroy()
        {
            _pauseMenu.PauseGameEvent -= ChangeInputState;
        }

        // Update is called once per frame
        void Update()
        {
            switch (_inputState)
            {
                case InputState.InGame:
                    SpellBarInput();
                    PauseInput();
                    MovementInput();
                    ZoomInput();
                    UsePower();
                    break;
                case InputState.InMenu:
                    PauseInput();
                    break;
                default:
                    Debug.Log("No input state");
                    break;
            }
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
                if (!_pauseMenu.IsPaused)
                {
                    _pauseMenu.PauseGame();
                    _inputState = InputState.InMenu;
                }
                else
                {
                    _pauseMenu.UnPauseGame();
                    _inputState = InputState.InGame;
                }
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
                if (_currentFaith < _currentPower.FaithCost)
                    return;
                
                bool powerSuccessful = _currentPower.Invoke(_terrainModification);

                if (!powerSuccessful)
                    return;
                
                _currentFaith = _currentFaith - _currentPower.FaithCost < 0 ? 0 : _currentFaith - _currentPower.FaithCost;    
                
                _hud.UpdateFaithBar(_currentFaith);
            }
        }

        public void SetPower(Power power)
        {
            _currentPower = power;
        }

        private void ChangeInputState(bool isPaused)
        {
            _inputState = isPaused ? InputState.InMenu : InputState.InGame;
        }
    }
}