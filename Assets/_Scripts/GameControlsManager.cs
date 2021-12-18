using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

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
        if (!GameManager.Instance.InGameSession) return;

        float mouseScroll = Input.GetAxis("Mouse ScrollWheel");
        if (mouseScroll != 0 && (PauseMenu.Instance == null || !PauseMenu.Instance.active))
        {
            TryZoom?.Invoke(mouseScroll);
        }
    }
}
