using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Xbox
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

        private Button _activeMainMenuButton;
        private ColorBlock _defaultMainMenuColorBlock;
        private ColorBlock _activeMainMenuColorBlock;

        // Start is called before the first frame update
        void Start()
        {
            if (!GameSettings.Instance.IsXboxMode) return;

            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            _activeMainMenuButton = NewGameGameObject.GetComponent<Button>();
            _defaultMainMenuColorBlock = _activeMainMenuButton.colors;
            _activeMainMenuColorBlock = _defaultMainMenuColorBlock;
            _activeMainMenuColorBlock.normalColor = _defaultMainMenuColorBlock.highlightedColor;
            _activeMainMenuButton.colors = _activeMainMenuColorBlock;

            ExitGameObject.SetActive(false);
            SteamWishListGameObject.SetActive(false);
            DiscordGameObject.SetActive(false);

            SettingsButtonGameObject.transform.localPosition = new Vector3(
                DiscordGameObject.transform.localPosition.x,
                DiscordGameObject.transform.localPosition.y);
        }

        // Update is called once per frame
        void Update()
        {
            if (Gamepad.current == null || !GameSettings.Instance.IsXboxMode) return;

            HandleControllerNavigation();

            var pressedButton = GamePadController.GetButton();
            if (pressedButton.Button == GamePadButton.South && pressedButton.Control.wasPressedThisFrame)
            {
                _activeMainMenuButton.onClick.Invoke();
            }
        }

        private void HandleControllerNavigation()
        {
            var continueGameAvailable = UI.Instance.ContinueBtn.interactable;
            var pressedDirection = GamePadController.GetDirection();
            if (!pressedDirection.Control.wasPressedThisFrame) return;


            if (continueGameAvailable && pressedDirection.Input is DirectionInput.Up or DirectionInput.Down)
            {
                SetNewActiveMainMenuButton(IsNewGameButtonActive ? ContinueGameObject : NewGameGameObject);
            }

            if (pressedDirection.Input is DirectionInput.Left or DirectionInput.Right)
            {
                SetNewActiveMainMenuButton(IsSettingsButtonActive ? NewGameGameObject : SettingsButtonGameObject);
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
