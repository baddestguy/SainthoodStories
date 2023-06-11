using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using static UnityEngine.InputSystem.InputAction;

public class GamepadCursor : MonoBehaviour
{
    private Mouse VirtualMouse;
    private bool PreviousMouseState;
    private Camera MainCamera;
    private const string GamepadScheme = "Gamepad";
    private const string MouseScheme = "Keyboard&Mouse";
    private string PreviousControlScheme = "";
    private Mouse CurrentMouse = Mouse.current;
    private MapTile PreviousHitMapTile;
    private bool HasInitialized;
   
    [SerializeField]
    private PlayerInput PlayerInput;

    [SerializeField]
    private RectTransform CursorTransform;

    [SerializeField]
    private float CursorSpeed = 1000;

    [SerializeField]
    private RectTransform CanvasRectTransform;

    [SerializeField]
    private Canvas Canvas;

    [SerializeField]
    private float padding = 35;

    private void OnEnable()
    {
        MainCamera = PlayerInput.camera;

        VirtualMouse = GameControlsManager.Instance.VirtualMouse;

        InputUser.PerformPairingWithDevice(VirtualMouse, PlayerInput.user);

        if (CursorTransform != null)
        {
            Vector2 position = CursorTransform.anchoredPosition;
            InputState.Change(VirtualMouse.position, position);
        }

        InputSystem.onAfterUpdate += UpdateMotion;
        PlayerInput.actions["Click"].performed += ActionButton;
        PlayerInput.actions["ScrollWheel"].performed += Scroll;
        Invoke("Init", 0.1f);
    }

    private void Init()
    {
        HasInitialized = true;
    }

    private void OnDisable()
    {
        InputSystem.onAfterUpdate -= UpdateMotion;
        PlayerInput.actions["Click"].performed -= ActionButton;
        PlayerInput.actions["ScrollWheel"].performed -= Scroll;
        PlayerInput.user.UnpairDevicesAndRemoveUser();
    }

    private void OnControlsChanged()
    {
        if (!HasInitialized) return;

        MainCamera = PlayerInput.camera;
        if (CurrentMouse == null) CurrentMouse = Mouse.current;

        if (PlayerInput.currentControlScheme == MouseScheme && PreviousControlScheme != MouseScheme)
        {
            CursorTransform.gameObject.SetActive(false);
            Cursor.visible = true;
            CurrentMouse.WarpCursorPosition(VirtualMouse.position.ReadValue());
            PreviousControlScheme = MouseScheme;
        }
        else if(PlayerInput.currentControlScheme == GamepadScheme && PreviousControlScheme != GamepadScheme)
        {
            CursorTransform.gameObject.SetActive(true);
            Cursor.visible = false;
            InputState.Change(VirtualMouse.position, CurrentMouse.position.ReadValue());
            AnchorCursor(CurrentMouse.position.ReadValue());
            PreviousControlScheme = GamepadScheme;
        }
    }

    private void Update()
    {
        if (!HasInitialized) return;

        if (PlayerInput.currentControlScheme != PreviousControlScheme)
        {
            OnControlsChanged();
            PreviousControlScheme = PlayerInput.currentControlScheme;
        }
    }

    private void UpdateMotion()
    {
        if ( VirtualMouse == null || Gamepad.current == null || !VirtualMouse.added)
        {
            return;
        }

        Vector2 deltaValue = Gamepad.current.leftStick.ReadValue();
        deltaValue *= CursorSpeed * Time.deltaTime;

        Vector2 currentPosition = VirtualMouse.position.ReadValue();
        Vector2 newPosition = currentPosition + deltaValue;

        newPosition.x = Mathf.Clamp(newPosition.x, padding, Screen.width - padding);
        newPosition.y = Mathf.Clamp(newPosition.y, padding, Screen.height - padding);

        InputState.Change(VirtualMouse.position, newPosition);
        InputState.Change(VirtualMouse.delta, deltaValue);

        bool aButtonIsPressed = Gamepad.current.aButton.isPressed;
        if (PreviousMouseState != aButtonIsPressed)
        {
            VirtualMouse.CopyState<MouseState>(out var mouseState);
            mouseState.WithButton(MouseButton.Left, aButtonIsPressed);
            InputState.Change(VirtualMouse, mouseState);
            PreviousMouseState = aButtonIsPressed;
        }
        AnchorCursor(newPosition);
    }

    private void AnchorCursor(Vector2 position)
    {
        if (MainCamera == null) return;
        Vector2 anchoredPosition;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(CanvasRectTransform, position, Canvas.renderMode == RenderMode.ScreenSpaceOverlay ? null : MainCamera, out anchoredPosition);
        CursorTransform.anchoredPosition = anchoredPosition;

        if (InteractableHouse.InsideHouse) return;

        Ray ray;
        if(PlayerInput.currentControlScheme == GamepadScheme)
        {
            ray = MainCamera.ScreenPointToRay(CursorTransform.position);
        }
        else
        {
            ray = MainCamera.ScreenPointToRay(CurrentMouse.position.ReadValue());
        }

        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            var mapTile = hit.transform.GetComponent<MapTile>();
            if(mapTile != null && mapTile != PreviousHitMapTile)
            {
                PreviousHitMapTile?.HoverExit();
                mapTile.Hover();
                PreviousHitMapTile = mapTile;
            }
         //  Debug.Log("hit: " + hit.transform.name);
        }

    }

    private void ActionButton(CallbackContext ctx)
    {
        if (InteractableHouse.InsideHouse) return;
        if (PlayerInput.currentControlScheme == MouseScheme) return;

        Ray ray = MainCamera.ScreenPointToRay(CursorTransform.position);
        RaycastHit hit;
        if (Physics.Raycast(ray, out hit, Mathf.Infinity))
        {
            var mapTile = hit.transform.GetComponent<MapTile>();
            if (mapTile != null)
            {
                mapTile.Click();
            }
        //    Debug.Log("CLICKED: " + hit.transform.name);
        }
    }

    private void Scroll(CallbackContext ctx)
    {
        if (PlayerInput.currentControlScheme == MouseScheme) return;

        if ((PauseMenu.Instance == null || !PauseMenu.Instance.active))
        {
            var scrollDirection = (Vector2)ctx.control.ReadValueAsObject();
            GameControlsManager.TryZoom?.Invoke(scrollDirection.y);
        }
    }
}
