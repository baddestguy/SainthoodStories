using Assets._Scripts.Xbox;
using TMPro;
using UnityEngine;

namespace Assets._Scripts
{
    public class SplashSceneController : MonoBehaviour
    {
        public static SplashSceneController Instance;

        public TextMeshProUGUI StartButtonPrompt;
        public TextMeshProUGUI FailureReasonPrompt;

        private bool _hasRegisteredForInputMethodChanged;
        private bool _hasIinitializedText;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (GameSettings.Instance == null || !GameSettings.Instance.IsUsingController) return;

            //if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonA))
            //{
            //    Debug.LogError("Gamepad1ButtonA pressed");
            //}
            //if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad2ButtonA))
            //{
            //    Debug.LogError("Gamepad2ButtonA pressed");
            //}
            //if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad3ButtonA))
            //{
            //    Debug.LogError("Gamepad3ButtonA pressed");
            //}
            //if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad4ButtonA))
            //{
            //    Debug.LogError("Gamepad4ButtonA pressed");
            //}
            //if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad5ButtonA))
            //{
            //    Debug.LogError("Gamepad5ButtonA pressed");
            //}
            //if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad6ButtonA))
            //{
            //    Debug.LogError("Gamepad6ButtonA pressed");
            //}
            //if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad7ButtonA))
            //{
            //    Debug.LogError("Gamepad7ButtonA pressed");
            //}
            //if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad8ButtonA))
            //{
            //    Debug.LogError("Gamepad8ButtonA pressed");
            //}


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

            (GamePadButton Button, CustomButtonControl Control) pressedButton = (GamePadButton.Void, new CustomButtonControl());
            if (GameSettings.Instance.IsXboxMode)
            {

#if MICROSOFT_GDK_SUPPORT
                // Check for button A press on all connected gamepads
                for (var i = 0; i < 8; i++) // Assume MaxGamepads is a constant defining the max number of supported gamepads
                {
                    if (GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonA + (20*i)))
                    {
                        GamePadController.ActiveXboxGamePadModifier = (20 * i);
                        pressedButton = (GamePadButton.South, new CustomButtonControl(true, true, false));
                    }
                }
#endif
            }
            else
            {
                pressedButton = GamePadController.GetButton();
            }

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
            //if(false)
            {
                XboxUserHandler.Instance.TryLogInUser();
            }
            else
            {
                //We might need to do something here to get the steam user in the future
                GameManager.Instance.PlayerLoginSuccess();
            }
        }


        private void HandleInputMethodChanged(bool isUsingController)
        {
            StartButtonPrompt.text = isUsingController ? "Press A to Start" : "Click to Start";
        }
    }
}
