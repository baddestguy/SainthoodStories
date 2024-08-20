using System;
using System.Collections;
using Assets._Scripts.Extensions;
using TMPro;
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

        /// <summary>
        /// 1: Resume button.
        /// <br />2: Main Menu Button.
        /// <br /> 3: Desktop Button.
        /// </summary>
        public GameObject[] PauseTabOptions;
        /// <summary>
        /// 1: Full Screen Toggle.
        /// <br />2: Show Grid Toggle.
        /// <br /> 3: Quality Dropdown.
        /// <br /> 4: Resolution Dropdown.
        /// </summary>
        public GameObject[] GraphicTabOptions;
        /// <summary>
        /// 1: Sound enable toggle.
        /// <br />2: Global slider.
        /// <br /> 3: SFX slider.
        /// <br /> 4: Music Slider.
        /// <br /> 5: Ambiance Slider.
        /// </summary>
        public GameObject[] SoundTabOptions;
        private int _selectedPauseButtonIndex;
        private int _selectedGraphicsButtonIndex;
        private int _selectedSoundButtonIndex;
        private ColorBlock _defaultPauseTabColorBlock;
        private ColorBlock _activePauseTabColorBlock;
        private readonly Color _defaultButtonColour = new(0.7882353f, 0.6627451f, 0.3882353f);
        private bool _skipFrame;
        private bool _isQualityDropdownOpen;
        private int _qualityDropdownIndex;

        private PauseMenu.ActivePauseTab ActiveTab => PauseMenu.Instance.ActiveTab;

        private Button ActivePauseTabButton => PauseTabOptions[_selectedPauseButtonIndex].GetComponent<Button>();

        private Text ActiveGraphicsTabOptionText => GraphicTabOptions[_selectedGraphicsButtonIndex].GetComponentInChildren<Text>();
        private Outline ActiveGraphicsTabOptionTextOutline => GraphicTabOptions[_selectedGraphicsButtonIndex].GetComponentInChildren<Outline>();
        private Toggle ActiveGraphicsTabOptionToggle => GraphicTabOptions[_selectedGraphicsButtonIndex].GetComponentInChildren<Toggle>();
        private Image ActiveGraphicsTabOptionDropdownImage => GraphicTabOptions[_selectedGraphicsButtonIndex].FindDeepChild("Dropdown").GetComponent<Image>();
        private TMP_Dropdown ActiveGraphicsTabOptionDropdown => GraphicTabOptions[_selectedGraphicsButtonIndex].GetComponentInChildren<TMP_Dropdown>();

        private Text ActiveSoundsTabOptionText => SoundTabOptions[_selectedSoundButtonIndex].GetComponentInChildren<Text>();
        private Outline ActiveSoundsTabOptionTextOutline => SoundTabOptions[_selectedSoundButtonIndex].GetComponentInChildren<Outline>();
        private Toggle ActiveSoundsTabOptionToggle => SoundTabOptions[_selectedSoundButtonIndex].GetComponentInChildren<Toggle>();
        private Image ActiveSoundsTabOptionHandle => SoundTabOptions[_selectedSoundButtonIndex].FindDeepChild("Handle").GetComponent<Image>();
        private Image ActiveSoundsTabOptionFill => SoundTabOptions[_selectedSoundButtonIndex].FindDeepChild("Fill").GetComponent<Image>();


        private void Awake()
        {
            Instance = this;
        }

        void Start()
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged += HandleInputMethodChanged;
            if (!GameSettings.Instance.IsXboxMode) return;

            //You can only change show grid option on xbox
            for (var i = 0; i < GraphicTabOptions.Length; i++)
            {
                if (i != 1)
                {
                    GraphicTabOptions[i].SetActive(false);
                }
            }
            //Move the show grid option to the top of the list
            GraphicTabOptions[1].transform.localPosition = new Vector3(GraphicTabOptions[1].transform.localPosition.x, GraphicTabOptions[0].transform.localPosition.y);
        }

        public void OnDisable()
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged -= HandleInputMethodChanged;
        }
        private void HandleInputMethodChanged(bool isUsingController)
        {
            if (isUsingController)
            {
                Activate();
                _skipFrame = true;
            }
            else
            {
                Deactivate();
            }
        }

        private void Update()
        {
            if (!GameSettings.Instance.IsUsingController) return;

            var pressedButton = GamePadController.GetButton();
            var pressedDirection = GamePadController.GetDirection();

            if (pressedButton.Control.WasPressedThisFrame && pressedButton.Button == GamePadButton.Start)
            {
                PauseMenu.Instance.Activate();
            }

            if (!PauseMenu.Instance.active) return;

            if (_skipFrame)
            {
                _skipFrame = false;
                return;
            }

            if (!_isQualityDropdownOpen && pressedButton.Control.WasPressedThisFrame && pressedButton.Button is GamePadButton.LeftShoulder or GamePadButton.RightShoulder)
            {
                HandleTabSwitch(pressedButton);
            }
            else
            {
                HandleTabInputs(pressedButton, pressedDirection);
            }
        }

        public void Activate()
        {
            _defaultPauseTabColorBlock = ActivePauseTabButton.colors;
            _activePauseTabColorBlock = _defaultPauseTabColorBlock;
            _activePauseTabColorBlock.normalColor = _defaultPauseTabColorBlock.highlightedColor;

            if (!GameSettings.Instance.IsUsingController) return;

            ResetIndexes();
            SetPauseTabHover(true);
            SetGraphicsTabHover(true);
            SetSoundTabHover(true);
        }

        public void Deactivate()
        {
            SetPauseTabHover(false);
            if (_isQualityDropdownOpen)
            {
                ActiveGraphicsTabOptionDropdown.Hide();
                _isQualityDropdownOpen = false;
            }
            SetGraphicsTabHover(false);
            SetSoundTabHover(false);

        }

        private void HandleTabInputs(
            (GamePadButton Button, CustomButtonControl Control) pressedButton,
            (DirectionInput Input, CustomButtonControl Control) pressedDirection)
        {
            switch (ActiveTab)
            {
                case PauseMenu.ActivePauseTab.Pause:
                    HandlePauseTabInput(pressedButton, pressedDirection);
                    break;
                case PauseMenu.ActivePauseTab.Graphics:
                    HandleGraphicsTabInput(pressedButton, pressedDirection);
                    break;
                case PauseMenu.ActivePauseTab.Sound:
                    HandleSoundTabInput(pressedButton, pressedDirection);
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }
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
                Deactivate();
                ResetIndexes();
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
                Activate();
            }
        }

        #region Pause
        private void HandlePauseTabInput(
            (GamePadButton Button, CustomButtonControl Control) pressedButton,
            (DirectionInput Input, CustomButtonControl Control) pressedDirection)
        {
            if (pressedDirection.Control.WasPressedThisFrame)
            {
                SetPauseTabHover(false);

                var pauseIncrement = pressedDirection.Input is DirectionInput.Up ? -1 : pressedDirection.Input is DirectionInput.Down ? 1 : 0;
                _selectedPauseButtonIndex = (_selectedPauseButtonIndex + pauseIncrement + PauseTabOptions.Length) % PauseTabOptions.Length;

                SetPauseTabHover(true);
            }
            else if (pressedButton.Control.WasPressedThisFrame && pressedButton.Button == GamePadButton.South)
            {
                ActivePauseTabButton.onClick.Invoke();
            }
        }
        private void SetPauseTabHover(bool setActive)
        {
            ActivePauseTabButton.colors = setActive ? _activePauseTabColorBlock : _defaultPauseTabColorBlock;
        }
        #endregion

        private void HandleGraphicsTabInput(
            (GamePadButton Button, CustomButtonControl Control) pressedButton,
            (DirectionInput Input, CustomButtonControl Control) pressedDirection)
        {
            //Navigate the graphics tab
            if (pressedDirection.Control.WasPressedThisFrame && !_isQualityDropdownOpen)
            {
                SetGraphicsTabHover(false);

                var graphicsIncrement = pressedDirection.Input is DirectionInput.Up ? -1 : pressedDirection.Input is DirectionInput.Down ? 1 : 0;
                _selectedGraphicsButtonIndex = (_selectedGraphicsButtonIndex + graphicsIncrement + GraphicTabOptions.Length) % GraphicTabOptions.Length;

                SetGraphicsTabHover(true);
            }
            //Navigate the open dropdown
            else if (pressedDirection.Control.WasPressedThisFrame && _isQualityDropdownOpen)
            {
                SetGraphicsTabDropdownHover(false);
                //select a dropdown option
                var dropdownIncrement = pressedDirection.Input is DirectionInput.Up ? -1 : pressedDirection.Input is DirectionInput.Down ? 1 : 0;
                _qualityDropdownIndex = (_qualityDropdownIndex + dropdownIncrement + ActiveGraphicsTabOptionDropdown.options.Count) % ActiveGraphicsTabOptionDropdown.options.Count;

                SetGraphicsTabDropdownHover(true);
            }
            else if (pressedButton.Control.WasPressedThisFrame)
            {
                //Toggle an option or open the dropdown
                if (pressedButton.Button == GamePadButton.South && !_isQualityDropdownOpen)
                {
                    switch (_selectedGraphicsButtonIndex)
                    {
                        case 0:
                            //Fullscreen
                            UIGraphicsSettings.Instance.fullscreenToggle.isOn = !UIGraphicsSettings.Instance.fullscreenToggle.isOn;
                            break;
                        case 1:
                            //Show Grid
                            PauseMenu.Instance.ShowGridToggle.isOn = !PauseMenu.Instance.ShowGridToggle.isOn;
                            break;
                        case 2:
                        case 3:
                            //Quality and Resolution
                            ActiveGraphicsTabOptionDropdown.Show();
                            _isQualityDropdownOpen = true;
                            _qualityDropdownIndex = ActiveGraphicsTabOptionDropdown.value;
                            SetGraphicsTabDropdownHover(true);
                            break;
                    }
                }
                //back button pressed
                else if (pressedButton.Button == GamePadButton.East)
                {
                    //dropdown is open. Close the dropdown
                    if (_isQualityDropdownOpen)
                    {
                        ActiveGraphicsTabOptionDropdown.Hide();
                        _isQualityDropdownOpen = false;
                    }
                    //dropdown is closed. Close the pause menu
                    else
                    {
                        PauseMenu.Instance.Activate();
                    }
                }
                //dropdown is open. Select an option
                else if (pressedButton.Button == GamePadButton.South && _isQualityDropdownOpen)
                {
                    ActiveGraphicsTabOptionDropdown.value = _qualityDropdownIndex;
                    SetGraphicsTabDropdownHover(false);
                    ActiveGraphicsTabOptionDropdown.Hide();
                    _isQualityDropdownOpen = false;
                }
            }
        }

        private void SetGraphicsTabHover(bool setActive)
        {
            var colour = setActive ? Color.black : Color.white;
            ActiveGraphicsTabOptionText.color = colour;
            ActiveGraphicsTabOptionTextOutline.enabled = !setActive;
            if (_selectedGraphicsButtonIndex < 2)
            {
                //these are the only two options that can be toggled
                ActiveGraphicsTabOptionToggle.colors = new ColorBlock { normalColor = colour, selectedColor = colour, colorMultiplier = 1 };
            }
            else
            {
                //these are the dropdowns
                ActiveGraphicsTabOptionDropdownImage.color = setActive ? Color.black : _defaultButtonColour;
            }
        }

        private void SetGraphicsTabDropdownHover(bool setActive)
        {
            // Get the dropdown list (created dynamically when the dropdown is expanded)
            var dropdownList = ActiveGraphicsTabOptionDropdown.transform.Find("Dropdown List").gameObject;

            // Find the first item in the dropdown list
            var firstItem = dropdownList.transform.Find("Viewport/Content").GetChild(_qualityDropdownIndex + 1); //Add 1 to account for the template item

            // Access the background of the first item
            var toggle = firstItem.gameObject.FindDeepChild("Item Background").GetComponent<Image>();

            if (toggle != null)
            {
                // Change the background color
                toggle.color = setActive ? _defaultButtonColour : Color.black;
            }

            var checkmark = firstItem.gameObject.FindDeepChild("Item Checkmark").GetComponent<Image>();
            if (checkmark != null)
            {
                // Change the background color
                checkmark.color = setActive ? Color.black : _defaultButtonColour;
            }

            if (setActive)
            {
                StartCoroutine(ScrollAfterDropdownIsGenerated(dropdownList));
            }
        }
        IEnumerator ScrollAfterDropdownIsGenerated(GameObject dropdownList)
        {
            yield return new WaitForEndOfFrame(); // Wait for the dropdown to be fully generated

            if (dropdownList != null)
            {
                var scrollRect = dropdownList.GetComponentInChildren<ScrollRect>();
                if (scrollRect != null)
                {
                    var content = scrollRect.content;
                    if (_qualityDropdownIndex >= 0 && _qualityDropdownIndex < content.childCount)
                    {
                        // Calculate the scroll position
                        // Add one because zero based, subtract one because the template item is not included
                        var normalizedPosition = (float)(_qualityDropdownIndex + 1) / (content.childCount - 1); 

                        // Set the scroll position (0 is the bottom, 1 is the top)
                        scrollRect.verticalNormalizedPosition = 1f - normalizedPosition;
                    }
                }
            }
        }
        #region Sound
        private void HandleSoundTabInput(
            (GamePadButton Button, CustomButtonControl Control) pressedButton,
            (DirectionInput Input, CustomButtonControl Control) pressedDirection)
        {
            if (pressedDirection.Control.WasPressedThisFrame || pressedDirection.Control.IsPressed)
            {
                if (pressedDirection.Control.WasPressedThisFrame && pressedDirection.Input is DirectionInput.Up or DirectionInput.Down)
                {
                    SetSoundTabHover(false);

                    var soundIncrement = pressedDirection.Input is DirectionInput.Up ? -1 : 1;
                    _selectedSoundButtonIndex = (_selectedSoundButtonIndex + soundIncrement + SoundTabOptions.Length) % SoundTabOptions.Length;

                    SetSoundTabHover(true);
                }
                else if (pressedDirection.Input is DirectionInput.Left or DirectionInput.Right && _selectedSoundButtonIndex != 0)
                {
                    var factor = pressedDirection.Control.WasPressedThisFrame ? 0.1f : 0.01f;
                    switch (_selectedSoundButtonIndex)
                    {
                        case 1:
                            //Global
                            UISoundSettings.Instance.global.value += pressedDirection.Input == DirectionInput.Right ? factor : -factor;
                            break;
                        case 2:
                            //SFX
                            UISoundSettings.Instance.SFX.value += pressedDirection.Input == DirectionInput.Right ? factor : -factor;
                            break;
                        case 3:
                            //Music
                            UISoundSettings.Instance.Music.value += pressedDirection.Input == DirectionInput.Right ? factor : -factor;
                            break;
                        case 4:
                            //Ambiance
                            UISoundSettings.Instance.Ambiance.value += pressedDirection.Input == DirectionInput.Right ? factor : -factor;
                            break;
                    }
                }
            }
            else if (pressedButton.Control.WasPressedThisFrame && pressedButton.Button == GamePadButton.South && _selectedSoundButtonIndex == 0)
            {
                UISoundSettings.Instance.soundEnable.isOn = !UISoundSettings.Instance.soundEnable.isOn;
            }
        }

        private void SetSoundTabHover(bool setActive)
        {
            var colour = setActive ? Color.black : Color.white;
            var fillColour = setActive ? Color.black : _defaultButtonColour;
            var handleColour = setActive ? Color.black : new Color(0.2313726f, 0.1803922f, 0.1215686f);

            ActiveSoundsTabOptionText.color = colour;
            ActiveSoundsTabOptionTextOutline.enabled = !setActive;
            if (_selectedSoundButtonIndex == 0)
            {
                //this is the only two option that can be toggled
                ActiveSoundsTabOptionToggle.colors = new ColorBlock { normalColor = colour, selectedColor = colour, colorMultiplier = 1 };
            }
            else
            {
                //these are the sliders
                ActiveSoundsTabOptionHandle.color = handleColour;
                ActiveSoundsTabOptionFill.color = fillColour;
            }
        }
        #endregion

        private void ResetIndexes()
        {
            _selectedPauseButtonIndex = 0;
            _selectedGraphicsButtonIndex = 0;
            _selectedSoundButtonIndex = 0;
        }
    }
}
