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
        public static DPadDirection? GetDirection(this DpadControl dPad)
        {
            if (dPad.up.wasPressedThisFrame) return DPadDirection.Up;
            if (dPad.down.wasPressedThisFrame) return DPadDirection.Down;
            if (dPad.left.wasPressedThisFrame) return DPadDirection.Left;
            if (dPad.right.wasPressedThisFrame) return DPadDirection.Right;

            return null;
        }

        public static bool IsVerticalPress(this DpadControl dPad)
        {
            return dPad.GetDirection() == DPadDirection.Up || dPad.GetDirection() == DPadDirection.Down;
        }

        public static bool IsHorizontalPress(this DpadControl dPad)
        {
            return dPad.GetDirection() == DPadDirection.Left || dPad.GetDirection() == DPadDirection.Right;
        }
    }
}
