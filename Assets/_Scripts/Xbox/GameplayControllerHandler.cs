using System;
using System.Linq;
using Assets._Scripts.Extensions;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace Assets._Scripts.Xbox
{

    /// <summary>
    /// Responsible for managing game pad input during game play,
    /// </summary>
    public class GameplayControllerHandler : MonoBehaviour
    {
        public static GameplayControllerHandler Instance { get; private set; }
        public delegate void InputMethodChanged(bool isUsingController);
        public event InputMethodChanged OnInputMethodChanged;

        private PopUI _currentPopUI;

        private CustomEventPopup _currentCustomEventPopup;
        private bool _customEventPopupFirstActionButtonAcknowledged;
        private int _customEventPopupButtonIndex;
        private bool _customEventLowerTooltips;

        private int _objectiveInventoryScrollIndex;
        private readonly Color _defaultColour = new(0.7882353f, 0.6627451f, 0.3882353f);

        private ActionButton _currentPopUIButton;
        private ProvisionsPopup _provisionsPopUp;
        private ProvisionUIItem[] _provisionItems;
        private ProvisionUIItem _currentProvisionUIItem;
        private GameObject _currentInventoryGameObject;

        private bool _hasHoveredFirstSaintButton;
        private bool _saintNextOptionHasHover;

        private PackageItem _currentPackageItem;
        private PackageSelector _packageSelector;
        private bool _selectedPackageSelectorItemIsPackage;

        private bool IsInBuilding => _currentPopUI != null;
        private static GameManager GameManager => GameManager.Instance;
        private static Player Player => GameManager.Player;
        private static InteractableObject[] InteractableObjectsOnMap =>
            FindObjectsByType<InteractableObject>(FindObjectsSortMode.None)
                .Where(x => x.transform.Cast<Transform>().Any(child => child.gameObject.activeInHierarchy))
                .ToArray();

        private bool ShouldHandleSaintCollection => SaintShowcaseHandler.Instance != null && SaintShowcaseHandler.Instance.isActiveAndEnabled;
        private bool ShouldHandleConversation => CustomEventPopup.IsDisplaying && _currentCustomEventPopup != null;
        /// <summary>
        /// This is the window that shows when you exit the monastery to select delivery supplies
        /// </summary>
        private bool ShouldHandlePackageSelector => _packageSelector != null && _packageSelector.isActiveAndEnabled;
        /// <summary>
        /// This is the window that shows when you exit the monastery to select passive abilities like extra coin, faster build times, or relationship bonuses
        /// </summary>
        private bool ShouldHandleProvisionsSelector => _provisionsPopUp != null;
        /// <summary>
        /// This is the menu that shows inventory, current objectives, ailments, and sacred artifacts
        /// </summary>
        private bool IsShowingObjectiveInventoryPopup => UI.Instance?.InventoryPopup != null && UI.Instance.InventoryPopup.activeSelf;
        /// <summary>
        /// We have finished the game and are now showing the end game dialog
        /// </summary>
        private bool IsEndGameConversationPopup => MissionManager.Instance.CurrentMissionId > GameDataManager.MAX_MISSION_ID && ShouldHandleConversation;
        private bool ShouldHandlerReturnEarly =>
            (Gamepad.current == null && !GameSettings.Instance.IsXboxMode) ||
            Player == null ||
            !GameSettings.Instance.IsUsingController ||
            (MissionManager.MissionOver && !IsEndGameConversationPopup) ||
            PauseMenu.Instance.active;



        // Start is called before the first frame update
        void Start()
        {
            Instance = this;

            if (GameSettings.Instance.IsXboxMode)
            {
#if MICROSOFT_GDK_SUPPORT
                UnityEngine.WindowsGames.WindowsGamesPLM.OnSuspendingEvent += WindowsGamesPLM_OnSuspendingEvent;
#endif
            }
            else
            {
                OnInputMethodChanged += HandleInputMethodChanged;
            }
        }

        public void OnDisable()
        {
            if (GameSettings.Instance.IsXboxMode)
            {
#if MICROSOFT_GDK_SUPPORT
                UnityEngine.WindowsGames.WindowsGamesPLM.OnSuspendingEvent -= WindowsGamesPLM_OnSuspendingEvent;
#endif
            }
            else
            {
                OnInputMethodChanged -= HandleInputMethodChanged;
            }
        }

        private void WindowsGamesPLM_OnSuspendingEvent()
        {
#if MICROSOFT_GDK_SUPPORT
            UnityEngine.WindowsGames.WindowsGamesPLM.AmReadyToSuspendNow();
#endif
        }

        /// <summary>
        /// This is a move of last resort. There are very VERY few cases where we would need to call this directly.
        /// </summary>
        public void AlertNoControllerDetected()
        {
            OnInputMethodChanged?.Invoke(false);
        }

        public void HandleInputMethodChanged(bool isUsingController)
        {
            if (ShouldHandlerReturnEarly) return;



            //todo: Show and Hide button prompts

            if (isUsingController)
            {

            }
            else
            {
                if (ShouldHandleConversation)
                {
                    var toolTipMouseOvers = _currentCustomEventPopup.GetComponentsInChildren<TooltipMouseOver>(false)
                        .Where(x => x.HasControllerHover)
                        .ToArray();

                    foreach (var tooltipMouseOver in toolTipMouseOvers)
                    {
                        tooltipMouseOver.HandleControllerExit();
                    }

                }
                else if (ShouldHandlePackageSelector)
                {
                    DeselectSelectedPackage();
                }
                else if (ShouldHandleProvisionsSelector)
                {
                    DeselectProvision();
                }
                else
                {
                    if (IsInBuilding)
                    {
                        DeselectBuildingButton();
                    }

                    if (IsShowingObjectiveInventoryPopup)
                    {
                        var inventoryPopup = UI.Instance.InventoryPopup.GetComponent<InventoryPopup>();
                        if (inventoryPopup == null) return;

                        DeselectInventoryItem();

                        var scrollRect = inventoryPopup.Tabs[3].GetComponentInChildren<ScrollRect>();
                        if (scrollRect == null) return;

                        HighlightArtifact(scrollRect, false);
                    }
                }
            }
        }


        // Update is called once per frame
        void Update()
        {
            // Check if mouse has moved, so we can switch to mouse mode
            if (GameSettings.Instance.IsUsingController && !GameSettings.Instance.IsXboxMode)
            {
                //var currentMousePosition = Mouse.current.position.ReadValue();
                var mouseX = Input.GetAxis("Mouse X");
                var mouseY = Input.GetAxis("Mouse Y");
                if (mouseX != 0 || mouseY != 0 || Gamepad.current == null)
                {
                    OnInputMethodChanged?.Invoke(false);
                    return;
                }
            }
            //else check if any controller button is pressed, so we can switch to controller mode
            else if (!GameSettings.Instance.IsUsingController)
            {
                var button = GamePadController.GetButton();
                var direction = GamePadController.GetDirection();

                if (button.Button != GamePadButton.Void || direction.Input != DirectionInput.Void)
                {
                    OnInputMethodChanged?.Invoke(true);
                    return;
                }
            }

            if (ShouldHandlerReturnEarly) return;

            if (Player.Animator.GetCurrentAnimatorClipInfo(0)[0].clip.name.Equals("Jump", StringComparison.InvariantCultureIgnoreCase) &&
                !Player.StatusEffects.Contains(PlayerStatusEffect.FROZEN) &&
                !IsInBuilding
                )
            {
                return;
            }

            var pressedButton = GamePadController.GetButton();
            if (PauseMenu.Instance.active)
            {
                DeselectBuildingButton();
                _currentPopUIButton = null;
            }
            else if (UI.Instance.SacredItemPopup.activeInHierarchy)
            {
                if (pressedButton.Button == GamePadButton.East && pressedButton.Control.WasPressedThisFrame)
                {
                    UI.Instance.SacredItemPopup.GetComponent<SacredItemPopup>().Close();
                }
            }
            else if (ShouldHandleSaintCollection)
            {
                HandleSaintCollectionPopup();
            }
            else if (ShouldHandleConversation)
            {
                HandleConversationEventPopup();
            }
            else if (ShouldHandlePackageSelector)
            {
                HandlePackageItemSelection();
            }
            else if (ShouldHandleProvisionsSelector)
            {
                HandleProvisionSelectPopup();
            }
            else if (pressedButton.Control.WasPressedThisFrame &&
                     (pressedButton.Button == GamePadButton.North || (pressedButton.Button == GamePadButton.East && IsShowingObjectiveInventoryPopup)))
            {
                if (IsShowingObjectiveInventoryPopup)
                {
                    DeselectInventoryItem();
                }

                UI.Instance.InventoryPopupEnable();

                var inventoryPopup = UI.Instance.InventoryPopup.GetComponent<InventoryPopup>();
                var backpackGameObject = inventoryPopup.Tabs[1].FindDeepChild("PackageUIItem");
                _currentInventoryGameObject = backpackGameObject;
            }
            else if (IsShowingObjectiveInventoryPopup)
            {
                HandleObjectiveInventoryPopup();
            }
            else if (IsInBuilding)
            {
                HandlePlayerActions();
            }
            else
            {
                HandlePlayerMovement();
            }

            if (!IsShowingObjectiveInventoryPopup)
            {
                HandleZoom();
            }

        }

        /// <summary>
        /// Sets the current PopUI when entering/exiting a building to enable/disable movement controls and enable/disable action button cycling.
        /// </summary>
        /// <param name="popUI">The PopUI with the action buttons</param>
        public void SetCurrentPopUI(PopUI popUI)
        {
            _currentPopUI = popUI;
            _currentPopUIButton = null;
        }

        /// <summary>
        /// Sets the current Custom Event Popup to handle conversations
        /// </summary>
        /// <param name="customEventPopup">The displayed Custom Event Pop Up</param>
        public void SetCurrentCustomEventPopup(CustomEventPopup customEventPopup)
        {
            try
            {
                if (_currentCustomEventPopup != null)
                {
                    var toolTipMouseOvers = _currentCustomEventPopup.GetComponentsInChildren<TooltipMouseOver>(false)
                        .OrderBy(x => x.name)
                        .ToArray();
                    toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerExit();
                }

                _currentCustomEventPopup = customEventPopup;
                _customEventPopupFirstActionButtonAcknowledged = false;
                _customEventLowerTooltips = _currentCustomEventPopup != null &&
                                            _currentCustomEventPopup.EventData.EventPopupType == EventPopupType.YESNO;

                if (customEventPopup == null) return;

                HandleConversationEventPopup();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
                //I'm fine with swallowing this exception, but we should log it.
            }
        }

        /// <summary>
        /// Sets the current Provision Popup to handle adding or upgrading 
        /// </summary>
        /// <param name="provisionsPopup"></param>
        public void SetCurrentProvisionsPopUp(ProvisionsPopup provisionsPopup)
        {
            _provisionsPopUp = provisionsPopup;
            if (_provisionsPopUp == null) return;

            _provisionItems = provisionsPopup.NewProvisionUIItems.Concat(provisionsPopup.UpgradeProvisionUIItems).Where(x => x.HasProvision).ToArray();

            if (GameSettings.Instance.IsUsingController)
            {
                SelectFirstProvision();
            }
        }

        /// <summary>
        /// Set a reference to the package selector when it's enabled so we can track selected and available items.
        /// </summary>
        /// <param name="packageSelector"></param>
        public void SetPackageSelector(PackageSelector packageSelector)
        {
            _packageSelector = packageSelector;
        }

        private void HandlePlayerMovement()
        {
            var direction = GamePadController.GetDirection();
            var pressedButton = GamePadController.GetButton();
            if (!direction.Control.WasPressedThisFrame && !pressedButton.Control.WasPressedThisFrame) return;

            if (pressedButton.Button == GamePadButton.South)
            {
                ProcessTileAction(Player.GetCurrentTile());
                return;
            }

            switch (direction.Input)
            {
                // Direction is rotated clockwise once because play facing direction is defined with Top left as up and bottom right as down
                // I guess we could support the option to have players choose.
                case DirectionInput.Up when Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.RIGHT, out var rightMapTile):
                    ProcessTileAction(rightMapTile);
                    break;
                case DirectionInput.Right when Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.DOWN, out var downMapTile):
                    ProcessTileAction(downMapTile);
                    break;
                case DirectionInput.Down when Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.LEFT, out var leftMapTile):
                    ProcessTileAction(leftMapTile);
                    break;
                case DirectionInput.Left when Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.UP, out var upMapTile):
                    ProcessTileAction(upMapTile);
                    break;
            }

            return;

            void ProcessTileAction(MapTile mapTile)
            {
                // This bit of logic here is to make sure we hop into buildings when we land on them.
                var occupyingBuilding = InteractableObjectsOnMap
                    .SingleOrDefault(x => x.CurrentGroundTile?.GetInstanceID() == mapTile.GetInstanceID());
                GameManager.OnMapTileTap(occupyingBuilding ?? mapTile);
            }
        }

        #region Objective & Inventory Popup
        private void HandleObjectiveInventoryPopup()
        {
            var pressedDirection = GamePadController.GetDirection();
            var pressedButton = GamePadController.GetButton();

            var inventoryPopup = UI.Instance.InventoryPopup.GetComponent<InventoryPopup>();
            if (inventoryPopup == null) return;

            var scrollRect = inventoryPopup.CurrentTab.TabIndex == 3 ?
                inventoryPopup.CurrentTab.GameObject.GetComponentInChildren<ScrollRect>() :
                null;
            HighlightArtifact(scrollRect, true);

            if (pressedDirection.Control.WasPressedThisFrame)
            {
                if (scrollRect != null && pressedDirection.Input is DirectionInput.Up or DirectionInput.Down && inventoryPopup.CurrentTab.TabIndex == 3)
                {
                    HighlightArtifact(scrollRect, false);
                    _objectiveInventoryScrollIndex = pressedDirection.Input switch
                    {
                        DirectionInput.Up => (_objectiveInventoryScrollIndex - 1 + scrollRect.content.childCount) % scrollRect.content.childCount,
                        DirectionInput.Down => (_objectiveInventoryScrollIndex + 1) % scrollRect.content.childCount,
                        _ => _objectiveInventoryScrollIndex
                    };

                    if (_objectiveInventoryScrollIndex == 0) _objectiveInventoryScrollIndex = pressedDirection.Input == DirectionInput.Up ?
                        scrollRect.content.childCount - 1 :
                            1; //skip the first item because it is a template

                    var normalizedPosition = (float)(_objectiveInventoryScrollIndex) / (scrollRect.content.childCount - 1);
                    scrollRect.verticalNormalizedPosition = 1f - normalizedPosition;
                    HighlightArtifact(scrollRect, true);

                }
            }
            else if (pressedButton.Control.WasPressedThisFrame)
            {
                if (pressedButton.Button is GamePadButton.LeftShoulder or GamePadButton.RightShoulder)
                {
                    HighlightArtifact(scrollRect, false);
                    _objectiveInventoryScrollIndex = 1;

                    switch (pressedButton.Button)
                    {
                        case GamePadButton.LeftShoulder:
                            inventoryPopup.PrevTab();
                            break;
                        case GamePadButton.RightShoulder:
                            inventoryPopup.NextTab();
                            break;
                    }
                }

            }

            if (inventoryPopup.CurrentTab.TabIndex == 1)
            {
                HandleBackPackNavigation();
            }

            return;

            void HandleBackPackNavigation()
            {
                _currentInventoryGameObject.GetComponent<TooltipMouseOver>().HandleControllerHover();
                if (pressedDirection.Input == DirectionInput.Void || !pressedDirection.Control.WasPressedThisFrame) return;

                DeselectInventoryItem();

                //get game objects nested in inventoryPopup that have a Tooltip Mouse Over Script attached
                var inventoryTooltips = inventoryPopup.CurrentTab.GameObject.GetComponentsInChildren<TooltipMouseOver>(true);

                if (!inventoryTooltips.Any()) return;

                var closestProvisionGameObject = pressedDirection.Input.GetClosestGameObjectOnCanvasInDirection(
                    _currentInventoryGameObject,
                    inventoryTooltips.Select(x => x.gameObject).ToArray());

                if (closestProvisionGameObject != null)
                {
                    _currentInventoryGameObject = closestProvisionGameObject;
                }

                if (_currentInventoryGameObject == null)
                {
                    _currentInventoryGameObject = _provisionItems.First().gameObject;
                }

                _currentInventoryGameObject.GetComponent<TooltipMouseOver>().HandleControllerHover();
            }
        }

        private void HighlightArtifact(ScrollRect scrollRect, bool setActive)
        {
            try
            {
                if (scrollRect == null) return;

                var artifact = scrollRect.content.GetChild(_objectiveInventoryScrollIndex);
                if (artifact == null || !artifact.gameObject.activeInHierarchy) return;

                artifact.gameObject.FindDeepChild("Square").GetComponent<Image>().color =
                    setActive ? Color.black : _defaultColour;
                artifact.gameObject.FindDeepChild("Image").GetComponent<Image>().color =
                    setActive ? _defaultColour : Color.white;
                artifact.gameObject.FindDeepChild("Title").GetComponent<TMP_Text>().fontSize = setActive ? 20 : 15;

                if (setActive) artifact.gameObject.GetComponent<SacredItemUIItem>().OnSelect();
            }
            catch (Exception e)
            {
                Debug.LogWarning(e);
            }
        }

        private void DeselectInventoryItem()
        {
            if (_currentInventoryGameObject != null)
            {
                _currentInventoryGameObject.GetComponent<TooltipMouseOver>().HandleControllerExit();
            }
        }
        #endregion

        #region BuildingMovementAndAction
        private void HandlePlayerActions()
        {
            var direction = GamePadController.GetDirection();
            if (direction.Control.WasPressedThisFrame)
            {
                HandleActionButtonNavigateWithDirection(direction.Input);
            }

            if (_currentPopUIButton == null || !_currentPopUIButton.HasControllerHover) return;

            var pressedButton = GamePadController.GetButton();
            if (pressedButton.Button == GamePadButton.South && pressedButton.Control.WasPressedThisFrame)
            {
                if (_currentPopUIButton.HasCriticalCircle)
                {
                    _currentPopUI.OnPointerDown(_currentPopUIButton.ButtonName);
                }
                else
                {
                    _currentPopUI.OnClick(_currentPopUIButton.ButtonName);
                    DeselectBuildingButton();
                }
            }
            else if (pressedButton.Button == GamePadButton.South && pressedButton.Control.WasReleasedThisFrame)
            {
                _currentPopUIButton.HandleControllerHover();
                if (_currentPopUIButton.HasCriticalCircle) _currentPopUI.OnPointerUp();
            }
            else if (pressedButton.Button == GamePadButton.East && pressedButton.Control.WasPressedThisFrame)
            {
                DeselectBuildingButton();
            }

            return;

            void HandleActionButtonNavigateWithDirection(DirectionInput directionInput)
            {
                var closestButtonGameObject = directionInput.GetClosestGameObjectOnCanvasInDirection(
                    _currentPopUIButton?.gameObject,
                    _currentPopUI.Buttons.Select(x => x.gameObject).ToArray(),
                    useAnchoredPosition: false);

                if (closestButtonGameObject != null)
                {
                    DeselectBuildingButton();
                    _currentPopUIButton = closestButtonGameObject.GetComponent<ActionButton>();
                }

                if (_currentPopUIButton == null)
                {
                    _currentPopUIButton = _currentPopUI.Buttons.FirstOrDefault();
                }

                _currentPopUIButton?.HandleControllerHover();
            }
        }

        private void DeselectBuildingButton()
        {
            _currentPopUIButton?.HandleControllerExit();
        }

        #endregion

        #region Conversations

        private void HandleConversationEventPopup()
        {
            TooltipMouseOver[] toolTipMouseOvers;

            if (_currentCustomEventPopup.EventData.EventPopupType == EventPopupType.OK)
            {
                //ordering by name because while Ok shows up before skip, skip shows up before continue in the array
                toolTipMouseOvers = _currentCustomEventPopup.GetComponentsInChildren<TooltipMouseOver>(false)
                    .OrderBy(x => x.name)
                    .ToArray();
            }
            else
            {
                if (_customEventLowerTooltips)
                {
                    toolTipMouseOvers = _currentCustomEventPopup.GetComponentsInChildren<TooltipMouseOver>(false)
                    .Where(x => x.name.Equals("YES", StringComparison.InvariantCultureIgnoreCase) || x.name.Equals("NO", StringComparison.InvariantCultureIgnoreCase))
                    .ToArray();
                }
                else
                {
                    toolTipMouseOvers = _currentCustomEventPopup.GetComponentsInChildren<TooltipMouseOver>(false)
                        .Where(x => !x.name.Equals("YES", StringComparison.InvariantCultureIgnoreCase) && !x.name.Equals("NO", StringComparison.InvariantCultureIgnoreCase))
                        .ToArray();
                }
            }

            if (!toolTipMouseOvers.Any()) return;

            if (!_customEventPopupFirstActionButtonAcknowledged)
            {
                _customEventPopupButtonIndex = 0;
                toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerHover();
                _customEventPopupFirstActionButtonAcknowledged = true;
            }
            else
            {
                var pressedDirection = GamePadController.GetDirection();
                var pressedButton = GamePadController.GetButton();

                if (pressedDirection.Control.WasPressedThisFrame)
                {
                    if (pressedDirection.Input == DirectionInput.Left)
                    {
                        HandleEventPopupButtonNavigate(-1);
                    }
                    else if (pressedDirection.Input == DirectionInput.Right)
                    {
                        HandleEventPopupButtonNavigate(1);
                    }
                    else if (_currentCustomEventPopup.EventData.EventPopupType == EventPopupType.YESNO)
                    {
                        toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerExit();
                        _customEventLowerTooltips = !_customEventLowerTooltips;
                        _customEventPopupFirstActionButtonAcknowledged = false;
                    }
                }
                else if (pressedButton.Button == GamePadButton.South)
                {
                    if (pressedButton.Control.WasPressedThisFrame)
                    {
                        if (_customEventLowerTooltips && _customEventPopupButtonIndex == 0)
                        {
                            _currentCustomEventPopup.OnPointerDown();
                        }
                        else
                        {
                            var button = toolTipMouseOvers[_customEventPopupButtonIndex]?.gameObject.GetComponentInChildren<Button>();
                            if (button != null) button.onClick.Invoke();
                        }
                    }
                    else if (pressedButton.Control.WasReleasedThisFrame)
                    {
                        if (_customEventLowerTooltips)
                        {
                            _currentCustomEventPopup.OnPointerUp();
                        }
                    }

                }
            }

            return;
            void HandleEventPopupButtonNavigate(int increment)
            {
                toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerExit();
                _customEventPopupButtonIndex = (_customEventPopupButtonIndex + increment + toolTipMouseOvers.Length) % toolTipMouseOvers.Length;
                toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerHover();
            }
        }
        #endregion

        #region Provisions

        private void HandleProvisionSelectPopup()
        {
            var pressedDirection = GamePadController.GetDirection();
            var pressedButton = GamePadController.GetButton();

            if (pressedDirection.Input != DirectionInput.Void && pressedDirection.Control.WasPressedThisFrame)
            {
                DeselectProvision();

                var closestProvisionGameObject = pressedDirection.Input.GetClosestGameObjectOnCanvasInDirection(
                    _currentProvisionUIItem?.gameObject,
                    _provisionItems.Select(x => x.gameObject).ToArray());
                if (closestProvisionGameObject != null)
                {
                    _currentProvisionUIItem = closestProvisionGameObject.GetComponentInChildren<ProvisionUIItem>();
                }

                if (_currentProvisionUIItem == null)
                {
                    _currentProvisionUIItem = _provisionItems.FirstOrDefault();
                }

                _currentProvisionUIItem?.HandleControllerHover();
            }
            else if (pressedButton.Control.WasPressedThisFrame)
            {
                if (pressedButton.Button == GamePadButton.East)
                {
                    var wasReplacePhase = _provisionsPopUp.PopupPhase == ProvisionsPopupPhase.REPLACE;
                    DeselectProvision();
                    _currentProvisionUIItem = null;
                    _provisionsPopUp.OnClick("X");

                    //If we are going back from the replace phase, set first available provision to hover
                    if (wasReplacePhase)
                    {
                        SelectFirstProvision();
                    }
                }
                else if (pressedButton.Button == GamePadButton.South && _currentProvisionUIItem != null)
                {
                    _currentProvisionUIItem.OnClick();
                    DeselectProvision();

                    //If we are at the limit and have to replace a provision, hover over the first existing provision
                    if (_currentProvisionUIItem.Type == ProvisionUIItemType.NEW &&
                        _provisionsPopUp != null &&
                        _provisionsPopUp.PopupPhase == ProvisionsPopupPhase.REPLACE)
                    {
                        SelectFirstProvision();
                    }
                    else
                    {
                        _currentProvisionUIItem = null;
                    }
                }
            }
        }

        private void SelectFirstProvision()
        {
            _currentProvisionUIItem = _provisionItems.FirstOrDefault(x => x.gameObject.activeInHierarchy);
            _currentProvisionUIItem?.HandleControllerHover();
        }

        private void DeselectProvision()
        {
            if (_currentProvisionUIItem != null)
            {
                _currentProvisionUIItem.EndControllerHover();
            }
        }


        #endregion

        #region SaintCollection


        private void HandleSaintCollectionPopup()
        {
            var saintPopup = SaintShowcaseHandler.Instance;
            var pressedDirection = GamePadController.GetDirection();
            var pressedButton = GamePadController.GetButton();

            if (pressedButton.Button == GamePadButton.East && pressedButton.Control.WasPressedThisFrame)
            {
                saintPopup.OnExit();
                _hasHoveredFirstSaintButton = false;
                return;
            }

            if (!SaintsManager.Instance.UnlockedSaints.Any()) return;

            if (SaintsManager.Instance.UnlockedSaints.Count > 1)
            {

                var nextOptionTransform = saintPopup.gameObject.FindDeepChild("No").transform;
                var previousOptionTransform = saintPopup.gameObject.FindDeepChild("Yes").transform;

                if (!_hasHoveredFirstSaintButton)
                {
                    HoverButton(nextOptionTransform, null);
                    _hasHoveredFirstSaintButton = true;
                    _saintNextOptionHasHover = true;
                    return;
                }

                if (pressedDirection.Control.WasPressedThisFrame)
                {
                    if (pressedDirection.Input == DirectionInput.Right && !_saintNextOptionHasHover)
                    {
                        _saintNextOptionHasHover = true;
                        HoverButton(nextOptionTransform, previousOptionTransform);
                    }
                    else if (pressedDirection.Input == DirectionInput.Left && _saintNextOptionHasHover)
                    {
                        _saintNextOptionHasHover = false;
                        HoverButton(previousOptionTransform, nextOptionTransform);
                    }
                }
            }

            if (pressedDirection.Input is DirectionInput.Up or DirectionInput.Down)
            {
                const float scrollFactor = 0.02f;
                var scrollView = saintPopup.gameObject.FindDeepChild("Scroll View");
                var scrollRect = scrollView.GetComponent<ScrollRect>();

                if (pressedDirection.Input == DirectionInput.Up)
                {
                    scrollRect.verticalNormalizedPosition = Math.Min(scrollRect.verticalNormalizedPosition + scrollFactor, 1f);
                }
                else if (pressedDirection.Input == DirectionInput.Down)
                {
                    scrollRect.verticalNormalizedPosition = Math.Max(scrollRect.verticalNormalizedPosition - scrollFactor, 0f);
                }
            }

            if (pressedButton.Button == GamePadButton.South && pressedButton.Control.WasPressedThisFrame)
            {
                if (_saintNextOptionHasHover)
                {
                    saintPopup.ShowNextSaint();
                }
                else
                {
                    saintPopup.ShowPreviusSaint();
                }
            }

            return;

            void HoverButton(Transform newHoveredOption, Transform oldHoveredOption)
            {
                const float transformScale = 1.25f;
                oldHoveredOption?.DOComplete();
                oldHoveredOption?.DOScale(oldHoveredOption.localScale / transformScale, 0.5f);

                newHoveredOption.DOComplete();
                newHoveredOption.DOScale(newHoveredOption.localScale * transformScale, 0.5f);
            }
        }
        #endregion

        #region PackageSelection
        private const float PackageScaleValue = 1.25f;
        private void HandlePackageItemSelection()
        {
            var pressedDirection = GamePadController.GetDirection();
            var pressedButton = GamePadController.GetButton();

            if (pressedDirection.Input != DirectionInput.Void && pressedDirection.Control.WasPressedThisFrame)
            {
                DeselectSelectedPackage();

                var gameObjects = _packageSelector.ItemList.Concat(_packageSelector.InstantiatedGos)
                    .Select(x => x.gameObject).ToList();
                gameObjects.Add(_packageSelector.ExitGameObject);

                var referenceGameObject = _currentPackageItem == null ? null : _currentPackageItem.gameObject;
                var closestPackageItemGameObject = pressedDirection.Input.GetClosestGameObjectOnCanvasInDirection(referenceGameObject, gameObjects.ToArray());

                if (closestPackageItemGameObject != null)
                {
                    _selectedPackageSelectorItemIsPackage = closestPackageItemGameObject.TryGetComponent(out _currentPackageItem);
                }
                else if (_currentPackageItem == null)
                {
                    _currentPackageItem = _packageSelector.ItemList[0];
                    _selectedPackageSelectorItemIsPackage = true;
                }

                if (_selectedPackageSelectorItemIsPackage)
                {
                    _currentPackageItem?.GetComponent<TooltipMouseOver>().HandleControllerHover();
                }
                else
                {
                    _packageSelector.ExitGameObject.transform.DOComplete();
                    _packageSelector.ExitGameObject.transform.DOScale(_packageSelector.ExitGameObject.transform.localScale * PackageScaleValue, 0.5f);
                }
            }
            else if (pressedButton.Control.WasPressedThisFrame)
            {

                if (pressedButton.Button == GamePadButton.East)
                {
                    DeselectSelectedPackage();
                    _currentPackageItem = null;
                    _packageSelector.Cancel();
                }
                else if (pressedButton.Button == GamePadButton.South)
                {
                    DeselectSelectedPackage();

                    if (_selectedPackageSelectorItemIsPackage)
                    {

                        if (_packageSelector.ItemList.Contains(_currentPackageItem))
                        {
                            _currentPackageItem.Deselect();
                        }
                        else
                        {
                            _currentPackageItem.Select();
                        }

                        _currentPackageItem = _packageSelector.ItemList[0];
                        _currentPackageItem.GetComponent<TooltipMouseOver>().HandleControllerHover();
                    }
                    else
                    {
                        _packageSelector.GoToWorld();
                    }
                }
            }

        }

        private void DeselectSelectedPackage()
        {
            if (_selectedPackageSelectorItemIsPackage)
            {
                if (_currentPackageItem != null)
                {
                    _currentPackageItem.GetComponent<TooltipMouseOver>().HandleControllerExit();
                }
            }
            else
            {
                // Exit button does not wiggle on mouse hover, so this will shrink the button if it's active on switch to controller
                _packageSelector.ExitGameObject.transform.DOComplete();
                _packageSelector.ExitGameObject.transform.DOScale(_packageSelector.ExitGameObject.transform.localScale / PackageScaleValue, 0.5f);
            }
        }
        #endregion

        private void HandleZoom()
        {
            var pressedButton = GamePadController.GetButton();
            if (!pressedButton.Control.WasPressedThisFrame) return;

            if (pressedButton.Button == GamePadButton.RightShoulder)
            {
                GameControlsManager.TryZoom?.Invoke(1);
            }
            else if (pressedButton.Button == GamePadButton.LeftShoulder)
            {
                DeselectBuildingButton();
                GameControlsManager.TryZoom?.Invoke(-1);
            }
        }
    }
}
