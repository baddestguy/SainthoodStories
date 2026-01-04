using System.Collections;
using UnityEngine;

public class CameraControls : MonoBehaviour
{
    public static bool CameraMove;
    public static bool CameraZoom;
    public float Speed;
    public float MinZoom;
    public float MaxZoom;
    public Vector2 BoundaryX; //-1.842995, 3.956034
    public Vector2 BoundaryY; //17.9911, 16.3156

    private Vector3 OriginalCamTarget;
    private Vector3 CamTarget;
    private float ZoomTarget;
    //private DepthOfField DepthOfField;
    //private Bloom Bloom;

    public Camera UICam3D;
    public Camera MyCamera;
    //public PostProcessVolume PostProcessor;

    [Header("Mouse Zoom Control (Not Impleamented)")]
    public bool canMouseZoom;
    public float mouseScrollSpeed = 2;
    public float minCamaraHeight;
    public float maxCamaraHeight;
    public float zoomSpeed;

    public static bool ZoomComplete;
    [SerializeField] private float doneThreshold = 0.02f;

    void OnEnable()
    {
        OriginalCamTarget = transform.position;
        CamTarget = OriginalCamTarget;
        ZoomTarget = Constants.INTERIOR_ZOOM_IN_TARGET;
        StartCoroutine(IntroZoomThenDisable());
    }

    IEnumerator IntroZoomThenDisable()
    {
        while (Mathf.Abs(MyCamera.orthographicSize - ZoomTarget) > doneThreshold)
        {
            float next = Mathf.Lerp(MyCamera.orthographicSize, ZoomTarget, Time.deltaTime * 1.5f);
            MyCamera.orthographicSize = next;

            yield return null;
        }

        MyCamera.orthographicSize = ZoomTarget;

        enabled = false;
    }

    public void SetCameraTarget(Vector3 newTarget, bool modifyPostProcess = true)
    {
        CamTarget = newTarget.magnitude != 0 ? newTarget : OriginalCamTarget;

        if(newTarget.magnitude == 0)
        {
            SetZoomTarget(9f);
            if (modifyPostProcess)
            {
                //DepthOfField.active = false;
                //Bloom.active = true;
            }
        }
        else
        {
            SetZoomTarget(Constants.EXTERIOR_ZOOM_IN_TARGET);
            if (modifyPostProcess)
            {
                //DepthOfField.active = true;
                //Bloom.active = true;
            }
        }
    }

    private void Zoom(float increment)
    {
        MyCamera.orthographicSize = Mathf.Lerp(MyCamera.orthographicSize, ZoomTarget, Time.deltaTime*3);
        UICam3D.orthographicSize = Mathf.Lerp(UICam3D.orthographicSize, ZoomTarget, Time.deltaTime * 3);

        if(Mathf.Abs(MyCamera.orthographicSize - ZoomTarget) <= 0.3f)
        {
            ZoomComplete = true;
        }
    }


    public void SetZoomTarget(float target)
    {
    //    ZoomTarget = target;
        ZoomComplete = false;
    }

    public void EnableDepthOfField(bool enable)
    {
        //DepthOfField.active = enable;
    }

    /// <summary>
    /// Note : This zoom controls the camera position
    /// </summary>
    /// <param name="oldDistance"></param>
    /// <param name="newDistance"></param>
    private void OnMouseZoom(float oldDistance, float newDistance)
    {
        float camHeightValue = transform.localPosition.y;
        camHeightValue = Mathf.Clamp(camHeightValue * (oldDistance / newDistance), minCamaraHeight, maxCamaraHeight);
        Vector3 newHeight = new Vector3(0, 1, -1) * camHeightValue;
        transform.localPosition = Vector3.Lerp(transform.localPosition, newHeight, Time.deltaTime * zoomSpeed);
    }
}
