using UnityEngine;
using UnityEngine.Events;

public class GameControlsManager : MonoBehaviour
{
    public static GameControlsManager Instance { get; private set; }
    public static UnityAction<float> TryZoom;

    private void Awake()
    {
        Instance = this;
    }

    void Update()
    {
        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll != 0)
        {
            TryZoom?.Invoke(mouseScroll);
        }
    }
}
