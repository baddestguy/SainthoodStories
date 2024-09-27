using System.Linq;
using JetBrains.Annotations;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.Controls;

namespace Assets._Scripts.Xbox
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

    /// <summary>
    /// Custom button control to override the wasPressedThisFrame property
    /// </summary>
    public class CustomButtonControl
    {
        public bool IsPressed { get; }
        public bool WasPressedThisFrame { get; }
        public bool WasReleasedThisFrame { get; }

        public CustomButtonControl([CanBeNull] ButtonControl control = null)
        {
            IsPressed = control?.isPressed ?? false;
            WasPressedThisFrame = control?.wasPressedThisFrame ?? false;
            WasReleasedThisFrame = control?.wasReleasedThisFrame ?? false;
        }

        public CustomButtonControl(bool isPressed, bool wasPressedThisFrame, bool wasReleasedThisFrame)
        {
            IsPressed = isPressed;
            WasPressedThisFrame = wasPressedThisFrame;
            WasReleasedThisFrame = wasReleasedThisFrame;
        }
    }

    public static class GamePadController
    {
        private static StickControl LeftStick => Gamepad.current.leftStick;
        private static DpadControl DPad => Gamepad.current.dpad;

        /// <summary>
        /// Get the current dpad direction, if any, that is pressed this frame
        /// </summary>
        /// <returns>The direction being pressed and the button control</returns>
        public static (DirectionInput Input, CustomButtonControl Control) GetDirection()
        {
            if (GameSettings.Instance.IsXboxMode)
            {
                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonDPadUp) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonDPadUp) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonDPadUp))
                {
                    return (DirectionInput.Up,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonDPadUp),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonDPadUp),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonDPadUp)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonDPadDown) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonDPadDown) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonDPadDown))
                {
                    return (DirectionInput.Down,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonDPadDown),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonDPadDown),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonDPadDown)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonDPadLeft) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonDPadLeft) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonDPadLeft))
                {
                    return (DirectionInput.Left,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonDPadLeft),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonDPadLeft),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonDPadLeft)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonDPadRight) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonDPadRight) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonDPadRight))
                {
                    return (DirectionInput.Right,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonDPadRight),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonDPadRight),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonDPadRight)));
                }

                return (DirectionInput.Void, new CustomButtonControl());
            }

            if (Gamepad.current == null)
            {
                GameplayControllerHandler.Instance.AlertNoControllerDetected();
                return (DirectionInput.Void, new CustomButtonControl());
            }

            if (DPad.up.isPressed || DPad.up.wasReleasedThisFrame) return (DirectionInput.Up, new(DPad.up));
            if (DPad.down.isPressed || DPad.down.wasReleasedThisFrame) return (DirectionInput.Down, new(DPad.down));
            if (DPad.left.isPressed || DPad.left.wasReleasedThisFrame) return (DirectionInput.Left, new(DPad.left));
            if (DPad.right.isPressed || DPad.right.wasReleasedThisFrame) return (DirectionInput.Right, new(DPad.right));

            if (LeftStick.up.isPressed || LeftStick.up.wasReleasedThisFrame) return (DirectionInput.Up, new(LeftStick.up));
            if (LeftStick.down.isPressed || LeftStick.down.wasReleasedThisFrame) return (DirectionInput.Down, new(LeftStick.down));
            if (LeftStick.left.isPressed || LeftStick.left.wasReleasedThisFrame) return (DirectionInput.Left, new(LeftStick.left));
            if (LeftStick.right.isPressed || LeftStick.right.wasReleasedThisFrame) return (DirectionInput.Right, new(LeftStick.right));

            return (DirectionInput.Void, new CustomButtonControl());
        }

        /// <summary>
        /// Get the current controller button being pressed, if any, that is pressed this frame
        /// </summary>
        /// <returns>The direction being pressed and the button control</returns>
        public static (GamePadButton Button, CustomButtonControl Control) GetButton()
        {
            if (GameSettings.Instance.IsXboxMode)
            {
                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonY) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonY) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonY))
                {
                    return (GamePadButton.North,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonY),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonY),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonY)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonB) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonB) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonB))
                {
                    return (GamePadButton.East,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonB),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonB),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonB)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonA) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonA) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonA))
                {
                    return (GamePadButton.South,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonA),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonA),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonA)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonX) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonX) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonX))
                {
                    return (GamePadButton.West,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonX),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonX),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonX)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonRightShoulder) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonRightShoulder) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonRightShoulder))
                {
                    return (GamePadButton.RightShoulder,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonRightShoulder),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonRightShoulder),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonRightShoulder)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonLeftShoulder) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonLeftShoulder) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonLeftShoulder))
                {
                    return (GamePadButton.LeftShoulder,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonLeftShoulder),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonLeftShoulder),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonLeftShoulder)));
                }

                if (GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonMenu) ||
                    GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonMenu) ||
                    GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonMenu))
                {
                    return (GamePadButton.Start,
                        new(GXDKInput.GetKey(GXDKKeyCode.Gamepad1ButtonMenu),
                            GXDKInput.GetKeyDown(GXDKKeyCode.Gamepad1ButtonMenu),
                            GXDKInput.GetKeyUp(GXDKKeyCode.Gamepad1ButtonMenu)));
                }

                return (GamePadButton.Void, new CustomButtonControl());
            }

            if (Gamepad.current == null)
            {
                GameplayControllerHandler.Instance.AlertNoControllerDetected();
                return (GamePadButton.Void, new CustomButtonControl());
            }

            if (Gamepad.current.buttonNorth.isPressed || Gamepad.current.buttonNorth.wasReleasedThisFrame)
            {
                return (GamePadButton.North, new(Gamepad.current.buttonNorth));
            }

            if (Gamepad.current.buttonEast.isPressed || Gamepad.current.buttonEast.wasReleasedThisFrame)
            {
                return (GamePadButton.East, new(Gamepad.current.buttonEast));
            }

            if (Gamepad.current.buttonSouth.isPressed || Gamepad.current.buttonSouth.wasReleasedThisFrame)
            {
                return (GamePadButton.South, new(Gamepad.current.buttonSouth));
            }

            if (Gamepad.current.buttonWest.isPressed || Gamepad.current.buttonWest.wasReleasedThisFrame)
            {
                return (GamePadButton.West, new(Gamepad.current.buttonWest));
            }

            if (Gamepad.current.rightShoulder.isPressed || Gamepad.current.rightShoulder.wasReleasedThisFrame)
            {
                return (GamePadButton.RightShoulder, new(Gamepad.current.rightShoulder));
            }

            if (Gamepad.current.rightTrigger.isPressed || Gamepad.current.rightTrigger.wasReleasedThisFrame)
            {
                return (GamePadButton.RightShoulder, new(Gamepad.current.rightTrigger));
            }

            if (Gamepad.current.leftShoulder.isPressed || Gamepad.current.leftShoulder.wasReleasedThisFrame)
            {
                return (GamePadButton.LeftShoulder, new(Gamepad.current.leftShoulder));
            }

            if (Gamepad.current.leftTrigger.isPressed || Gamepad.current.leftTrigger.wasReleasedThisFrame)
            {
                return (GamePadButton.LeftShoulder, new(Gamepad.current.leftTrigger));
            }

            if (Gamepad.current.startButton.isPressed || Gamepad.current.startButton.wasReleasedThisFrame)
            {
                return (GamePadButton.Start, new(Gamepad.current.startButton));
            }

            return (GamePadButton.Void, new CustomButtonControl());
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

            if (direction == DirectionInput.Void) return null;

            var closestObjectDistance = double.MaxValue;
            GameObject closestGameObject = null;

            var currentPosition = new Vector3(0, 0);
            if (currentGameObject != null)
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
