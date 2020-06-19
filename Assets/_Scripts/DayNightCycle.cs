using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

public class DayNightCycle : MonoBehaviour
{
    [SerializeField]
    public Light Light;
    public PostProcessVolume PostProcessor;

    private Bloom Bloom;
    private Vector3 TargetRotation = new Vector3(0, 0, 0);
    private Color TargetColor = new Color();
    private Color TargetBloomColor = new Color();
    private float TargetBloomIntensity = 5;
    private float ShadowStrength;

    public Material CurrentSkybox;
    public List<Texture> DaySkyboxTextures;
    public List<Texture> EveningSkyboxTextures;
    public List<Texture> NightSkyboxTextures;
    public List<Texture> RainSkyboxTextures;

    private float BlendValue = 0f;
    private bool LockSkybox;
    void Start()
    {
        GameClock.Ticked += OnTick;
        ShadowStrength = Light.shadowStrength;
        PostProcessor.profile.TryGetSettings(out Bloom);
        CurrentSkybox.SetFloat("_Blend", 0);
        StartingSkybox();
        OnTick(GameManager.Instance.GameClock.Time, GameManager.Instance.GameClock.Day);
    }

    private void StartingSkybox()
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (clock.Time >= 21 || clock.Time < 6)
        {
            OverrideSkybox(NightSkyboxTextures);
        }
        else if (clock.Time >= 16)
        {
            OverrideSkybox(EveningSkyboxTextures);
        }
        else if (clock.Time >= 6)
        {
            OverrideSkybox(DaySkyboxTextures);
        }
    }

    private void OverrideSkybox(List<Texture> newTextures)
    {
        CurrentSkybox.SetTexture("_FrontTex2", newTextures[0]);
        CurrentSkybox.SetTexture("_BackTex2", newTextures[1]);
        CurrentSkybox.SetTexture("_LeftTex2", newTextures[2]);
        CurrentSkybox.SetTexture("_RightTex2", newTextures[3]);
        CurrentSkybox.SetTexture("_UpTex2", newTextures[4]);
        CurrentSkybox.SetTexture("_DownTex2", newTextures[5]);
        CurrentSkybox.SetTexture("_FrontTex", newTextures[0]);
        CurrentSkybox.SetTexture("_BackTex", newTextures[1]);
        CurrentSkybox.SetTexture("_LeftTex", newTextures[2]);
        CurrentSkybox.SetTexture("_RightTex", newTextures[3]);
        CurrentSkybox.SetTexture("_UpTex", newTextures[4]);
        CurrentSkybox.SetTexture("_DownTex", newTextures[5]);
    }

    private void OnTick(double time, int day)
    {
        SetDayNight();
    }

    private void SetDayNight()
    {
        GameClock clock = GameManager.Instance.GameClock;

        switch (clock.Time)
        {
            case 0: TargetRotation = new Vector3(10, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 0.5: TargetRotation = new Vector3(13, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 1: TargetRotation = new Vector3(17.5f, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 1.5: TargetRotation = new Vector3(21.5f, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 2: TargetRotation = new Vector3(25, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 2.5: TargetRotation = new Vector3(28, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 3: TargetRotation = new Vector3(32.5f, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 3.5: TargetRotation = new Vector3(36f, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 4: TargetRotation = new Vector3(40, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 4.5: TargetRotation = new Vector3(43, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 5: TargetRotation = new Vector3(47.5f, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 5.5: TargetRotation = new Vector3(50f, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 6: TargetRotation = new Vector3(55, -30, 0); TargetColor = new Color32(198, 255, 250, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 6.5: TargetRotation = new Vector3(58, -30, 0); TargetColor = new Color32(198, 255, 250, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 7: TargetRotation = new Vector3(62.5f, -30, 0); TargetColor = new Color32(198, 255, 250, 255);TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 7.5: TargetRotation = new Vector3(66f, -30, 0); TargetColor = new Color32(198, 255, 250, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 8: TargetRotation = new Vector3(70, -30, 0); TargetColor = new Color32(198, 255, 250, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 8.5: TargetRotation = new Vector3(75, -30, 0); TargetColor = new Color32(198, 255, 250, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 9: TargetRotation = new Vector3(77.5f, -30, 0); TargetColor = new Color32(198, 255, 250, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 9.5: TargetRotation = new Vector3(81f, -30, 0); TargetColor = new Color32(198, 255, 250, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 10: TargetRotation = new Vector3(85f, -30, 0); TargetColor = new Color32(225, 255, 255, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 10.5: TargetRotation = new Vector3(86f, -30, 0); TargetColor = new Color32(225, 255, 255, 255); TargetBloomColor = Color.cyan; SetFutureSkybox(DaySkyboxTextures); break;
            case 11: TargetRotation = new Vector3(87, -30, 0); TargetColor = new Color32(225, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 11.5: TargetRotation = new Vector3(88.5f, -30, 0); TargetColor = new Color32(225, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 12: TargetRotation = new Vector3(90, -30, 0); TargetColor = new Color32(255, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break; 
            case 12.5: TargetRotation = new Vector3(93, -30, 0); TargetColor = new Color32(255, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 13: TargetRotation = new Vector3(96.6f, -30, 0); TargetColor = new Color32(255, 255, 255, 255);TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 13.5: TargetRotation = new Vector3(99f, -30, 0); TargetColor = new Color32(255, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 14: TargetRotation = new Vector3(102, -30, 0); TargetColor = new Color32(255, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 14.5: TargetRotation = new Vector3(105, -30, 0); TargetColor = new Color32(255, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 15: TargetRotation = new Vector3(108, -30, 0); TargetColor = new Color32(255, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 15.5: TargetRotation = new Vector3(112, -30, 0); TargetColor = new Color32(255, 255, 255, 255); TargetBloomColor = Color.white; SetFutureSkybox(DaySkyboxTextures); break;
            case 16: TargetRotation = new Vector3(115, -30, 0); TargetColor = new Color32(255, 211, 160, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 16.5: TargetRotation = new Vector3(117, -30, 0); TargetColor = new Color32(255, 211, 160, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 17: TargetRotation = new Vector3(121, -30, 0); TargetColor = new Color32(255, 211, 160, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 17.5: TargetRotation = new Vector3(125, -30, 0); TargetColor = new Color32(255, 211, 160, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 18: TargetRotation = new Vector3(128, -30, 0); TargetColor = new Color32(255, 211, 160, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 18.5: TargetRotation = new Vector3(131, -30, 0); TargetColor = new Color32(255, 211, 160, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 19: TargetRotation = new Vector3(135, -30, 0); TargetColor = new Color32(255, 187, 110, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 19.5: TargetRotation = new Vector3(137, -30, 0); TargetColor = new Color32(255, 187, 110, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 20: TargetRotation = new Vector3(141, -30, 0); TargetColor = new Color32(255, 187, 110, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 20.5: TargetRotation = new Vector3(145, -30, 0); TargetColor = new Color32(255, 187, 110, 255); TargetBloomColor = Color.red; SetFutureSkybox(EveningSkyboxTextures); break;
            case 21: TargetRotation = new Vector3(148, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 21.5: TargetRotation = new Vector3(151, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 22: TargetRotation = new Vector3(154, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 22.5: TargetRotation = new Vector3(156, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 23: TargetRotation = new Vector3(160, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
            case 23.5: TargetRotation = new Vector3(162, -30, 0); TargetColor = new Color32(104, 222, 211, 255); TargetBloomColor = Color.blue; SetFutureSkybox(NightSkyboxTextures); break;
        }

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

        if (Mathf.Abs(Bloom.color.GetValue<Color>().r - TargetBloomColor.r) > 0.001f || Mathf.Abs(Bloom.color.GetValue<Color>().g - TargetBloomColor.g) > 0.001f || Mathf.Abs(Bloom.color.GetValue<Color>().b - TargetBloomColor.b) > 0.001f)
        {
            Bloom.color.Override(Color.Lerp(Bloom.color.GetValue<Color>(), TargetBloomColor, Time.deltaTime * 0.2f));
        }

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

        CurrentSkybox.SetFloat("_Blend", Mathf.Lerp(CurrentSkybox.GetFloat("_Blend"), BlendValue, Time.deltaTime));

        Bloom.intensity.Override(Mathf.Lerp(Bloom.intensity.GetValue<float>(), TargetBloomIntensity, Time.deltaTime * 0.2f));
        if(Mathf.Abs(CurrentSkybox.GetFloat("_Blend") - BlendValue) < 0.01f)
        {
            if(WeatherManager.Instance.IsNormal())
            {
                LockSkybox = false;
                SetDayNight();
            }
        }
    }

    public void SetFutureSkyBox(WeatherType type)
    {
        switch (type)
        {
            case WeatherType.PRERAIN:
            case WeatherType.RAIN:
            case WeatherType.SNOW:
                LockSkybox = false;
                SetFutureSkybox(RainSkyboxTextures);
                TargetBloomIntensity = 1;
                LockSkybox = true;
                return;

            case WeatherType.DAY:
            case WeatherType.NIGHT:
                LockSkybox = false;
                SetDayNight();
                TargetBloomIntensity = 5;
                return;
        }
    }

    private void SetFutureSkybox(List<Texture> newTextures)
    {
        if (LockSkybox) return;

        if(BlendValue < 0.5f)
        {
            if (CurrentSkybox.GetTexture("_FrontTex2") == newTextures[0])
            {
                return;
            }

            CurrentSkybox.SetTexture("_FrontTex2", newTextures[0]);
            CurrentSkybox.SetTexture("_BackTex2", newTextures[1]);
            CurrentSkybox.SetTexture("_LeftTex2", newTextures[2]);
            CurrentSkybox.SetTexture("_RightTex2", newTextures[3]);
            CurrentSkybox.SetTexture("_UpTex2", newTextures[4]);
            CurrentSkybox.SetTexture("_DownTex2", newTextures[5]);
        }
        else
        {
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

        BlendValue = BlendValue < 0.5f ? 1f : 0f;
    }
}
