using Assets._Scripts.Xbox;
using UnityEngine;

public class CreditsControllerHandler : MonoBehaviour
{

    // Update is called once per frame
    void Update()
    {
        if (!GameSettings.Instance.IsUsingController) return;

        var pressedButton = GamePadController.GetButton();

        if (pressedButton.Control.WasPressedThisFrame &&
            (pressedButton.Button is GamePadButton.South or GamePadButton.East))
        {
            UI.Instance.Credits();
        }
    }
}
