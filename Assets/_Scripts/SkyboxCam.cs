using UnityEngine;

public class SkyboxCam : MonoBehaviour
{
    void Update()
    {
        transform.eulerAngles += new Vector3(0, 0.01f, 0);
    }
}
