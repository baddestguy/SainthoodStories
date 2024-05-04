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
        public static DPadDirection? GetDirection(this DpadControl dPad, bool initialPressOnly = true)
        {
            if (dPad.up.wasPressedThisFrame || (dPad.up.isPressed && !initialPressOnly)) return DPadDirection.Up;
            if (dPad.down.wasPressedThisFrame || (dPad.down.isPressed && !initialPressOnly)) return DPadDirection.Down;
            if (dPad.left.wasPressedThisFrame || (dPad.left.isPressed && !initialPressOnly)) return DPadDirection.Left;
            if (dPad.right.wasPressedThisFrame || (dPad.right.isPressed && !initialPressOnly)) return DPadDirection.Right;

            return null;
        }

        public static bool IsVerticalPress(this DpadControl dPad, bool initialPressOnly = true)
        {
            return dPad.GetDirection(initialPressOnly) == DPadDirection.Up || dPad.GetDirection(initialPressOnly) == DPadDirection.Down;
        }

        public static bool IsHorizontalPress(this DpadControl dPad, bool initialPressOnly = true)
        {
            return dPad.GetDirection(initialPressOnly) == DPadDirection.Left || dPad.GetDirection(initialPressOnly) == DPadDirection.Right;
        }
    }
}
