using Assets.Xbox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using UnityEngine.InputSystem.Users;
using static UnityEngine.InputSystem.InputAction;
using PlayerInput = UnityEngine.InputSystem.PlayerInput;

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
    public PlayerInput PlayerInput;

    [SerializeField]
    private RectTransform CursorTransform;

    [SerializeField]
    public static float CursorSpeed = 2000;

    [SerializeField]
    private RectTransform CanvasRectTransform;

    [SerializeField]
    private Canvas Canvas;

    [SerializeField]
    private float padding = 35;

    //todo: We don't need this shit anymore.
    private void OnEnable()
    {
        //MainCamera = PlayerInput.camera;

        //VirtualMouse = GameControlsManager.Instance.VirtualMouse;

        //InputUser.PerformPairingWithDevice(VirtualMouse, PlayerInput.user);

        //if (CursorTransform != null)
        //{
        //    Vector2 position = new Vector2(Screen.width/2, Screen.height/2);
        //    InputState.Change(VirtualMouse.position, position);
        //}

        //if (GameSettings.Instance.IsUsingController) return;
        //InputSystem.onAfterUpdate += UpdateMotion;

        //PlayerInput.actions["Click"].performed += ActionButton;
        //PlayerInput.actions["ScrollWheel"].performed += Scroll;
        //Invoke("Init", 0.1f);
    }

    private void Init()
    {
        //HasInitialized = true;
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
            TooltipMouseOver.OnHover?.Invoke();
        }
    }

    private void Update()
    {
        //if (!HasInitialized) return;

        //if (PlayerInput.currentControlScheme != PreviousControlScheme)
        //{
        //    OnControlsChanged();
        //    PreviousControlScheme = PlayerInput.currentControlScheme;
        //}
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
            //    PreviousHitMapTile?.HoverExit();
                TooltipMouseOver.OnHover?.Invoke();
                mapTile.Hover();
                PreviousHitMapTile = mapTile;
            }
            //  Debug.Log("hit: " + hit.transform.name);
        }
        else
        {
       //     TooltipMouseOver.OnHover?.Invoke();
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

    /// <summary>
    /// Handle Scroll movements. When running in Xbox mode, zoom controls are specifically handled in <see cref="GameplayControllerHandler.HandleZoom"/>
    /// </summary>
    /// <param name="ctx"></param>
    private void Scroll(CallbackContext ctx)
    {
        if (PlayerInput.currentControlScheme == MouseScheme || GameSettings.Instance.IsUsingController) return;

        if (PauseMenu.Instance == null || !PauseMenu.Instance.active)
        {
            var scrollDirection = (Vector2)ctx.control.ReadValueAsObject();
            GameControlsManager.TryZoom?.Invoke(scrollDirection.y);
        }
    }

    public void SnapToLocation(Vector2 newPosition)
    {
        var screenPosition = UI.Instance.GetComponent<Canvas>().worldCamera.WorldToScreenPoint(newPosition);
        InputState.Change(VirtualMouse.position, screenPosition);
#if UNITY_EDITOR
#else
        screenPosition.Set(Screen.width, Screen.height, screenPosition.z);
#endif
    //    Debug.Log(screenPosition);
    //    CurrentMouse.WarpCursorPosition(screenPosition);
    }
}
