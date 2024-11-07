using Assets._Scripts.Xbox;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Assets._Scripts
{
    public class SplashSceneController : MonoBehaviour
    {
        public static SplashSceneController Instance;

        public Text StatusText;
        public TextMeshProUGUI FailureReasonPrompt;

        private bool _clickedStart;
        private const string DefaultStatusText = "PRESS ANY BUTTON TO START";

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
            StatusText.text = DefaultStatusText;
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance == null || GameSettings.Instance == null)
            {
                Debug.Log("Splash screen controller waiting for Game manager and settings");
                return;  // We can't do anything without GameManager or GameSettings is ready
            }

            if (_clickedStart)
                return;

            (GamePadButton Button, CustomButtonControl Control) pressedButton = (GamePadButton.Void, new CustomButtonControl());

            if (GameSettings.Instance.IsUsingController)
            {
                if (GameSettings.Instance.IsUsingController)
                {
                    pressedButton = GamePadController.GetButton();
                }

                if (pressedButton.Button != GamePadButton.Void && pressedButton.Control.WasPressedThisFrame)
                {
                    StartButtonClicked();
                }
            }
            else
            {
                if (Input.anyKeyDown || Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1))
                {
                    StartButtonClicked();
                }
            }


        }

        void OnDisable()
        {
            if (GameSettings.Instance.IsXboxMode)
            {
                XboxUserHandler.Instance.OnXboxUserLoginStatusChange -= OnXboxUserLoginStatusChange;
            }
        }

        public void StartButtonClicked()
        {
            if (_clickedStart)
                return;

            _clickedStart = true;

            if (GameSettings.Instance.IsXboxMode)
            {
                XboxUserHandler.Instance.OnXboxUserLoginStatusChange += OnXboxUserLoginStatusChange;
                XboxUserHandler.Instance.TryLogInUser();
            }
            else
            {
                GameManager.Instance.PlayerLoginSuccess();
            }
        }

        private void OnXboxUserLoginStatusChange(bool isLoggedIn, string message, bool isError)
        {
            if (isLoggedIn) return;

            if (isError)
            {
                StatusText.text = DefaultStatusText;
                FailureReasonPrompt.text = message;
                FailureReasonPrompt.gameObject.SetActive(true);
                _clickedStart = false;
            }
            else
            {
                FailureReasonPrompt.gameObject.SetActive(false);

                if (!string.IsNullOrWhiteSpace(message))
                {
                    StatusText.text = message;
                }
            }
        }
    }
}
