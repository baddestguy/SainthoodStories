using UnityEngine;
using UnityEngine.InputSystem.Controls;

namespace Assets.Xbox
{
    public enum DPadDirection
    {
        Up = 0,
        Right,
        Down,
        Left
    }

    public static class DPadExtensions
    {
        /// <summary>
        /// Get the current direction, if any, that was pressed on the dpad this frame
        /// </summary>
        /// <param name="dPad">The current dpad</param>
        /// <param name="initialPressOnly">If true, will only check if the dpad was pressed this frame. <br />
        ///     If false, will additionally check if the dpad is being pressed in this frame.</param>
        /// <returns>The direction being pressed if any</returns>
        public static DPadDirection? GetDirection(this DpadControl dPad, bool initialPressOnly = true)
        {
            if (dPad.up.wasPressedThisFrame || (dPad.up.isPressed && !initialPressOnly)) return DPadDirection.Up;
            if (dPad.down.wasPressedThisFrame || (dPad.down.isPressed && !initialPressOnly)) return DPadDirection.Down;
            if (dPad.left.wasPressedThisFrame || (dPad.left.isPressed && !initialPressOnly)) return DPadDirection.Left;
            if (dPad.right.wasPressedThisFrame || (dPad.right.isPressed && !initialPressOnly)) return DPadDirection.Right;

            return null;
        }

        /// <summary>
        /// Check if Up or Down was pressed on the dpad.
        /// </summary>
        /// <param name="dPad">The current dpad</param>
        /// <param name="initialPressOnly">If true, will only check if the dpad was pressed this frame. <br />
        ///     If false, will additionally check if the dpad is being pressed in this frame.</param>
        /// <returns>True if Up/Down is being pressed</returns>
        public static bool IsVerticalPress(this DpadControl dPad, bool initialPressOnly = true)
        {
            return dPad.GetDirection(initialPressOnly) == DPadDirection.Up || dPad.GetDirection(initialPressOnly) == DPadDirection.Down;
        }

        /// <summary>
        /// Check if Left or Right was pressed on the dpad.
        /// </summary>
        /// <param name="dPad">The current dpad</param>
        /// <param name="initialPressOnly">If true, will only check if the dpad was pressed this frame. <br />
        ///     If false, will additionally check if the dpad is being pressed in this frame.</param>
        /// <returns>True if Left/Right  is being pressed</returns>
        public static bool IsHorizontalPress(this DpadControl dPad, bool initialPressOnly = true)
        {
            return dPad.GetDirection(initialPressOnly) == DPadDirection.Left || dPad.GetDirection(initialPressOnly) == DPadDirection.Right;
        }

        /// <summary>
        /// Given a direction, get the closest game object based on local position of the objects. Note that the Z axis is ignored when determining distance.
        /// </summary>
        /// <param name="direction">The direction to check</param>
        /// <param name="currentGameObject">Our starting point. Can be null</param>
        /// <param name="gameObjects">The list of game objects to evaluate</param>
        /// <returns>The closest game object in that direction if it exists</returns>
        public static GameObject GetClosestGameObjectOnCanvasInDirection(this DPadDirection direction, GameObject currentGameObject, GameObject[] gameObjects)
        {
            const float axisWeight = 0.05f;

            var closestObjectDistance = double.MaxValue;
            GameObject closestGameObject = null;

            foreach (var gameObjectToEvaluate in gameObjects)
            {
                if (gameObjectToEvaluate == currentGameObject) continue;

                var currentPosition = currentGameObject == null ? new Vector2(0, 0) : currentGameObject.GetComponent<RectTransform>().anchoredPosition;
                var vectorToTarget = gameObjectToEvaluate.GetComponent<RectTransform>().anchoredPosition - currentPosition;

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
                    DPadDirection.Up or DPadDirection.Down => new Vector2(1, axisWeight),
                    DPadDirection.Left or DPadDirection.Right => new Vector2(axisWeight, 1),
                    _ => new Vector2(1, 1)
                };

                // Apply direction weight to vectorToTarget and square it to calculate distance of vector.
                //z axis is ignored because 2d canvas but more importantly, package items are deep in the z axis and throwing this algorithm off
                var weightedVector = new Vector2(vectorToTarget.x * weight.x, vectorToTarget.y * weight.y); 
                var distance = weightedVector.sqrMagnitude //square magnitude is less cpu intensive since magnitude has to take the square root
                    ;
                if (!(distance < closestObjectDistance)) continue;

                closestObjectDistance = distance;
                closestGameObject = gameObjectToEvaluate;
            }

            return closestGameObject;
        }
    }
}
