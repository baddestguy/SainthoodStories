using System;
using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;
using UnityEngine.UI;

namespace Assets.Xbox
{
    public class GameplayControllerHandler : MonoBehaviour
    {
        public static GameplayControllerHandler Instance { get; private set; }

        private bool _popUIFirstActionButtonAcknowledged;
        private PopUI _currentPopUI;
        private int _buildingActionButtonIndex;

        private CustomEventPopup _currentCustomEventPopup;
        private bool _customEventPopupFirstActionButtonAcknowledged;
        private int _customEventPopupButtonIndex;
        private bool IsInBuilding => _currentPopUI != null;
        private ActionButton CurrentPopUIButton => _currentPopUI.Buttons[_buildingActionButtonIndex];
        //private TooltipMouseOver[] ConversationToolTipMouseOvers => _currentCustomEventPopup.GetComponentsInChildren<TooltipMouseOver>(false);

        private ProvisionsPopup _provisionsPopUp;
        private int _provisionPopupButtonIndex;
        private ProvisionUIItem[] _provisionItems;
        private ProvisionUIItem CurrentProvisionUIItem => _provisionItems[_provisionPopupButtonIndex];

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
            if (Gamepad.current == null || Player == null) return;

            if (CustomEventPopup.IsDisplaying && _currentCustomEventPopup != null)
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
            _currentPopUI = popUI;
            _popUIFirstActionButtonAcknowledged = false;
            Debug.Log($"New Gameplay POP UI: {popUI?.name}");
            if (_currentPopUI == null) return;

            //I have this as separate for now in case I want to do something between the two if statements
            if (!_currentPopUI.Buttons.Any()) return;

            AcknowledgeFirstPopUIButton();
        }

        /// <summary>
        /// Sets the current Custom Event Popup to handle conversations
        /// </summary>
        /// <param name="customEventPopup">The displayed Custom Event Pop Up</param>
        public void SetCurrentCustomEventPopup(CustomEventPopup customEventPopup)
        {
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
            
            if(provisionsPopup == null) return;

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
            if (!_popUIFirstActionButtonAcknowledged && _currentPopUI.Buttons.Any())
            {
                AcknowledgeFirstPopUIButton();
            }

            if (DPad.left.wasPressedThisFrame)
            {
                HandleActionButtonNavigate(-1);
            }
            else if (DPad.right.wasPressedThisFrame)
            {
                HandleActionButtonNavigate(1);
            }

            if (Gamepad.current.buttonSouth.wasPressedThisFrame)
            {
                CurrentPopUIButton.HandleControllerExit();
                if (CurrentPopUIButton.HasCriticalCircle)
                {
                    _currentPopUI.OnPointerDown(CurrentPopUIButton.ButtonName);
                }
                else
                {
                    _currentPopUI.OnClick(CurrentPopUIButton.ButtonName);
                }
            }
            else if (Gamepad.current.buttonSouth.wasReleasedThisFrame)
            {
                CurrentPopUIButton.HandleControllerHover();
                if (CurrentPopUIButton.HasCriticalCircle) _currentPopUI.OnPointerUp();
            }

            return;


            void HandleActionButtonNavigate(int indexIncrement)
            {
                CurrentPopUIButton.HandleControllerExit();

                //Add the increment but keep the index within the range of action button count using the mod factor
                _buildingActionButtonIndex = (_buildingActionButtonIndex + indexIncrement + _currentPopUI.Buttons.Count) % _currentPopUI.Buttons.Count;

                Debug.Log($"Current Pop Action is: {CurrentPopUIButton.ButtonName}");
                CurrentPopUIButton.HandleControllerHover();
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
                //tooltipMouseOvers[_customEventPopupButtonIndex].gameObject.GetComponentInChildren<Button>().onClick.Invoke();
                //_customEventPopupFirstActionButtonAcknowledged = false;
                _provisionPopupButtonIndex = 0;
            }

            void HandleProvisionButtonNavigate(int increment)
            {
                CurrentProvisionUIItem.EndControllerHover();
                _provisionPopupButtonIndex = (_provisionPopupButtonIndex + increment + _provisionItems.Length) % _provisionItems.Length;
                CurrentProvisionUIItem.HandleControllerHover();
                Debug.Log($"Current Provision Button is: {CurrentProvisionUIItem.name}" +
                          $" [{Enum.GetName(typeof(ProvisionUIItemType),CurrentProvisionUIItem.Type)}]");
            }
        }

        private void AcknowledgeFirstPopUIButton()
        {
            _popUIFirstActionButtonAcknowledged = true;
            _buildingActionButtonIndex = 0;
            CurrentPopUIButton.HandleControllerHover();
        }
    }
}
