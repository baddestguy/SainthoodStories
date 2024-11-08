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

#if !MICROSOFT_GDK_SUPPORT
        //Safety check, Just in case resolution decides to be 0x0 (yes this can sometimes happen). This resolution is just for the splash screen, as it will be overridden once player hits Main Menu
        Screen.SetResolution(Screen.mainWindowDisplayInfo.width, Screen.mainWindowDisplayInfo.height, false);
#endif

    }
}
