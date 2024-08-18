using System;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts.Xbox
{
    /// <summary>
    /// Responsible for managing game pad input during on the pause menu, and the settings screen.
    /// </summary>
    public class PauseMenuControllerHandler : MonoBehaviour
    {
        public static PauseMenuControllerHandler Instance { get; private set; }

        public GameObject[] Buttons;
        private int _selectedButtonIndex;
        private ColorBlock _defaultMainMenuColorBlock;
        private ColorBlock _activeMainMenuColorBlock;

        private Button ActiveButton => Buttons[_selectedButtonIndex].GetComponent<Button>();

        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged += HandleInputMethodChanged;
        }

        public void OnDisable()
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged -= HandleInputMethodChanged;
        }

        private void Update()
        {
            if (!GameSettings.Instance.IsUsingController || !PauseMenu.Instance.active) return;

            var pressedButton = GamePadController.GetButton();
            var pressedDirection = GamePadController.GetDirection();


            if (pressedButton.Control.WasPressedThisFrame && pressedButton.Button is GamePadButton.LeftShoulder or GamePadButton.RightShoulder)
            {
                HandleTabSwitch(pressedButton);
            }
            else if (pressedDirection.Control.WasPressedThisFrame)
            {
                HandleNavigation(pressedDirection);
            }

            HandleAction(pressedButton, pressedDirection);
        }

        private void HandleInputMethodChanged(bool isUsingController)
        {
            if (!isUsingController)
            {
                Deactivate();
            }
        }

        public void Activate(ToggleGroup menuToggleGroup)
        {
            if (!GameSettings.Instance.IsUsingController) return;

            _selectedButtonIndex = 0;
            _defaultMainMenuColorBlock = ActiveButton.colors;
            _activeMainMenuColorBlock = _defaultMainMenuColorBlock;
            _activeMainMenuColorBlock.normalColor = _defaultMainMenuColorBlock.highlightedColor;
            ActiveButton.colors = _activeMainMenuColorBlock;

            //if(!GameSettings.Instance.IsXboxMode) return;
            //var graphicsToggleTransform = menuToggleGroup.transform.Find("Graphics");
            var soundToggleTransform = menuToggleGroup.transform.Find("SoundTab");

            ////We don't allow changes to graphics settings when running in xbox mode.
            //graphicsToggleTransform.gameObject.SetActive(false);
            //soundToggleTransform.localPosition = new Vector3(graphicsToggleTransform.localPosition.x, soundToggleTransform.localPosition.y);


            // There is no graphics tab on xbox so set the sound tab to be the default
            PauseMenu.Instance.ToggleSound();
            var soundToggle = soundToggleTransform.GetComponent<Toggle>();
            soundToggle.isOn = true;
        }

        public void Deactivate()
        {
            ActiveButton.colors = _defaultMainMenuColorBlock;
        }

        private void HandleNavigation((DirectionInput Input, CustomButtonControl Control) pressedDirection)
        {
            ActiveButton.colors = _defaultMainMenuColorBlock;

            var increment = pressedDirection.Input is DirectionInput.Up ? -1 : 1;
            _selectedButtonIndex = (_selectedButtonIndex + increment + Buttons.Length) % Buttons.Length;
            ActiveButton.colors = _activeMainMenuColorBlock;
        }

        private void HandleTabSwitch((GamePadButton Button, CustomButtonControl Control) pressedButton)
        {
            var tabIndex = (int)PauseMenu.Instance.ActiveTab;
            var numberOfTabs = Enum.GetNames(typeof(PauseMenu.ActivePauseTab)).Length;
            if (pressedButton.Button == GamePadButton.LeftShoulder && pressedButton.Control.WasPressedThisFrame)
            {
                tabIndex = (tabIndex - 1 + numberOfTabs) % numberOfTabs;
            }
            else if (pressedButton.Button == GamePadButton.RightShoulder && pressedButton.Control.WasPressedThisFrame)
            {
                tabIndex = (tabIndex + 1) % numberOfTabs;
            }

            var newTab = (PauseMenu.ActivePauseTab)tabIndex;
            if (newTab != PauseMenu.Instance.ActiveTab)
            {
                switch (newTab)
                {
                    case PauseMenu.ActivePauseTab.Pause:
                        PauseMenu.Instance.TogglePause();
                        break;
                    case PauseMenu.ActivePauseTab.Graphics:
                        PauseMenu.Instance.ToggleGraphics();
                        break;
                    case PauseMenu.ActivePauseTab.Sound:
                        PauseMenu.Instance.ToggleSound();
                        break;
                    default:
                        //We really should never get here but this is the one tab we know will always be there
                        PauseMenu.Instance.ToggleSound();
                        break;
                }
            }
        }

        private void HandleAction(
            (GamePadButton Button, CustomButtonControl Control) pressedButton,
            (DirectionInput Input, CustomButtonControl Control) pressedDirection)
        {
            if (pressedButton.Control.WasPressedThisFrame && pressedButton.Button == GamePadButton.South && ActiveButton != null)
            {
                ActiveButton.onClick.Invoke();
                return;
            }

            if (pressedDirection.Control.WasPressedThisFrame && pressedDirection.Input == DirectionInput.Right)
            {
                //todo: Stuff with sliders and checkboxes
            }

            if (pressedDirection.Control.WasReleasedThisFrame && pressedDirection.Input == DirectionInput.Right)
            {
                //todo: Stuff with sliders 
            }

            if (pressedDirection.Control.WasPressedThisFrame && pressedDirection.Input == DirectionInput.Left)
            {
                //todo: Stuff with sliders and checkboxes
            }

            if (pressedDirection.Control.WasReleasedThisFrame && pressedDirection.Input == DirectionInput.Left)
            {
                //todo: Stuff with sliders
            }
        }
    }
}
