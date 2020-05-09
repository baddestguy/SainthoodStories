using UnityEngine;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField]
    public Light Light;
    private Vector3 TargetRotation = new Vector3(0, 0, 0);
    private Color TargetColor = new Color();
    void Start()
    {
        GameClock.Ticked += OnTick;
        OnTick(GameManager.Instance.GameClock.Time, GameManager.Instance.GameClock.Day);
    }

    private void OnTick(double time, int day)
    {
        switch (time)
        {
            case 0: TargetRotation = new Vector3(10,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
            case 1: TargetRotation = new Vector3(17.5f,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
            case 2: TargetRotation = new Vector3(25,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
            case 3: TargetRotation = new Vector3(32.5f,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
            case 4: TargetRotation = new Vector3(40,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
            case 5: TargetRotation = new Vector3(47.5f,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
            case 6: TargetRotation = new Vector3(55,-30,0); TargetColor = new Color32(198, 255, 250, 255); break;
            case 7: TargetRotation = new Vector3(62.5f,-30,0); TargetColor = new Color32(198, 255, 250, 255); break;
            case 8: TargetRotation = new Vector3(70,-30,0); TargetColor = new Color32(198, 255, 250, 255); break;
            case 9: TargetRotation = new Vector3(77.5f,-30,0); TargetColor = new Color32(198, 255, 250, 255); break;
            case 10: TargetRotation = new Vector3(85f,-30,0); TargetColor = new Color32(225, 255, 255, 255); break;
            case 11: TargetRotation = new Vector3(87,-30,0); TargetColor = new Color32(225, 255, 255, 255); break;
            case 12: TargetRotation = new Vector3(90,-30,0); TargetColor = new Color32(255, 255, 255, 255); break;
            case 13: TargetRotation = new Vector3(96.6f,-30,0); TargetColor = new Color32(255, 255, 255, 255); break;
            case 14: TargetRotation = new Vector3(102,-30,0); TargetColor = new Color32(255, 255, 255, 255); break;
            case 15: TargetRotation = new Vector3(108,-30,0); TargetColor = new Color32(255, 255, 255, 255);break;
            case 16: TargetRotation = new Vector3(115,-30,0); TargetColor = new Color32(255, 211, 160, 255); break;
            case 17: TargetRotation = new Vector3(121,-30,0); TargetColor = new Color32(255, 211, 160, 255); break;
            case 18: TargetRotation = new Vector3(128,-30,0); TargetColor = new Color32(255, 211, 160, 255);break;
            case 19: TargetRotation = new Vector3(135,-30,0); TargetColor = new Color32(255, 187, 110, 255);break;
            case 20: TargetRotation = new Vector3(141,-30,0); TargetColor = new Color32(255, 187, 110, 255); break;
            case 21: TargetRotation = new Vector3(148,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
            case 22: TargetRotation = new Vector3(154,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
            case 23: TargetRotation = new Vector3(160,-30,0); TargetColor = new Color32(104, 222, 211, 255); break;
        }
    }

    void Update()
    {
        Transform lightTransform = Light.transform;
        Quaternion target = Quaternion.Euler(TargetRotation);
        if(Mathf.Abs(Quaternion.Angle(Light.transform.rotation, target)) > 0.1f)// Light.transform.eulerAngles.x - TargetRotation.x) > 0.1f)
        {
            lightTransform.rotation = Quaternion.Lerp(lightTransform.rotation, target, Time.deltaTime * 1.5f);
        }
        if (Mathf.Abs(Light.color.r - TargetColor.r) > 0.001f || Mathf.Abs(Light.color.g - TargetColor.g) > 0.001f || Mathf.Abs(Light.color.b - TargetColor.b) > 0.001f)
        {
            Light.color = Color.Lerp(Light.color, TargetColor, Time.deltaTime);
        }
    }
}
