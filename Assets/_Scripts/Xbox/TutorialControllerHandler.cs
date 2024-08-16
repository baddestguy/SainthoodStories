using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;

namespace Assets._Scripts.Xbox
{
    public class TutorialControllerHandler : MonoBehaviour
    {

        private bool _firstActionButtonAcknowledged;
        private int _buttonIndex;
        private bool _skipFrame;

        // Start is called before the first frame update
        void Start()
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged += HandleInputMethodChanged;
        }

        public void OnDisable()
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged -= HandleInputMethodChanged;
        }

        private void HandleInputMethodChanged(bool isUsingController)
        {
            var toolTipMouseOvers = GetComponentsInChildren<TooltipMouseOver>(false)
                .Where(x => x.HasControllerHover)
                .ToArray();

            if (_firstActionButtonAcknowledged)
            {
                foreach (var tooltipMouseOver in toolTipMouseOvers)
                {
                    tooltipMouseOver.HandleControllerExit();
                }
            }

            if (isUsingController)
            {
                _skipFrame = true;
            }

            _firstActionButtonAcknowledged = false;
        }


        // Update is called once per frame
        void Update()
        {
            if (Gamepad.current == null || !GameSettings.Instance.IsUsingController) return;

            if (_skipFrame)
            {
                _skipFrame = false;
                return;
            }

            //ordering by descending name because Yes comes before No
            var toolTipMouseOvers = GetComponentsInChildren<TooltipMouseOver>(false)
                .OrderByDescending(x => x.name)
                .ToArray();

            if (!toolTipMouseOvers.Any()) return;

            if (!_firstActionButtonAcknowledged)
            {
                _buttonIndex = 0;
                toolTipMouseOvers[_buttonIndex].HandleControllerHover();
                _firstActionButtonAcknowledged = true;
            }
            else
            {
                var pressedDirection = GamePadController.GetDirection();
                var pressedButton = GamePadController.GetButton();

                if (pressedDirection.Control.wasPressedThisFrame)
                {
                    if (pressedDirection.Input == DirectionInput.Left)
                    {
                        HandleEventPopupButtonNavigate(-1);
                    }
                    else if (pressedDirection.Input == DirectionInput.Right)
                    {
                        HandleEventPopupButtonNavigate(1);
                    }
                }
                else if (pressedButton.Button == GamePadButton.South && pressedButton.Control.wasPressedThisFrame)
                {
                    if(_buttonIndex == 0) UI.Instance.TutorialConfirmation("YES");
                    if(_buttonIndex == 1) UI.Instance.TutorialConfirmation("NO");
                }
                else if (pressedButton.Button == GamePadButton.East && pressedButton.Control.wasPressedThisFrame)
                {
                    UI.Instance.TutorialConfirmation("CLOSE");
                }

                void HandleEventPopupButtonNavigate(int increment)
                {
                    toolTipMouseOvers[_buttonIndex].HandleControllerExit();
                    _buttonIndex = (_buttonIndex + increment + toolTipMouseOvers.Length) % toolTipMouseOvers.Length;
                    toolTipMouseOvers[_buttonIndex].HandleControllerHover();
                }
            }
        }
    }
}
