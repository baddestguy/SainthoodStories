using UnityEngine;

public class ExteriorCamera : MonoBehaviour
{
    public static ExteriorCamera Instance { get; private set; }
    public Camera Camera;
    public Camera UICamera;

    private void Awake()
    {
        Instance = this;
    }
}
