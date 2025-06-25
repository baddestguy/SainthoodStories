using UnityEngine;

public class ActsCameraControl : MonoBehaviour
{
    public float ZoomDuration = 1200f; // total time to reach max zoom
    public float MinSize = 5f;
    public float MaxSize = 25f;

    private float elapsedTime = 0f;
    private Camera cam;

    void Start()
    {
        cam = GetComponent<Camera>();
    }

    void Update()
    {
        elapsedTime += ActsTimeKeeper.Instance.DeltaTime;
        float t = Mathf.Clamp01(elapsedTime / ZoomDuration);
        cam.orthographicSize = Mathf.Lerp(MinSize, MaxSize, t);
    }
}
