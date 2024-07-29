using UnityEngine;
using UnityEngine.UI;

namespace Assets.Xbox
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

        private void Update()
        {
            if (!GameSettings.Instance.IsUsingController || !PauseMenu.Instance.active) return;

            HandleNavigation();
            HandleAction();
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

            var graphicsToggleTransform = menuToggleGroup.transform.Find("Graphics");
            var soundToggleTransform = menuToggleGroup.transform.Find("SoundTab");

            //We don't allow changes to graphics settings when running in xbox mode.
            graphicsToggleTransform.gameObject.SetActive(false);
            soundToggleTransform.localPosition = new Vector3(graphicsToggleTransform.localPosition.x, soundToggleTransform.localPosition.y);


            // There is no graphics tab on xbox so set the sound tab to be the default
            PauseMenu.Instance.ToggleSound();
            var soundToggle = soundToggleTransform.GetComponent<Toggle>();
            soundToggle.isOn = true;


        }

        public void Deactivate()
        {
            ActiveButton.colors = _defaultMainMenuColorBlock;
        }

        private void HandleNavigation()
        {
            var pressedDirection = GamePadController.GetDirection();
            if (!pressedDirection.Control.wasPressedThisFrame || pressedDirection.Input is not (DirectionInput.Up or DirectionInput.Down)) return;

            ActiveButton.colors = _defaultMainMenuColorBlock;

            var increment = pressedDirection.Input is DirectionInput.Up ? -1 : 1;
            _selectedButtonIndex = (_selectedButtonIndex + increment + Buttons.Length) % Buttons.Length;
            ActiveButton.colors = _activeMainMenuColorBlock;
        }

        private void HandleAction()
        {
            var pressedButton = GamePadController.GetButton();
            if (pressedButton.Button == GamePadButton.South && pressedButton.Control.wasPressedThisFrame)
            {
                ActiveButton.onClick.Invoke();
            }
        }
    }
}
