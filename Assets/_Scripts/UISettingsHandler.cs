using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Linq;

public class UISettingsHandler : MonoBehaviour
{

    public static UISettingsHandler instance;

    [Header("Graphics")]
    public Toggle fullscreenToggle;
    public TMP_Dropdown quality;
    public TMP_Dropdown resolution;
    public Slider brightness;
    public Slider gamma;
    public Sprite dropdownSprite;

    

    [Header("Sounds")]
    public Toggle soundEnable;
    //public Toggle musicEnebled;
    public Slider global;
    public Slider SFX;
    public Slider Music;
    public Slider Ambiance;

    [Header("Language")]
    public TMP_Dropdown language;

    private void Start()
    {
        instance = this;
        InitializeGraphicsUi();
        //Sound
        InitilaizeSound();
        //Language
        InitalizeLanguage();

    }


    //Edit as needed
    private void InitializeGraphicsUi()
    {
        List<TMP_Dropdown.OptionData> qualityData = new List<TMP_Dropdown.OptionData>();
        QualityLevel[] gtypes = (QualityLevel[])Enum.GetValues(typeof(QualityLevel));
        for (int i = 0; i < gtypes.Length; i++)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData(LocalizationManager.Instance.GetText($"{gtypes[i]}"), dropdownSprite);
            qualityData.Add(data);
        }
        quality.options = qualityData;
        quality.value = GetCurrentUIGraphicsQualityIndex(GameSettings.Instance.currentQualityLevel);


        List<TMP_Dropdown.OptionData> resolutionData = new List<TMP_Dropdown.OptionData>();
        for (int i = 0; i < Screen.resolutions.Length; i++)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData($"{Screen.resolutions[i].width} x {Screen.resolutions[i].height}", dropdownSprite);
            resolutionData.Add(data);
        }
        resolution.options = new List<TMP_Dropdown.OptionData>();
        resolution.options = resolutionData;
        resolution.value = GetCurrentUIResolutionIndex(GameSettings.Instance.currentResolution);

        //remove First
        quality.onValueChanged.RemoveAllListeners();
        fullscreenToggle.onValueChanged.RemoveAllListeners();
        resolution.onValueChanged.RemoveAllListeners();
        brightness.onValueChanged.RemoveAllListeners();
        gamma.onValueChanged.RemoveAllListeners();

        //Than Add
        fullscreenToggle.isOn = GameSettings.Instance.fullScreenMode;
        fullscreenToggle.onValueChanged.AddListener((value) => {
            //print(value);
            GameSettings.Instance.SetFullScreen(value);
        });
        quality.onValueChanged.AddListener((value) => {
            GameSettings.Instance.SetQuality((QualityLevel)value);
        });
        resolution.onValueChanged.AddListener((index) => {
            //print(GetSelectedResolutionFromIndex(index));
           
            GameSettings.Instance.SetResolution(GetSelectedResolutionFromIndex(index));
        });
    }

    //Edit as needed
    private void InitilaizeSound()
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
            GameSettings.Instance.EnableSound("Global",value);
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

    //Edit as needed
    private void InitalizeLanguage()
    {
        List<TMP_Dropdown.OptionData> langData = new List<TMP_Dropdown.OptionData>();
        Language[] lang = (Language[])Enum.GetValues(typeof(Language));
        for (int i = 0; i < lang.Length; i++)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData($"{lang[i]}", dropdownSprite);
            langData.Add(data);
        }
        language.options = langData;
        language.value = GetCurrentUILanguageIndex(GameSettings.Instance.currentLanguage);

        language.onValueChanged.RemoveAllListeners();
        language.onValueChanged.AddListener((value) =>
        {
            GameSettings.Instance.SetLanguage((Language)value);
        });
    }


    public int GetCurrentUIGraphicsQualityIndex(QualityLevel graphicsType)
    {
        return quality.options.FindIndex(x => x.text == LocalizationManager.Instance.GetText($"{graphicsType}"));
    }
    public int GetCurrentUIResolutionIndex(Resolution res)
    {
        return resolution.options.FindIndex(x => x.text == $"{res.width}x{res.height}");
    }
    public string GetSelectedResolutionFromIndex(int index)
    {
        return resolution.options[index].text;
    }

    public int GetCurrentUILanguageIndex(Language lang)
    {
        return language.options.FindIndex(x => x.text == lang.ToString());
    }
}
