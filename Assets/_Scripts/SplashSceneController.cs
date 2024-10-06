using Assets._Scripts.Xbox;
using TMPro;
using UnityEngine;

namespace Assets._Scripts
{
    public class SplashSceneController : MonoBehaviour
    {
        public static SplashSceneController Instance;

        public TextMeshProUGUI FailureReasonPrompt;

        private bool _clickedStart;

        // Start is called before the first frame update
        void Start()
        {
            Instance = this;
        }

        // Update is called once per frame
        void Update()
        {
            if (GameManager.Instance == null || GameSettings.Instance == null)
            {
                Debug.Log("Splash screen controller waiting for Game manager and settings");
                return;  // We can't do anything without GameManager or GameSettings is ready
            }

            (GamePadButton Button, CustomButtonControl Control) pressedButton = (GamePadButton.Void, new CustomButtonControl());

            if (GameSettings.Instance.IsUsingController)
            {
                if (GameSettings.Instance.IsXboxMode)
                {

#if MICROSOFT_GDK_SUPPORT
                    pressedButton = GamePadController.TryInitializeXboxController();
#endif
                }
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

        public void StartButtonClicked()
        {
            if (_clickedStart)
            {
            }
            else
            {
                _clickedStart = true;

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
        }

    }
}
