using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets._Scripts.Xbox
{
    /// <summary>
    /// Responsible for managing game pad input on the main menu
    /// </summary>
    public class MainMenuControllerHandler : MonoBehaviour
    {
        public GameObject NewGameGameObject;
        public GameObject ContinueGameObject;
        public GameObject ExitGameObject;
        public GameObject SettingsButtonGameObject;
        public GameObject SteamWishListGameObject;
        public GameObject DiscordGameObject;

        private bool IsNewGameButtonActive => _activeMainMenuButton.gameObject.Equals(NewGameGameObject);
        private bool IsSettingsButtonActive => _activeMainMenuButton.gameObject.Equals(SettingsButtonGameObject);
        private GameObject[] VerticalButtons => new[] { NewGameGameObject, ContinueGameObject, ExitGameObject };
        private GameObject[] HorizontalButtons => new[] { SettingsButtonGameObject, SteamWishListGameObject, DiscordGameObject };

        private Button _activeMainMenuButton;
        private ColorBlock _defaultMainMenuColorBlock;
        private ColorBlock _activeMainMenuColorBlock;
        private bool _skipFrame;
        private bool _hasConfiguredForController;
        private int _currentVerticalButtonIndex = -1;
        private int _currentHorizontalButtonIndex = -1;
        private int _verticalButtonsLength;
        private int _horizontalButtonCount;

        // Start is called before the first frame update
        void Start()
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged += HandleInputMethodChanged;

            _verticalButtonsLength = GameSettings.Instance.IsUsingController ? 2 : VerticalButtons.Length;
            _horizontalButtonCount = GameSettings.Instance.IsUsingController ? 1 : HorizontalButtons.Length;

            if (GameSettings.Instance.IsXboxMode)
            {
                _currentVerticalButtonIndex = 0;
                ExitGameObject.SetActive(false);
                SteamWishListGameObject.SetActive(false);
                DiscordGameObject.SetActive(false);

                SettingsButtonGameObject.transform.localPosition = new Vector3(
                    DiscordGameObject.transform.localPosition.x,
                    DiscordGameObject.transform.localPosition.y);
            }
        }

        public void OnDisable()
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged -= HandleInputMethodChanged;
        }

        private void HandleInputMethodChanged(bool isUsingController)
        {
            if(isUsingController)
            {
                _skipFrame = true;
                _currentVerticalButtonIndex = 0;
            }
            else
            {
            }
        }

        public void TryApplyControllerHover()
        {
            if (!_hasConfiguredForController && GameSettings.Instance.IsUsingController)
            {
                _activeMainMenuButton = NewGameGameObject.GetComponent<Button>();
                _defaultMainMenuColorBlock = _activeMainMenuButton.colors;
                _activeMainMenuColorBlock = _defaultMainMenuColorBlock;
                _activeMainMenuColorBlock.normalColor = _defaultMainMenuColorBlock.highlightedColor;
                _activeMainMenuButton.colors = _activeMainMenuColorBlock;

                _hasConfiguredForController = true;
            }

            if (_hasConfiguredForController && !GameSettings.Instance.IsUsingController)
            {
                _activeMainMenuButton.colors = _defaultMainMenuColorBlock;
                _hasConfiguredForController = false;
            }
        }

        // Update is called once per frame
        void Update()
        {
            TryApplyControllerHover();
            
            if (Gamepad.current == null || !GameSettings.Instance.IsUsingController ||
                PauseMenu.Instance.active || UI.Instance.TutorialPopupQuestion.activeInHierarchy) return;

            if(_skipFrame)
            {
                _skipFrame = false;
                return;
            }

            var pressedButton = GamePadController.GetButton();
            if (pressedButton.Button == GamePadButton.South && pressedButton.Control.WasPressedThisFrame)
            {
                _activeMainMenuButton.onClick.Invoke();
            }
            else
            {
                HandleControllerNavigation();
            }
        }

        private void HandleControllerNavigation()
        {
            var continueGameAvailable = UI.Instance.ContinueBtn.interactable;
            var pressedDirection = GamePadController.GetDirection();
            if (!pressedDirection.Control.WasPressedThisFrame) return;

            if (pressedDirection.Input is DirectionInput.Up or DirectionInput.Down)
            {
                var buttonIndex = pressedDirection.Input switch
                {
                    DirectionInput.Up => _currentVerticalButtonIndex - 1,
                    DirectionInput.Down => _currentVerticalButtonIndex + 1,
                    _ => _currentVerticalButtonIndex //impossible to get here
                };

                _currentHorizontalButtonIndex = -1;
                _currentVerticalButtonIndex = buttonIndex % _verticalButtonsLength;

                if (_currentVerticalButtonIndex < 0)
                {
                    _currentVerticalButtonIndex = _verticalButtonsLength - 1;
                }

                // Skip the continue button if it's not available
                if (!continueGameAvailable && _currentVerticalButtonIndex == 1)
                {
                    _currentVerticalButtonIndex = pressedDirection.Input switch
                    {
                        DirectionInput.Up => 0,
                        DirectionInput.Down => 2,
                        _ => _currentVerticalButtonIndex //impossible to get here
                    };
                }

                SetNewActiveMainMenuButton(VerticalButtons[_currentVerticalButtonIndex]);
            }
            else if (pressedDirection.Input is DirectionInput.Left or DirectionInput.Right)
            {
                var buttonIndex = pressedDirection.Input switch
                {
                    DirectionInput.Left => _currentHorizontalButtonIndex - 1,
                    DirectionInput.Right => _currentHorizontalButtonIndex + 1,
                    _ => _currentHorizontalButtonIndex
                };

                switch (buttonIndex)
                {
                    case < 0:
                        _currentHorizontalButtonIndex = -1;
                        if(_currentVerticalButtonIndex < 0) _currentVerticalButtonIndex = 0;
                        SetNewActiveMainMenuButton(VerticalButtons[_currentVerticalButtonIndex]);
                        break;
                    case >= 0 when buttonIndex < _horizontalButtonCount:
                        _currentHorizontalButtonIndex = buttonIndex;
                        SetNewActiveMainMenuButton(HorizontalButtons[buttonIndex]);
                        break;
                }
            }

            _activeMainMenuButton.colors = _activeMainMenuColorBlock;

            return;

            void SetNewActiveMainMenuButton(GameObject gameObjectHoldingMenuButton)
            {

                _activeMainMenuButton.colors = _defaultMainMenuColorBlock;
                _activeMainMenuButton = gameObjectHoldingMenuButton.GetComponent<Button>();
            }
        }
    }
}
