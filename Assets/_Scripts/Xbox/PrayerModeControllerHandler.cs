using System.Linq;
using Assets._Scripts.Xbox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PrayerModeController : MonoBehaviour
{
    [Header("Update RosaryButtonIndex if Rosary is moved")]
    public GameObject[] HorizonalButtons;
    public int RosaryButtonIndex = 1;
    public GameObject RosaryMysteryButtonGroup;

    private int _horizontalButtonIndex = 1;
    private int _rosaryMysteryIndex;

    private bool _skipFrame;
    private bool _hasConfiguredForController;
    private ColorBlock _defaultMainMenuColorBlock;
    private ColorBlock _activeMainMenuColorBlock;
    private Button _activeButton;
    private GameObject[] _rosaryMysteryButtons;

    // Start is called before the first frame update
    void Start()
    {
        GameplayControllerHandler.Instance.OnInputMethodChanged += HandleInputMethodChanged;
    }

    public void OnDisable()
    {
        GameplayControllerHandler.Instance.OnInputMethodChanged -= HandleInputMethodChanged;
    }

    private void HandleInputMethodChanged(bool isUsingController)
    {
        if (isUsingController)
        {
            _skipFrame = true;
            _rosaryMysteryIndex = 0;
        }
        else
        {
        }
    }

    public void TryApplyControllerHover()
    {
        if (!_hasConfiguredForController && GameSettings.Instance.IsUsingController)
        {
            _activeButton = HorizonalButtons[1].GetComponent<Button>();
            _defaultMainMenuColorBlock = _activeButton.colors;
            _activeMainMenuColorBlock = _defaultMainMenuColorBlock;
            _activeMainMenuColorBlock.normalColor = _defaultMainMenuColorBlock.highlightedColor;
            _activeButton.colors = _activeMainMenuColorBlock;

            _rosaryMysteryButtons = RosaryMysteryButtonGroup.GetComponentsInChildren<Button>().Select(x => x.gameObject).ToArray()
                .Concat(new[] { HorizonalButtons[RosaryButtonIndex] })
                .ToArray();
            _rosaryMysteryIndex = _rosaryMysteryButtons.Length - 1;

            _hasConfiguredForController = true;
        }

        if (_hasConfiguredForController && !GameSettings.Instance.IsUsingController)
        {
            _activeButton.colors = _defaultMainMenuColorBlock;
            _hasConfiguredForController = false;
        }
    }

    // Update is called once per frame
    void Update()
    {
        TryApplyControllerHover();

        //Don't do anything if we aren't using a controller or a pop up is overlaying the main menu.
        if ((Gamepad.current == null && !GameSettings.Instance.IsXboxMode) ||
            !GameSettings.Instance.IsUsingController ||
            PauseMenu.Instance.active ||
            (UI.Instance != null && UI.Instance.TutorialPopupQuestion.activeInHierarchy) ||
            (UI.Instance != null && UI.Instance.CreditsObj.activeInHierarchy)) return;

        if (_skipFrame)
        {
            _skipFrame = false;
            return;
        }

        var pressedButton = GamePadController.GetButton();
        var pressedDirection = GamePadController.GetDirection();

        if (pressedButton.Control.WasPressedThisFrame && pressedButton.Button == GamePadButton.South)
        {
            _activeButton.onClick.Invoke();
        }
        else if(pressedDirection.Control.WasPressedThisFrame)
        {
            HandleControllerNavigation(pressedDirection.Input);
        }
    }


    private void HandleControllerNavigation(DirectionInput input)
    {
        if (input.IsHorizontal())
        {
            _rosaryMysteryIndex = _rosaryMysteryButtons.Length - 1;

            // Increment to the next active game object
            do
            {
                _horizontalButtonIndex += input == DirectionInput.Left ? -1 : +1;
                //allows looping
                _horizontalButtonIndex %= HorizonalButtons.Length;
                if (_horizontalButtonIndex < 0)
                {
                    _horizontalButtonIndex = HorizonalButtons.Length - 1;
                }
            } while (!HorizonalButtons[_horizontalButtonIndex].activeInHierarchy);

            SetNewActiveMainMenuButton(HorizonalButtons[_horizontalButtonIndex]);
        }
        else if (input.IsVertical())
        {
            if (PrayerManager.Instance.PrayerButtonTypes.activeInHierarchy)
            {
                _horizontalButtonIndex = 1;

                // Increment to the next active game object
                do
                {
                    _rosaryMysteryIndex += input == DirectionInput.Up ? -1 : +1;
                    //allows looping
                    _rosaryMysteryIndex %= _rosaryMysteryButtons.Length;
                    if (_rosaryMysteryIndex < 0)
                    {
                        _rosaryMysteryIndex = _rosaryMysteryButtons.Length - 1;
                    }
                } while (!_rosaryMysteryButtons[_rosaryMysteryIndex].gameObject.activeInHierarchy);

                SetNewActiveMainMenuButton(_rosaryMysteryButtons[_rosaryMysteryIndex].gameObject);
            }
        }


        void SetNewActiveMainMenuButton(GameObject gameObjectHoldingMenuButton)
        {
            _activeButton.colors = _defaultMainMenuColorBlock;
            _activeButton = gameObjectHoldingMenuButton.GetComponent<Button>();
            _activeButton.colors = _activeMainMenuColorBlock;
        }
    }
}
