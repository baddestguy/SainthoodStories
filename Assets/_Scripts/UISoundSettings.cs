using UnityEngine;
using UnityEngine.UI;

public class UISoundSettings : MonoBehaviour
{
    public static UISoundSettings Instance;

    [Header("Sounds")]
    public Toggle soundEnable;
    //public Toggle musicEnebled;
    public Slider global;
    public Slider SFX;
    public Slider Music;
    public Slider Ambiance;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        InitilaizeSoundUI();
    }

    private void InitilaizeSoundUI()
    {
        soundEnable.onValueChanged.RemoveAllListeners();
        global.onValueChanged.RemoveAllListeners();
        SFX.onValueChanged.RemoveAllListeners();
        Music.onValueChanged.RemoveAllListeners();
        Ambiance.onValueChanged.RemoveAllListeners();

        soundEnable.isOn = GameSettings.Instance.ambianceEnabled || GameSettings.Instance.musicEnabled || GameSettings.Instance.sfxEnebled;
        soundEnable.onValueChanged.AddListener((value) =>
        {
            print(value);
            GameSettings.Instance.EnableSound("Global", value);
        });

        global.value = GameSettings.Instance.globalVolume;
        global.onValueChanged.AddListener((value) =>
        {
            GameSettings.Instance.SetVolume("Global", value);
        });

        SFX.value = GameSettings.Instance.sfxVolume;
        SFX.onValueChanged.AddListener((value) =>
        {
            GameSettings.Instance.SetVolume("SFX", value);
        });

        Music.value = GameSettings.Instance.musicVolume;
        Music.onValueChanged.AddListener((value) => {
            GameSettings.Instance.SetVolume("Music", value);
        });

        Ambiance.value = GameSettings.Instance.ambianceVolume;
        Ambiance.onValueChanged.AddListener((value) => {
            GameSettings.Instance.SetVolume("Ambiance", value);
        });
    }
}
