using UnityEngine;
using UnityEngine.Events;
using UnityEngine.InputSystem;

public class GameControlsManager : MonoBehaviour
{
    public static GameControlsManager Instance { get; private set; }
    public static UnityAction<float> TryZoom;
    public Mouse VirtualMouse;

    private void Awake()
    {
        Instance = this;
        if (VirtualMouse == null)
        {
            VirtualMouse = (Mouse)InputSystem.AddDevice("VirtualMouse");
        }
        else if (!VirtualMouse.added)
        {
            InputSystem.AddDevice(VirtualMouse);
        }
    }

    void Update()
    {
        if (!GameManager.Instance.InGameSession) return;

        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll != 0 && (PauseMenu.Instance == null || !PauseMenu.Instance.active))
        {
            TryZoom?.Invoke(mouseScroll);
        }

    }

    private void OnDisable()
    {
        InputSystem.RemoveDevice(VirtualMouse);
    }
}
