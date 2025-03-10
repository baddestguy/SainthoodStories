using System;
using System.Linq;
using Assets._Scripts.Xbox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class PrayerModeController : MonoBehaviour
{
    [Header("Base Buttons")]
    public GameObject ExitButton;
    public GameObject RosaryButton;
    public GameObject SkipButton;

    private bool _skipFrame;
    private bool _hasConfiguredForController;
    private int _currentVerticalButtonIndex = -1;
    private ColorBlock _defaultMainMenuColorBlock;
    private ColorBlock _activeMainMenuColorBlock;
    private Button _activeButton;
    private Button[] _rosaryMysteryButtons;
    private GameObject[] _acceptableButtonsWhilePraying;

    // Start is called before the first frame update
    void Start()
    {
        GameplayControllerHandler.Instance.OnInputMethodChanged += HandleInputMethodChanged;
        _acceptableButtonsWhilePraying = new[] { ExitButton, SkipButton };
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
            _activeButton = RosaryButton.GetComponent<Button>();
            _defaultMainMenuColorBlock = _activeButton.colors;
            _activeMainMenuColorBlock = _defaultMainMenuColorBlock;
            _activeMainMenuColorBlock.normalColor = _defaultMainMenuColorBlock.highlightedColor;
            _activeButton.colors = _activeMainMenuColorBlock;
            _rosaryMysteryButtons = PrayerManager.Instance.PrayerButtonTypes.GetComponentsInChildren<Button>();
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
        if (pressedButton.Button == GamePadButton.South && pressedButton.Control.WasPressedThisFrame)
        {
            if (PrayerManager.Instance.Praying && !_acceptableButtonsWhilePraying.Contains(_activeButton.gameObject))
            {
                return;
            }

            _activeButton.onClick.Invoke();
        }
        else
        {
            HandleControllerNavigation();
        }
    }


    private void HandleControllerNavigation()
    {
        var pressedDirection = GamePadController.GetDirection();
        if (!pressedDirection.Control.WasPressedThisFrame) return;

        //if pressed direction is left, make Exit button active
        if (pressedDirection.Input == DirectionInput.Left)
        {
            SetNewActiveMainMenuButton(ExitButton);
            _currentVerticalButtonIndex = -1;
            return;
        }

        if (pressedDirection.Input == DirectionInput.Right)
        {
            _currentVerticalButtonIndex = -1;
            if (PrayerManager.Instance.Praying)
            {
                SetNewActiveMainMenuButton(SkipButton);
            }
            else
            {
                SetNewActiveMainMenuButton(RosaryButton);
            }
            return;
        }

        if (PrayerManager.Instance.PrayerButtonTypes.activeInHierarchy && pressedDirection.Input == DirectionInput.Up || pressedDirection.Input == DirectionInput.Down)
        {

            //if rosary button and up set to last in group.If down, do nothing.
            if (_currentVerticalButtonIndex == -1)
            {
                if (pressedDirection.Input is DirectionInput.Up)
                {
                    _currentVerticalButtonIndex = _rosaryMysteryButtons.Length - 1;
                }
                else
                {
                    return;
                }
            }
            else
            {
                _currentVerticalButtonIndex = pressedDirection.Input switch
                {
                    DirectionInput.Up => _currentVerticalButtonIndex - 1,
                    DirectionInput.Down => _currentVerticalButtonIndex + 1,
                    _ => _currentVerticalButtonIndex //impossible to get here
                };

                if (_currentVerticalButtonIndex < 0)
                {
                    //if at top, stay at top
                    _currentVerticalButtonIndex = 0;
                }
                else if (_currentVerticalButtonIndex >= _rosaryMysteryButtons.Length)
                {
                    //swap back to rosary button
                    _currentVerticalButtonIndex = -1;
                    SetNewActiveMainMenuButton(RosaryButton);
                    return;
                }
            }

            SetNewActiveMainMenuButton(_rosaryMysteryButtons[_currentVerticalButtonIndex].gameObject);
        }


        void SetNewActiveMainMenuButton(GameObject gameObjectHoldingMenuButton)
        {
            _activeButton.colors = _defaultMainMenuColorBlock;
            _activeButton = gameObjectHoldingMenuButton.GetComponent<Button>();
            _activeButton.colors = _activeMainMenuColorBlock;
        }
    }
}
