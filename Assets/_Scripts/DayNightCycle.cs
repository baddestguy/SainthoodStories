using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI.Table;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField]
    public Light Light;
    //public PostProcessVolume PostProcessor;
    //public PostProcessVolume PostProcessorInterior;

    //private Bloom Bloom;
    //private Bloom BloomInterior;
    private Vector3 TargetRotation = new Vector3(0, 0, 0);
    private Color TargetColor = new Color();
    private float ShadowStrength;

    public Material CurrentSkybox;
    public List<Texture> DaySkyboxTextures;
    public List<Texture> EveningSkyboxTextures;
    public List<Texture> NightSkyboxTextures;
    public List<Texture> RainSkyboxTextures;

    private float BlendValue = 0f;
    private bool LockSkybox;

    public List<TimeOfDayKeyframe> TimeOfDayKeyFrames = new List<TimeOfDayKeyframe>();
    private int lastMinute = -1;

    public DayNight DayNight
    {
        get { 
            if (GameManager.Instance.GameClock.Time >= 6 && GameManager.Instance.GameClock.Time < 19) return DayNight.DAY;
            else return DayNight.NIGHT; 
        }
    }

    void Start()
    {
        GameClock.Ticked += OnTick;
        ShadowStrength = Light.shadowStrength;
        //PostProcessor.profile.TryGetSettings(out Bloom);
        //PostProcessorInterior.profile.TryGetSettings(out BloomInterior);

        TimeOfDayKeyFrames.AddRange(new List<TimeOfDayKeyframe>() 
        {
            new TimeOfDayKeyframe { time = 0, rotation = new Vector3(10, -30, 0), color = new Color32(104, 222, 211, 255), skybox = NightSkyboxTextures },
            new TimeOfDayKeyframe { time = 6, rotation = new Vector3(55, -30, 0), color = new Color32(198, 255, 250, 255), skybox = DaySkyboxTextures },
            new TimeOfDayKeyframe { time = 12, rotation = new Vector3(90, -30, 0), color = Color.white, skybox = DaySkyboxTextures },
            new TimeOfDayKeyframe { time = 16, rotation = new Vector3(115, -30, 0), color = new Color32(255, 211, 160, 255), skybox = EveningSkyboxTextures },
            new TimeOfDayKeyframe { time = 19, rotation = new Vector3(135, -30, 0), color = new Color32(104, 222, 211, 255), skybox = NightSkyboxTextures },
            new TimeOfDayKeyframe { time = 24, rotation = new Vector3(170, -30, 0), color = new Color32(104, 222, 211, 255), skybox = NightSkyboxTextures }
        });

    //    StartingSkybox();
        int minute = DateTime.Now.Minute;
        lastMinute = minute;
        float time = DateTime.Now.Hour + (minute / 60f);
        UpdateLighting(time);
        FindFirstObjectByType<LightColors>(FindObjectsInactive.Include).UpdateLight(time);
    }


    private void OnTick(double time, int day)
    {
    }

    void UpdateLighting(float currentTime)
    {
        // Normalize to 0–24
        if (currentTime < 0) currentTime = 0;
        if (currentTime >= 24) currentTime -= 24;

        TimeOfDayKeyframe a = null, b = null;

        // Find keyframes surrounding the time
        for (int i = 0; i < TimeOfDayKeyFrames.Count - 1; i++)
        {
            if (currentTime >= TimeOfDayKeyFrames[i].time && currentTime <= TimeOfDayKeyFrames[i + 1].time)
            {
                a = TimeOfDayKeyFrames[i];
                b = TimeOfDayKeyFrames[i + 1];
                break;
            }
        }

        if (a == null || b == null)
            return;

        float t = Mathf.InverseLerp(a.time, b.time, currentTime);

        TargetRotation = Vector3.Lerp(a.rotation, b.rotation, t);
        TargetColor = Color.Lerp(a.color, b.color, t);

        // Skybox switches immediately at a keyframe boundary
        SetFutureSkybox(t < 0.5f ? a.skybox : b.skybox);

        LockSkybox = true;
    }   

    void Update()
    {
        Transform lightTransform = Light.transform;
        Quaternion target = Quaternion.Euler(TargetRotation);
        if(Mathf.Abs(Quaternion.Angle(Light.transform.rotation, target)) > 0.1f)
        {
            lightTransform.rotation = Quaternion.Lerp(lightTransform.rotation, target, Time.deltaTime * 1.5f);
        }

        //if (Mathf.Abs(Bloom.color.GetValue<Color>().r - TargetBloomColor.r) > 0.001f || Mathf.Abs(Bloom.color.GetValue<Color>().g - TargetBloomColor.g) > 0.001f || Mathf.Abs(Bloom.color.GetValue<Color>().b - TargetBloomColor.b) > 0.001f)
        //{
        ////    Bloom.color.Override(Color.Lerp(Bloom.color.GetValue<Color>(), TargetBloomColor, Time.deltaTime * 0.2f));
        //    //BloomInterior.color.Override(Color.Lerp(Bloom.color.GetValue<Color>(), TargetBloomColor, Time.deltaTime * 0.2f));
        //}

        if (Mathf.Abs(Light.color.r - TargetColor.r) > 0.001f || Mathf.Abs(Light.color.g - TargetColor.g) > 0.001f || Mathf.Abs(Light.color.b - TargetColor.b) > 0.001f)
        {
            Light.color = Color.Lerp(Light.color, TargetColor, Time.deltaTime);
        }

        if (!WeatherManager.Instance.IsNormal())
        {
            Light.shadowStrength = Mathf.Lerp(Light.shadowStrength, 0.6f, Time.deltaTime);
        }
        else
        {
            Light.shadowStrength = Mathf.Lerp(Light.shadowStrength, ShadowStrength, Time.deltaTime);
        }

        //    CurrentSkybox.SetFloat("_Blend", Mathf.Lerp(CurrentSkybox.GetFloat("_Blend"), BlendValue, Time.deltaTime));

        //Bloom.intensity.Override(Mathf.Lerp(Bloom.intensity.GetValue<float>(), TargetBloomIntensity, Time.deltaTime * 0.2f));
        ////BloomInterior.intensity.Override(Mathf.Lerp(Bloom.intensity.GetValue<float>(), TargetBloomIntensity, Time.deltaTime * 0.2f));
        //if(Mathf.Abs(CurrentSkybox.GetFloat("_Blend") - BlendValue) < 0.01f)
        //{
        //    if(WeatherManager.Instance.IsNormal())
        //    {
        //        LockSkybox = false;
        //        SetDayNight();
        //    }
        //}

        int minute = DateTime.Now.Minute;

        if (minute != lastMinute && (minute % 15 == 0))
        {
            lastMinute = minute;
            float time = DateTime.Now.Hour + (minute / 60f);
            UpdateLighting(time);
            FindFirstObjectByType<LightColors>(FindObjectsInactive.Include).UpdateLight(time);
        }
    }

    public void SetFutureSkyBox(WeatherType type)
    {
        GameClock clock = GameManager.Instance.GameClock;

        switch (type)
        {
            case WeatherType.PRESTORM:
            case WeatherType.RAIN:
            case WeatherType.SNOW:
            case WeatherType.HAIL:
                LockSkybox = false;
                SetFutureSkybox(RainSkyboxTextures);
                LockSkybox = true;
                return;

            case WeatherType.NONE:
            //    LockSkybox = false;
            //    SetDayNight();
            //    if(clock.Time > 19)
            //        TargetBloomIntensity = 2.5f;
            //    else
            //        TargetBloomIntensity = 5f;
            //    Bloom.threshold.Override(1f);
                return;
        }
    }

    private void SetFutureSkybox(List<Texture> newTextures)
    {
        if (LockSkybox) return;

        GameClock clock = GameManager.Instance.GameClock;

        if (CurrentSkybox.GetTexture("_FrontTex") == newTextures[0])
        {
            return;
        }

        CurrentSkybox.SetTexture("_FrontTex", newTextures[0]);
        CurrentSkybox.SetTexture("_BackTex", newTextures[1]);
        CurrentSkybox.SetTexture("_LeftTex", newTextures[2]);
        CurrentSkybox.SetTexture("_RightTex", newTextures[3]);
        CurrentSkybox.SetTexture("_UpTex", newTextures[4]);
        CurrentSkybox.SetTexture("_DownTex", newTextures[5]);
    }

    public void RunDayNightCycle()
    {
        StartCoroutine(RunDayNightCycleAsync());
    }

    private IEnumerator RunDayNightCycleAsync()
    {
        yield return new WaitForSeconds(1f);
        var clock = GameManager.Instance.GameClock;
        while (true)
        {
            clock.Tick();
            yield return new WaitForSeconds(0.1f);
        }
    }
}


[System.Serializable]
public class TimeOfDayKeyframe
{
    public float time;  // 0–24
    public Vector3 rotation;
    public Color color;
    public List<Texture> skybox;
}
