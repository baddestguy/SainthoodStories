using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets.Xbox
{
    public class GameplayControllerHandler : MonoBehaviour
    {
        public static GameplayControllerHandler Instance { get; private set; }

        private GameManager GameManager => GameManager.Instance;
        private Player Player => GameManager.Player;

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

            HandlePlayerMovement();

            HandlePlayerActions();
        }

        private void HandlePlayerMovement()
        {
            var dPad = Gamepad.current.dpad;
            // Direction is rotated clockwise once because play facing direction is defined with Top left as up and bottom right as down
            // I guess we could support the option to have players choose.
            if (dPad.up.wasPressedThisFrame && Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.RIGHT, out var rightMapTile))
            {
                ProcessTileAction(rightMapTile);
            }
            else if (dPad.right.wasPressedThisFrame && Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.DOWN, out var downMapTile))
            {
                ProcessTileAction(downMapTile);
            }
            else if (dPad.down.wasPressedThisFrame && Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.LEFT, out var leftMapTile))
            {
                ProcessTileAction(leftMapTile);
            }
            else if (dPad.left.wasPressedThisFrame && Player.AdjacentTiles.TryGetValue(PlayerFacingDirection.UP, out var upMapTile))
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

        }
    }
}
