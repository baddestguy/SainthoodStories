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
    public Transform target; // The target object to rotate around
    public float rotationSpeed = 1f; // The speed of rotation
    public Vector3 offset; // Offset from the target object

    void Update()
    {
        // Calculate the desired position based on the target object and offset
        //Vector3 desiredPosition = target.position + offset;

        //// Rotate the camera around the target object
        //transform.RotateAround(target.position, Vector3.up, rotationSpeed * Time.deltaTime);

        //// Update the position of the camera
        //transform.position = desiredPosition;

        //// Make the camera look at the target object
        //transform.LookAt(target.position);
    }

}
