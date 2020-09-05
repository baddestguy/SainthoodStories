using UnityEngine;

public class PersistentObjects : MonoBehaviour
{
    private void Awake()
    {
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
    }
}
