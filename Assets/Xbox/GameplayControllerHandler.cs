using System;
using System.Linq;
using Assets._Scripts.Extensions;
using DG.Tweening;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace Assets.Xbox
{
    /// <summary>
    /// Responsible for managing game pad input during game play,
    /// </summary>
    public class GameplayControllerHandler : MonoBehaviour
    {
        public static GameplayControllerHandler Instance { get; private set; }

        private PopUI _currentPopUI;

        private CustomEventPopup _currentCustomEventPopup;
        private bool _customEventPopupFirstActionButtonAcknowledged;
        private int _customEventPopupButtonIndex;

        private ActionButton _currentPopUIButton;
        private bool IsInBuilding => _currentPopUI != null;

        private ProvisionsPopup _provisionsPopUp;
        private ProvisionUIItem[] _provisionItems;
        private ProvisionUIItem _currentProvisionUIItem;

        private bool _hasHoveredFirstSaintButton;
        private bool _saintNextOptionHasHover;

        private PackageItem _currentPackageItem;
        private PackageSelector _packageSelector;
        private bool _selectedPackageSelectorItemIsPackage;

        private static DpadControl DPad => Gamepad.current.dpad;
        private static GameManager GameManager => GameManager.Instance;
        private static Player Player => GameManager.Player;
        private static InteractableObject[] InteractableObjectsOnMap =>
            FindObjectsByType<InteractableObject>(FindObjectsSortMode.None)
                .Where(x => x.transform.Cast<Transform>().Any(child => child.gameObject.activeInHierarchy))
                .ToArray();


        private void Awake()
        {
            Instance = this;
        }

        // Start is called before the first frame update
        void Start()
        {
        }


        // Update is called once per frame
        void Update()
        {
            if (Gamepad.current == null || Player == null || !GameSettings.Instance.IsXboxMode || MissionManager.MissionOver) return;

            if (PauseMenu.Instance.active)
            {
                _currentPopUIButton?.HandleControllerExit();
                _currentPopUIButton = null;
            }
            else if (SaintShowcaseHandler.Instance != null && SaintShowcaseHandler.Instance.isActiveAndEnabled)
            {
                HandleSaintCollectionPopup();
            }
            else if (CustomEventPopup.IsDisplaying && _currentCustomEventPopup != null)
            {
                HandleConversationEventPopup();
            }
            else if (_packageSelector?.isActiveAndEnabled ?? false)
            {
                HandlePackageItemSelection();
            }
            else if (_provisionsPopUp != null)
            {
                HandleProvisionSelectPopup();
            }
            else if (IsInBuilding)
            {
                HandlePlayerActions();
            }
            else
            {
                HandlePlayerMovement();
            }

            HandleZoom();
        }

        /// <summary>
        /// Sets the current PopUI when entering/exiting a building to enable/disable movement controls and enable/disable action button cycling.
        /// </summary>
        /// <param name="popUI">The PopUI with the action buttons</param>
        public void SetCurrentPopUI(PopUI popUI)
        {
            if (!GameSettings.Instance.IsXboxMode) return;

            _currentPopUI = popUI;
            _currentPopUIButton = null;
        }

        /// <summary>
        /// Sets the current Custom Event Popup to handle conversations
        /// </summary>
        /// <param name="customEventPopup">The displayed Custom Event Pop Up</param>
        public void SetCurrentCustomEventPopup(CustomEventPopup customEventPopup)
        {
            if (!GameSettings.Instance.IsXboxMode) return;

            if (_currentCustomEventPopup != null)
            {
                var toolTipMouseOvers = _currentCustomEventPopup.GetComponentsInChildren<TooltipMouseOver>(false)
                    .OrderBy(x => x.name)
                    .ToArray();
                toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerExit();
            }

            _currentCustomEventPopup = customEventPopup;
            _customEventPopupFirstActionButtonAcknowledged = false;
            HandleConversationEventPopup();

            Debug.Log($"New Custom Event Popup: {_currentCustomEventPopup?.name}");
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
            // Direction is rotated clockwise once because play facing direction is defined with Top left as up and bottom right as down
            // I guess we could support the option to have players choose.
            if (DPad.up.wasPressedThisFrame && Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.RIGHT, out var rightMapTile))
            {
                ProcessTileAction(rightMapTile);
            }
            else if (DPad.right.wasPressedThisFrame && Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.DOWN, out var downMapTile))
            {
                ProcessTileAction(downMapTile);
            }
            else if (DPad.down.wasPressedThisFrame && Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.LEFT, out var leftMapTile))
            {
                ProcessTileAction(leftMapTile);
            }
            else if (DPad.left.wasPressedThisFrame && Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.UP, out var upMapTile))
            {
                ProcessTileAction(upMapTile);
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

        private void HandlePlayerActions()
        {
            var directionPressed = DPad.GetDirection();
            if (directionPressed != null)
            {
                HandleActionButtonNavigateWithDirection(directionPressed.Value);
            }

            if (_currentPopUIButton != null)
            {
                if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                {
                    if (_currentPopUIButton.HasCriticalCircle)
                    {
                        _currentPopUI.OnPointerDown(_currentPopUIButton.ButtonName);
                    }
                    else
                    {
                        _currentPopUI.OnClick(_currentPopUIButton.ButtonName);
                        _currentPopUIButton.HandleControllerExit();
                    }
                }
                else if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
                {
                    _currentPopUIButton.HandleControllerHover();
                    if (_currentPopUIButton.HasCriticalCircle) _currentPopUI.OnPointerUp();
                }

                if (Gamepad.current.buttonEast.wasPressedThisFrame)
                {
                    _currentPopUIButton.HandleControllerExit();
                }
            }

            return;

            void HandleActionButtonNavigateWithDirection(DPadDirection direction)
            {
                var closestButtonGameObject = direction.GetClosestGameObjectOnCanvasInDirection(
                    _currentPopUIButton?.gameObject,
                    _currentPopUI.Buttons.Select(x => x.gameObject).ToArray());

                if (closestButtonGameObject != null)
                {
                    _currentPopUIButton?.HandleControllerExit();
                    _currentPopUIButton = closestButtonGameObject.GetComponent<ActionButton>();
                }

                if (_currentPopUIButton == null)
                {
                    _currentPopUIButton = _currentPopUI.Buttons.FirstOrDefault();
                }

                _currentPopUIButton?.HandleControllerHover();
            }
        }

        private void HandleZoom()
        {
            if (Gamepad.current.rightShoulder.wasPressedThisFrame || Gamepad.current.rightTrigger.wasPressedThisFrame)
            {
                GameControlsManager.TryZoom?.Invoke(1);
            }
            else if (Gamepad.current.leftShoulder.wasPressedThisFrame || Gamepad.current.leftTrigger.wasPressedThisFrame)
            {
                _currentPopUIButton?.HandleControllerExit();
                GameControlsManager.TryZoom?.Invoke(-1);
            }
        }

        private void HandleConversationEventPopup()
        {
            if (_currentCustomEventPopup == null) return;

            if (_currentCustomEventPopup.EventData.EventPopupType == EventPopupType.OK)
            {
                //ordering by name because while Ok shows up before skip, skip shows up before continue in the array
                var toolTipMouseOvers = _currentCustomEventPopup.GetComponentsInChildren<TooltipMouseOver>(false)
                    .OrderBy(x => x.name)
                    .ToArray();

                if (!_customEventPopupFirstActionButtonAcknowledged)
                {
                    _customEventPopupButtonIndex = 0;
                    toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerHover();
                    _customEventPopupFirstActionButtonAcknowledged = true;
                }
                else
                {
                    if (DPad.left.wasPressedThisFrame)
                    {
                        HandleEventPopupButtonNavigate(-1);
                    }
                    else if (DPad.right.wasPressedThisFrame)
                    {
                        HandleEventPopupButtonNavigate(1);
                    }
                    else if (Gamepad.current.buttonSouth.wasPressedThisFrame)
                    {
                        toolTipMouseOvers[_customEventPopupButtonIndex].gameObject.GetComponentInChildren<Button>().onClick.Invoke();
                    }


                    void HandleEventPopupButtonNavigate(int increment)
                    {
                        toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerExit();
                        _customEventPopupButtonIndex = (_customEventPopupButtonIndex + increment + toolTipMouseOvers.Length) % toolTipMouseOvers.Length;
                        toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerHover();
                        Debug.Log($"Current Event Popup Button is: {toolTipMouseOvers[_customEventPopupButtonIndex].name}");
                    }
                }

            }
        }

        private void HandleProvisionSelectPopup()
        {
            var direction = DPad.GetDirection();
            if (direction != null)
            {
                _currentProvisionUIItem?.EndControllerHover();
                var closestProvisionGameObject = direction.Value.GetClosestGameObjectOnCanvasInDirection(
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
            else if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                _currentProvisionUIItem?.EndControllerHover();
                _currentProvisionUIItem = null;
                _provisionsPopUp.OnClick("X");
            }
            else if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                _currentProvisionUIItem?.OnClick();
                _currentProvisionUIItem?.EndControllerHover();
                _currentProvisionUIItem = null;
            }
        }

        private void HandleSaintCollectionPopup()
        {
            var saintPopup = SaintShowcaseHandler.Instance;
            if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                saintPopup.OnExit();
                _hasHoveredFirstSaintButton = false;
                return;
            }

            if (!SaintsManager.Instance.UnlockedSaints.Any()) return;

            var nextOptionTransform = saintPopup.gameObject.FindDeepChild("No").transform;
            var previousOptionTransform = saintPopup.gameObject.FindDeepChild("Yes").transform;

            if (!_hasHoveredFirstSaintButton)
            {
                HoverButton(nextOptionTransform, null);
                _hasHoveredFirstSaintButton = true;
                _saintNextOptionHasHover = true;
                return;
            }

            if (DPad.GetDirection() == DPadDirection.Right && !_saintNextOptionHasHover)
            {
                _saintNextOptionHasHover = true;
                HoverButton(nextOptionTransform, previousOptionTransform);
            }
            else if (DPad.GetDirection() == DPadDirection.Left && _saintNextOptionHasHover)
            {
                _saintNextOptionHasHover = false;
                HoverButton(previousOptionTransform, nextOptionTransform);
            }

            if (DPad.IsVerticalPress(false))
            {
                const float scrollFactor = 0.02f;
                var scrollView = saintPopup.gameObject.FindDeepChild("Scroll View");
                var scrollRect = scrollView.GetComponent<ScrollRect>();

                if (DPad.GetDirection(false) == DPadDirection.Up)
                {
                    scrollRect.verticalNormalizedPosition = Math.Min(scrollRect.verticalNormalizedPosition + scrollFactor, 1f);
                }
                else if (DPad.GetDirection(false) == DPadDirection.Down)
                {
                    scrollRect.verticalNormalizedPosition = Math.Max(scrollRect.verticalNormalizedPosition - scrollFactor, 0f);
                }
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
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

        private void HandlePackageItemSelection()
        {
            const float scaleValue = 1.25f;
            var direction = DPad.GetDirection();

            if (direction != null)
            {
                if (_selectedPackageSelectorItemIsPackage)
                {
                    _currentPackageItem?.GetComponent<TooltipMouseOver>().HandleControllerExit();
                }
                else
                {
                    _packageSelector.ExitGameObject.transform.DOComplete();
                    _packageSelector.ExitGameObject.transform.DOScale(_packageSelector.ExitGameObject.transform.localScale / scaleValue, 0.5f);
                }

                var gameObjects = _packageSelector.ItemList.Concat(_packageSelector.AvailableItems)
                    .Select(x => x.gameObject).ToList();
                gameObjects.Add(_packageSelector.ExitGameObject);

                var closestPackageItemGameObject = direction.Value.GetClosestGameObjectOnCanvasInDirection(_currentPackageItem?.gameObject, gameObjects.ToArray());

                if (closestPackageItemGameObject != null)
                {
                    _selectedPackageSelectorItemIsPackage = closestPackageItemGameObject.TryGetComponent(out _currentPackageItem);
                }
                else if (_currentPackageItem == null)
                {
                    _currentPackageItem = _packageSelector.ItemList[0];
                }

                if (_selectedPackageSelectorItemIsPackage)
                {
                    _currentPackageItem?.GetComponent<TooltipMouseOver>().HandleControllerHover();
                }
                else
                {
                    _packageSelector.ExitGameObject.transform.DOComplete();
                    _packageSelector.ExitGameObject.transform.DOScale(_packageSelector.ExitGameObject.transform.localScale * scaleValue, 0.5f);
                }
            }
            else if (Gamepad.current.buttonEast.wasPressedThisFrame)
            {
                _currentPackageItem?.GetComponent<TooltipMouseOver>().HandleControllerExit();
                _currentPackageItem = null;
                _packageSelector.Cancel();
            }
            else if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                _currentPackageItem?.GetComponent<TooltipMouseOver>().HandleControllerExit();

                if (_selectedPackageSelectorItemIsPackage)
                {

                    if (_packageSelector.ItemList.Contains(_currentPackageItem))
                    {
                        _packageSelector.PackageDeselected(_currentPackageItem);
                    }
                    else
                    {
                        _packageSelector.PackageSelected(_currentPackageItem);
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
}
