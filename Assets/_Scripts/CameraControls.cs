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

    void Start()
    {
        
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
    }

    private void Zoom(float increment)
    {
        Camera.main.orthographicSize = Mathf.Clamp(Camera.main.orthographicSize - increment, MinZoom, MaxZoom);
    }
}
