using UnityEngine;
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
        [SerializeField] private GameObject _menu = default;

        private CameraMovement _cameraMovement;
        private TerrainModification _terrainModification;
        private PlayerPower _playerPower;
        private PauseMenu _pauseMenu;
        private HUD _hud;
        private InputState _inputState = InputState.InGame;

        // Start is called before the first frame update
        void Start()
        {
            _cameraMovement = GetComponent<CameraMovement>();
            _terrainModification = GetComponent<TerrainModification>();
            _playerPower = GetComponent<PlayerPower>();
            _pauseMenu = _menu.GetComponent<PauseMenu>();
            _hud = _menu.GetComponent<HUD>();
            PauseMenu.PauseGameEvent += ChangeInputState;
        }

        private void OnDestroy()
        {
            PauseMenu.PauseGameEvent -= ChangeInputState;
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
                    UsePowerInput();
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
                _hud.AnimateMenu();
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

        private void UsePowerInput()
        {
            if (Input.GetMouseButtonDown(0))
            {
                _playerPower.UsePower(_terrainModification);
            }
        }

        private void ChangeInputState(bool isPaused)
        {
            _inputState = isPaused ? InputState.InMenu : InputState.InGame;
        }
    }
}