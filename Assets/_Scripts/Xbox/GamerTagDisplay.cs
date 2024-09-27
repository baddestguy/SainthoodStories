using Assets._Scripts.Xbox;
using TMPro;
using UnityEngine;

public class GamerTagDisplay : MonoBehaviour
{
    public TextMeshProUGUI TextMesh;

    // Start is called before the first frame update
    void Start()
    {
        if (GameSettings.Instance.IsXboxMode)
        {
            TextMesh.text = XboxUserHandler.Instance.GamerTag;
        }
        else
        {
            TextMesh.text = "";
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}
