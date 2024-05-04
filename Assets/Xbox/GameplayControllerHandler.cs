using System;
using System.Linq;
using Assets._Scripts.Extensions;
using DG.Tweening;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace Assets.Xbox
{
    public class GameplayControllerHandler : MonoBehaviour
    {
        public float ButtonAxisWeight = 0.05f;

        public static GameplayControllerHandler Instance { get; private set; }

        private PopUI _currentPopUI;

        private CustomEventPopup _currentCustomEventPopup;
        private bool _customEventPopupFirstActionButtonAcknowledged;
        private int _customEventPopupButtonIndex;

        [CanBeNull] private ActionButton _currentPopUIButton;
        private bool IsInBuilding => _currentPopUI != null;

        private ProvisionsPopup _provisionsPopUp;
        private int _provisionPopupButtonIndex;
        private ProvisionUIItem[] _provisionItems;
        private ProvisionUIItem CurrentProvisionUIItem => _provisionItems[_provisionPopupButtonIndex];

        private bool _hasHoveredFirstSaintButton;
        private bool _saintNextOptionHasHover;

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
            if (Gamepad.current == null || Player == null || !GameSettings.Instance.IsXboxMode) return;

            if (SaintShowcaseHandler.Instance != null && SaintShowcaseHandler.Instance.isActiveAndEnabled)
            {
                HandleSaintCollectionPopup();
            }
            else if (CustomEventPopup.IsDisplaying && _currentCustomEventPopup != null)
            {
                HandleConversationEventPopup();
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
        public void SetCurrentPopUI([CanBeNull] PopUI popUI)
        {
            if (!GameSettings.Instance.IsXboxMode) return;

            _currentPopUI = popUI;
            _currentPopUIButton = null;
            Debug.Log($"New Gameplay POP UI: {popUI?.name}");
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
                toolTipMouseOvers[_customEventPopupButtonIndex].EndControllerTooltip();
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
            Debug.Log($"New Provisions Popup: {provisionsPopup?.name}");

            if (provisionsPopup == null) return;

            _provisionItems = provisionsPopup.NewProvisionUIItems.Concat(provisionsPopup.UpgradeProvisionUIItems).Where(x => x.HasProvision).ToArray();
            _provisionPopupButtonIndex = 0;
            _provisionItems[_provisionPopupButtonIndex].HandleControllerHover();
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
                    }
                    _currentPopUIButton.HandleControllerExit();
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
                var closestObjectDistance = double.MaxValue;
                ActionButton selectedActionButton = null;

                foreach (var actionButton in _currentPopUI.Buttons)
                {
                    if (actionButton == _currentPopUIButton) continue;

                    var currentPosition = _currentPopUIButton == null ? new Vector3(0, 0, 0) : _currentPopUIButton.transform.localPosition;
                    var vectorToTarget = actionButton.transform.localPosition - currentPosition;

                    var isDesiredDirection = direction switch
                    {
                        DPadDirection.Up => vectorToTarget.y > 0,
                        DPadDirection.Down => vectorToTarget.y < 0,
                        DPadDirection.Left => vectorToTarget.x < 0,
                        DPadDirection.Right => vectorToTarget.x > 0,
                        _ => false
                    };

                    if (!isDesiredDirection) continue;

                    var weight = direction switch
                    {
                        DPadDirection.Up or DPadDirection.Down => new Vector3(1, ButtonAxisWeight, 1),
                        DPadDirection.Left or DPadDirection.Right => new Vector3(ButtonAxisWeight, 1, 1),
                        _ => new Vector3(1, 1, 1)
                    };

                    // Apply direction weight to vectorToTarget and square it to calculate distance of vector.
                    var distance = Math.Pow(vectorToTarget.x * weight.x, 2) + Math.Pow(vectorToTarget.y * weight.y, 2) + Math.Pow(vectorToTarget.z * weight.z, 2);
                    if (!(distance < closestObjectDistance)) continue;

                    closestObjectDistance = distance;
                    selectedActionButton = actionButton;
                }

                if (selectedActionButton != null)
                {
                    _currentPopUIButton?.HandleControllerExit();
                    _currentPopUIButton = selectedActionButton;
                }

                if (_currentPopUIButton == null)
                {
                    _currentPopUIButton = _currentPopUI.Buttons.FirstOrDefault();
                }

                Debug.Log($"Current Pop Action is: {_currentPopUIButton?.ButtonName}");
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
                    toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerTooltip();
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
                        toolTipMouseOvers[_customEventPopupButtonIndex].EndControllerTooltip();
                        _customEventPopupButtonIndex = (_customEventPopupButtonIndex + increment + toolTipMouseOvers.Length) % toolTipMouseOvers.Length;
                        toolTipMouseOvers[_customEventPopupButtonIndex].HandleControllerTooltip();
                        Debug.Log($"Current Event Popup Button is: {toolTipMouseOvers[_customEventPopupButtonIndex].name}");
                    }
                }

            }
        }

        private void HandleProvisionSelectPopup()
        {
            if (DPad.left.wasPressedThisFrame)
            {
                HandleProvisionButtonNavigate(-1);
            }
            else if (DPad.right.wasPressedThisFrame)
            {
                HandleProvisionButtonNavigate(1);
            }
            else if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                CurrentProvisionUIItem.OnClick();
                _provisionPopupButtonIndex = 0;
            }

            void HandleProvisionButtonNavigate(int increment)
            {
                CurrentProvisionUIItem.EndControllerHover();
                _provisionPopupButtonIndex = (_provisionPopupButtonIndex + increment + _provisionItems.Length) % _provisionItems.Length;
                CurrentProvisionUIItem.HandleControllerHover();
                Debug.Log($"Current Provision Button is: {CurrentProvisionUIItem.name}" +
                          $" [{Enum.GetName(typeof(ProvisionUIItemType), CurrentProvisionUIItem.Type)}]");
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

            void HoverButton(Transform newHoveredOption, [CanBeNull] Transform oldHoveredOption)
            {
                const float transformScale = 1.25f;
                oldHoveredOption?.DOComplete();
                oldHoveredOption?.DOScale(oldHoveredOption.localScale / transformScale, 0.5f);

                newHoveredOption.DOComplete();
                newHoveredOption.DOScale(newHoveredOption.localScale * transformScale, 0.5f);
            }
        }
    }
}
