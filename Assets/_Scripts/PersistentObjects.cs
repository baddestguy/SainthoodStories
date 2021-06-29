using UnityEngine;

public class PersistentObjects : MonoBehaviour
{

    public static PersistentObjects instance;
    public bool developerMode;

    private void Awake()
    {
        if(instance != null && instance != this)
        {
            Destroy(gameObject);
        }
        instance = this;
        Application.targetFrameRate = 60;
        DontDestroyOnLoad(gameObject);
    }
}
