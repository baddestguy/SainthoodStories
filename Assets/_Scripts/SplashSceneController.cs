using Assets._Scripts.Xbox;
using TMPro;
using UnityEngine;

namespace Assets._Scripts
{
    public class SplashSceneController : MonoBehaviour
    {
        public TextMeshProUGUI StartButtonPrompt;
        public TextMeshProUGUI FailureReasonPrompt;

        private bool _hasRegisteredForInputMethodChanged;
        private bool _hasIinitializedText;

        // Start is called before the first frame update
        void Start()
        {
        }

        // Update is called once per frame
        void Update()
        {
            if (GameSettings.Instance == null || !GameSettings.Instance.IsUsingController) return;

            if (!_hasRegisteredForInputMethodChanged)
            {
                GameplayControllerHandler.Instance.OnInputMethodChanged += HandleInputMethodChanged;
                _hasRegisteredForInputMethodChanged = true;
            }

            if (!_hasIinitializedText)
            {
                HandleInputMethodChanged(GameSettings.Instance.IsUsingController);
                _hasIinitializedText = true;
            }

            var pressedButton = GamePadController.GetButton();
            if (pressedButton.Button == GamePadButton.South && pressedButton.Control.WasPressedThisFrame)
            {
                StartButtonClicked();
            }
        }

        public void OnDisable()
        {
            if (_hasRegisteredForInputMethodChanged)
            {
                GameplayControllerHandler.Instance.OnInputMethodChanged -= HandleInputMethodChanged;
                _hasRegisteredForInputMethodChanged = false;
            }
        }

        public void StartButtonClicked()
        {
            while (GameManager.Instance == null || GameSettings.Instance == null)
            {
                // We can't do anything until the GameManager is initialized
            }

            if (GameSettings.Instance.IsXboxMode)
            {
                XboxUserManager.Instance.TryLogInUser();
            }
            else
            {
                //We might need to do something here to get the steam user in the future
                GameManager.Instance.PlayerLoginSuccess();
            }
        }

        public void FailedToLogin(string failureReason)
        {

        }

        private void HandleInputMethodChanged(bool isUsingController)
        {
            StartButtonPrompt.text = isUsingController ? "Press A to Start" : "Click to Start";
        }
    }
}
