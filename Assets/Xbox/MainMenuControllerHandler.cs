using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets.Xbox
{
    public class MainMenuControllerHandler : MonoBehaviour
    {
        public GameObject NewGameGameObject;
        public GameObject ContinueGameObject;
        public GameObject ExitGameObject;
        public GameObject SettingsButtonGameObject;
        public GameObject SteamWishListGameObject;
        public GameObject DiscordGameObject;

        private bool IsNewGameButtonActive => _activeMainMenuButton.gameObject.Equals(NewGameGameObject);
        private Button _activeMainMenuButton;
        private ColorBlock _defaultMainMenuColorBlock;
        private ColorBlock _activeMainMenuColorBlock;

        // Start is called before the first frame update
        void Start()
        {
            if (!GameSettings.Instance.IsXboxMode) return;

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
            if (!GameSettings.Instance.IsXboxMode) return;
            if (Gamepad.current == null) return;

            HandleControllerNavigation();

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                _activeMainMenuButton.onClick.Invoke();
            }
            else if(Gamepad.current.buttonWest.wasPressedThisFrame)
            {
                SettingsButtonGameObject.GetComponent<Button>().onClick.Invoke();
            }
        }

        private void HandleControllerNavigation()
        {
            if (IsNewGameButtonActive && (Gamepad.current.dpad.down.wasPressedThisFrame || Gamepad.current.dpad.up.wasPressedThisFrame))
            {
                SetNewActiveMainMenuButton(ContinueGameObject);
            }
            else if (!IsNewGameButtonActive && (Gamepad.current.dpad.up.wasPressedThisFrame || Gamepad.current.dpad.down.wasPressedThisFrame))
            {
                SetNewActiveMainMenuButton(NewGameGameObject);
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
