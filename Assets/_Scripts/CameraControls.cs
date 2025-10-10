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

    void Start()
    {
        OriginalCamTarget = transform.position;
        CamTarget = OriginalCamTarget;
        ZoomTarget = MyCamera.orthographicSize;
        //PostProcessor.profile.TryGetSettings(out DepthOfField);
        //PostProcessor.profile.TryGetSettings(out Bloom);
        //DepthOfField.active = false;
        //Bloom.active = true;
    }

    void Update()
    {
        if (Input.touchCount > 1)
        {
            CameraZoom = true;
            Touch touch1 = Input.GetTouch(0);
            Touch touch2 = Input.GetTouch(1);

            Vector2 touch1Prev = touch1.position - touch1.deltaPosition;
            Vector2 touch2Prev = touch2.position - touch2.deltaPosition;

            float prevMagnitude = (touch1Prev - touch2Prev).magnitude;
            float currMagnitude = (touch1.position - touch2.position).magnitude;

            float diff = currMagnitude - prevMagnitude;

            Zoom(diff * 0.1f);
        }
        else if (CameraMove || Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Moved 
            && (Input.GetTouch(0).deltaPosition.magnitude) > 25f)
        {
            CameraMove = true;
            Vector3 touchDeltaPosition = Input.GetTouch(0).deltaPosition;
            Vector3 newPos = new Vector3(transform.position.x - touchDeltaPosition.x, transform.position.y - touchDeltaPosition.y, transform.position.z);
            //if (newPos.x > BoundaryX.x
            //    && newPos.x < BoundaryX.y
            //    && newPos.y > BoundaryY.x
            //    && newPos.y < BoundaryY.y)
            {
                //transform.position = Vector3.Lerp(transform.position, newPos, Time.deltaTime);
                transform.Translate(-touchDeltaPosition.x * Speed, -touchDeltaPosition.y * Speed, 0); //Move Camera
            }
        }

        if (Input.touchCount > 0 && Input.GetTouch(0).phase == TouchPhase.Ended)
        {
            CameraMove = false;
            CameraZoom = false;
        }

        Zoom(0);
        transform.position = Vector3.Lerp(transform.position, CamTarget, Time.deltaTime*3);
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
            SetZoomTarget(3f);
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
        ZoomTarget = target;
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
