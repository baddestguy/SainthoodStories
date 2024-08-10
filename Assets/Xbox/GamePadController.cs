using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Assets.Xbox
{
    public enum DirectionInput
    {
        Void = -1,
        Up = 0,
        Right,
        Down,
        Left
    }

    public enum GamePadButton
    {
        Void = -1,
        North = 0,
        East,
        South,
        West,
        RightShoulder,
        LeftShoulder,
        Start
    }

    public static class GamePadController
    {
        private static StickControl LeftStick => Gamepad.current.leftStick;
        private static DpadControl DPad => Gamepad.current.dpad;

        /// <summary>
        /// Get the current dpad direction, if any, that is pressed this frame
        /// </summary>
        /// <returns>The direction being pressed and the button control</returns>
        public static (DirectionInput Input, ButtonControl Control) GetDirection()
        {
            if (Gamepad.current == null) return (DirectionInput.Void, null);

            if (DPad.up.isPressed || DPad.up.wasReleasedThisFrame) return (DirectionInput.Up, DPad.up);
            if (DPad.down.isPressed || DPad.down.wasReleasedThisFrame) return (DirectionInput.Down, DPad.down);
            if (DPad.left.isPressed || DPad.left.wasReleasedThisFrame) return (DirectionInput.Left, DPad.left);
            if (DPad.right.isPressed || DPad.right.wasReleasedThisFrame) return (DirectionInput.Right, DPad.right);

            if (LeftStick.up.isPressed || LeftStick.up.wasReleasedThisFrame) return (DirectionInput.Up, LeftStick.up);
            if (LeftStick.down.isPressed || LeftStick.down.wasReleasedThisFrame) return (DirectionInput.Down, LeftStick.down);
            if (LeftStick.left.isPressed || LeftStick.left.wasReleasedThisFrame) return (DirectionInput.Left, LeftStick.left);
            if (LeftStick.right.isPressed || LeftStick.right.wasReleasedThisFrame) return (DirectionInput.Right, LeftStick.right);

            return (DirectionInput.Void, DPad.up);
        }

        /// <summary>
        /// Get the current controller button being pressed, if any, that is pressed this frame
        /// </summary>
        /// <returns>The direction being pressed and the button control</returns>
        public static (GamePadButton Button, ButtonControl Control) GetButton()
        {
            if(Gamepad.current == null) return (GamePadButton.Void, null);

            if (Gamepad.current.buttonNorth.isPressed || Gamepad.current.buttonNorth.wasReleasedThisFrame) return (GamePadButton.North, Gamepad.current.buttonNorth);
            if (Gamepad.current.buttonEast.isPressed || Gamepad.current.buttonEast.wasReleasedThisFrame) return (GamePadButton.East, Gamepad.current.buttonEast);
            if (Gamepad.current.buttonSouth.isPressed || Gamepad.current.buttonSouth.wasReleasedThisFrame) return (GamePadButton.South, Gamepad.current.buttonSouth);
            if (Gamepad.current.buttonWest.isPressed || Gamepad.current.buttonWest.wasReleasedThisFrame) return (GamePadButton.West, Gamepad.current.buttonWest);
            if (Gamepad.current.rightShoulder.isPressed || Gamepad.current.rightShoulder.wasReleasedThisFrame) return (GamePadButton.RightShoulder, Gamepad.current.rightShoulder);
            if (Gamepad.current.rightTrigger.isPressed || Gamepad.current.rightTrigger.wasReleasedThisFrame) return (GamePadButton.RightShoulder, Gamepad.current.rightTrigger);
            if (Gamepad.current.leftShoulder.isPressed || Gamepad.current.leftShoulder.wasReleasedThisFrame) return (GamePadButton.LeftShoulder, Gamepad.current.leftShoulder);
            if (Gamepad.current.leftTrigger.isPressed || Gamepad.current.leftTrigger.wasReleasedThisFrame) return (GamePadButton.LeftShoulder, Gamepad.current.leftTrigger);
            if (Gamepad.current.startButton.isPressed || Gamepad.current.startButton.wasReleasedThisFrame) return (GamePadButton.Start, Gamepad.current.startButton);

            return (GamePadButton.Void, Gamepad.current.selectButton);
        }

        /// <summary>
        /// Get the closest game object in a direction on the canvas.
        /// <br />
        /// 2D objects have proven to work better with anchored position.
        /// </summary>
        /// <param name="direction">The direction to check in</param>
        /// <param name="currentGameObject">The reference game object</param>
        /// <param name="gameObjects">The list of game objects to evaluate</param>
        /// <param name="useAnchoredPosition">Optional. Set to true if using well anchored 2d objects.</param>
        /// <returns></returns>
        public static GameObject GetClosestGameObjectOnCanvasInDirection(this DirectionInput direction,
            GameObject currentGameObject,
            GameObject[] gameObjects,
            bool useAnchoredPosition = true)
        {
            const float axisWeight = 0.3f;

            if(direction == DirectionInput.Void) return null;

            var closestObjectDistance = double.MaxValue;
            GameObject closestGameObject = null;

            var currentPosition = new Vector3(0, 0);
            if(currentGameObject != null)
            {
                var currentGameObjectRectTransform = currentGameObject.GetComponent<RectTransform>();
                currentPosition = useAnchoredPosition ? currentGameObjectRectTransform.anchoredPosition3D : currentGameObjectRectTransform.position;
            }

            foreach (var gameObjectToEvaluate in gameObjects.Where(x => x.activeInHierarchy))
            {
                if (gameObjectToEvaluate == currentGameObject) continue;

                var evaluateRectTransform = gameObjectToEvaluate.GetComponent<RectTransform>();
                var evaluatePosition = useAnchoredPosition ? evaluateRectTransform.anchoredPosition3D : evaluateRectTransform.position;

                var vectorToTarget = evaluatePosition - currentPosition;

                var isDesiredDirection = direction switch
                {
                    DirectionInput.Up => vectorToTarget.y > 0,
                    DirectionInput.Down => vectorToTarget.y < 0,
                    DirectionInput.Left => vectorToTarget.x < 0,
                    DirectionInput.Right => vectorToTarget.x > 0,
                    _ => false
                };

                if (!isDesiredDirection) continue;

                var weight = direction switch
                {
                    DirectionInput.Up or DirectionInput.Down => new Vector2(1, axisWeight),
                    DirectionInput.Left or DirectionInput.Right => new Vector2(axisWeight, 1),
                    _ => new Vector2(1, 1)
                };

                // Apply direction weight to vectorToTarget and square it to calculate distance of vector.
                //z axis is ignored because 2d canvas but more importantly, package items are deep in the z axis and throwing this algorithm off
                var weightedVector = new Vector2(vectorToTarget.x * weight.x, vectorToTarget.y * weight.y);
                var distance = weightedVector.sqrMagnitude; //square magnitude is less cpu intensive since magnitude has to take the square root

                if (!(distance < closestObjectDistance)) continue;

                closestObjectDistance = distance;
                closestGameObject = gameObjectToEvaluate;
            }

            return closestGameObject;
        }
    }


}
