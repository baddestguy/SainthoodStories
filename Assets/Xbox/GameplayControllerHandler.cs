using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Assets.Xbox
{
    public class GameplayControllerHandler : MonoBehaviour
    {
        public static GameplayControllerHandler Instance { get; private set; }

        private bool _popUIFirstActionButtonAcknowledged;
        private PopUI _currentPopUI;
        private int _buildingActionButtonIndex;

        private static DpadControl DPad => Gamepad.current.dpad;
        private static GameManager GameManager => GameManager.Instance;
        private static Player Player => GameManager.Player;
        private static InteractableObject[] InteractableObjectsOnMap =>
            FindObjectsByType<InteractableObject>(FindObjectsSortMode.None)
                .Where(x => x.transform.Cast<Transform>().Any(child => child.gameObject.activeInHierarchy))
                .ToArray();
        private bool IsInBuilding => _currentPopUI != null;
        private ActionButton CurrentPopUIButton => _currentPopUI.Buttons[_buildingActionButtonIndex];


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

            if (IsInBuilding)
            {
                HandlePlayerActions();
            }
            else
            {
                HandlePlayerMovement();
            }

            HandleZoom();

            if (_currentPopUI != null && !_popUIFirstActionButtonAcknowledged && _currentPopUI.Buttons.Any())
            {
                AcknowledgeFirstPopUIButton();
            }
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
            if (DPad.left.wasPressedThisFrame)
            {
                HandleActionButtonNavigate(-1);
            }
            else if (DPad.right.wasPressedThisFrame)
            {
                HandleActionButtonNavigate(1);
            }

            //todo: Check if WORLD or EXIST button
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

        private void AcknowledgeFirstPopUIButton()
        {
            _popUIFirstActionButtonAcknowledged = true;
            _buildingActionButtonIndex = 0;
            CurrentPopUIButton.HandleControllerHover();
        }
    }
}
