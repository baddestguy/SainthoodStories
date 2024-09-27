using System;
using System.Collections;
using System.IO;
using System.Linq;
using System.Runtime.Serialization.Formatters.Binary;
using Assets._Scripts.Xbox;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public bool StoryToggle;
    public bool CustomEventsToggle;
    public bool ProvisionsToggle;
    /// <summary>
    /// First Time User Experience
    /// </summary>
    public bool FTUE; //First Time User Experience!
    public bool TutorialToggle;
    public bool SkipSplashScreens;
    public bool IgnoreHouseBuildingAtEndofDay;
    [HideInInspector]
    public bool DEMO_MODE;
    public bool DEMO_MODE_2;
    public bool ShowGrid;
    public bool InfiniteBoost;
    public bool IsXboxMode;
    public XboxResolution MaxXboxResolution;
    public bool ShowFPSCounter;
    public bool TUTORIAL_MODE;
    public bool DEMO_MODE_3;
    private bool _hasRegisteredForInputMethodChanged;
    private bool _hasRegisteredForLoginStatusChanged;

    [HideInInspector]
    public bool IsUsingController
    {
        get => _isUsingController;
        set
        {
            Cursor.visible = !value;
            Cursor.lockState = value ? CursorLockMode.Locked : CursorLockMode.None;

            if (value != _isUsingController)
            {
                Debug.Log($"Input method switched to <b>{(value ? "Controller" : "Mouse")}</b>");
            }
            _isUsingController = value;
        }
    }

    private bool _isUsingController;
    public enum XboxResolution
    {
        _1080P = 2_073_600,
        _2k = 2_211_840,
        _1440P = 3_686_400,
        _4k = 8_294_400
    }

    [HideInInspector] public bool fullScreenMode;
    [HideInInspector] public QualityLevel currentQualityLevel;
    [HideInInspector] public Resolution[] resolutions;
    [HideInInspector] public Resolution currentResolution;
    [HideInInspector] public float brightnessPercent;
    [HideInInspector] public float gammaPercent;
    //You can choose what sound to enable or disable (Full bread is better than half)
    [HideInInspector] public bool sfxEnebled;
    [HideInInspector] public bool musicEnabled;
    [HideInInspector] public bool ambianceEnabled;
    //Note Volumes set here are in range of 0 to 1 ( Anything is possible with a normalized value )
    [HideInInspector] public float globalVolume;
    [HideInInspector] public float sfxVolume;
    [HideInInspector] public float musicVolume;
    [HideInInspector] public float ambianceVolume;

    [HideInInspector] public Language currentLanguage;

    string GetPath() => Path.Combine(Application.persistentDataPath, "Settings.dat");

    private void Awake()
    {
        Instance = this;
        resolutions = Screen.resolutions;
    }

    private void Start()
    {
        Load();
        if (IsXboxMode)
        {
            IsUsingController = true;
        }
    }

    private void Update()
    {
        if (!_hasRegisteredForInputMethodChanged)
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged += HandleInputMethodChanged;
            _hasRegisteredForInputMethodChanged = true;
        }
    }

    public void OnDisable()
    {
        if (_hasRegisteredForInputMethodChanged)
        {
            GameplayControllerHandler.Instance.OnInputMethodChanged -= HandleInputMethodChanged;
            _hasRegisteredForInputMethodChanged = false;
        }
    }

    private void HandleInputMethodChanged(bool isUsingController)
    {
        IsUsingController = isUsingController;
    }

    public void Load()
    {

        SaveSettingsData data = GetSavedDataSet();
        if (data != null)
        {
            fullScreenMode = data.fullscreen;
            SetQuality(data.qualityLevel);
            brightnessPercent = data.brightnessPercent;
            currentResolution = GetResolution($"{data.resolutionWidth}x{data.resolutionHeight}");
            //Sound
            sfxEnebled = data.sfxEnabled;
            musicEnabled = data.musicEnabled;
            ambianceEnabled = data.ambianceEnabled;
            TutorialToggle = data.tutorialEnabled;
            ShowGrid = data.ShowGrid;

            SetVolume("Global", data.globalVolume);
            SetVolume("Music", data.musicVolume);
            SetVolume("Music2", data.musicVolume);
            SetVolume("SFX", data.sfxVolume);
            SetVolume("Ambiance", data.ambianceVolume);
            //language
            SetLanguage(data.language);
        }
        else
        {
            fullScreenMode = true;
            SetQuality(QualityLevel.QUALITY_SETTING_MEDIUM);
            brightnessPercent = 0.5f;
            currentResolution = GetResolution($"1920x1080");
            sfxEnebled = true;
            musicEnabled = true;
            ambianceEnabled = true;
            ShowGrid = true;
            SetVolume("Global", 1);
            SetVolume("Music", 1);
            SetVolume("SFX", 0.85f);
            SetVolume("Ambiance", 1);

            SetLanguage(Language.ENGLISH);

        }


        Screen.SetResolution(currentResolution.width, currentResolution.height, fullScreenMode);

    }

    public void Save()
    {
        SaveSettingsData data = new SaveSettingsData
        {
            //Graphics
            fullscreen = fullScreenMode,
            brightnessPercent = brightnessPercent,
            resolutionHeight = currentResolution.height,
            resolutionWidth = currentResolution.width,
            qualityLevel = currentQualityLevel,
            gammaPercent = gammaPercent,
            //Sound
            sfxEnabled = sfxEnebled,
            musicEnabled = musicEnabled,
            ambianceEnabled = ambianceEnabled,
            globalVolume = globalVolume,
            sfxVolume = sfxVolume,
            musicVolume = musicVolume,
            ambianceVolume = ambianceVolume,
            tutorialEnabled = !TutorialManager.Instance.SkipTutorial,
            DEMO_MODE = DEMO_MODE,
            ShowGrid = ShowGrid,

            //Language
            language = currentLanguage
        };

        if (IsXboxMode)
        {

        }
        else
        {
            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Create(GetPath());
            bf.Serialize(file, data);
            file.Close();
        }
    }

    private SaveSettingsData GetSavedDataSet()
    {
        try
        {
            if (IsXboxMode)
            {
                return null;
            }

            BinaryFormatter bf = new BinaryFormatter();
            FileStream file = File.Open(GetPath(), FileMode.Open);
            SaveSettingsData saveObjects = (SaveSettingsData)bf.Deserialize(file);
            file.Close();

            return saveObjects;

        }
        catch (Exception e)
        {
            //Debug.LogError("Error " + e.Message);
            return null;

        }

    }

    public void ToggleGrid()
    {
        ShowGrid = !ShowGrid;
        if (GameManager.Instance.InGameSession)
        {
            GameManager.Instance.Player.RefreshGrid();
        }
    }

    public Resolution GetResolution(string value)
    {
        if (IsXboxMode)
        {
            var orderedResolutions = resolutions.Select(x => new { Resolution = x, Pixels = x.height * x.width })
                .OrderByDescending(x => x.Pixels)
                .ThenByDescending(x => x.Resolution.refreshRateRatio.value)
                .ToList();

            var bestResolutionBelowMax = orderedResolutions.FirstOrDefault(x => x.Pixels <= (int)MaxXboxResolution);

            return bestResolutionBelowMax?.Resolution ?? orderedResolutions.Last().Resolution;
        }

        string[] val = value.Replace(" ", "").Split('x');
        Resolution? res = resolutions.FirstOrDefault(x => x.width.ToString() == val[0] && x.height.ToString() == val[1]);
        if (res == null)
        {
            return Screen.currentResolution;
        }
        return res.Value;
    }

    public void SetResolution(string resolution)
    {
        currentResolution = GetResolution(resolution);
        Screen.SetResolution(currentResolution.width, currentResolution.height, fullScreenMode);
    }
    public void SetQuality(QualityLevel quality)
    {
        if (IsXboxMode)
        {
            QualitySettings.SetQualityLevel((int)QualityLevel.QUALITY_SETTING_ULTRA);
            return;
        }

        currentQualityLevel = quality;
        QualitySettings.SetQualityLevel((int)quality);
    }
    public void SetFullScreen(bool value)
    {
        fullScreenMode = value;
        Screen.SetResolution(currentResolution.width, currentResolution.height, fullScreenMode);
    }
    public void SetBrightmessLevel(float percent)
    {
        brightnessPercent = percent;
    }
    public void SetGammaLevel(float percent)
    {
        gammaPercent = percent;
    }

    public void SetVolume(string target, float percent)
    {
        switch (target)
        {
            case "Global":
                globalVolume = percent;
                SoundManager.Instance.SetGlobalAudio(true, percent);
                break;
            case "SFX":
                sfxVolume = percent;
                SoundManager.Instance.EnableSFX(sfxEnebled, sfxVolume);
                break;
            case "Music":
                musicVolume = percent;
                SoundManager.Instance.EnableMusic(musicEnabled, musicVolume);
                break;
            case "Ambiance":
                ambianceVolume = percent;
                SoundManager.Instance.EnableAmbiance(ambianceEnabled, ambianceVolume);
                break;
        }


    }

    public void EnableSound(string target, bool value)
    {
        switch (target)
        {
            case "Global":
                sfxEnebled = value;
                musicEnabled = value;
                ambianceEnabled = value;
                SoundManager.Instance.EnableSFX(sfxEnebled, sfxVolume);
                SoundManager.Instance.EnableMusic(musicEnabled, musicVolume);
                SoundManager.Instance.EnableAmbiance(ambianceEnabled, ambianceVolume);
                break;
            case "SFX":
                sfxEnebled = value;
                SoundManager.Instance.EnableSFX(sfxEnebled, sfxVolume);
                break;
            case "Music":
                musicEnabled = value;
                SoundManager.Instance.EnableMusic(musicEnabled, musicVolume);
                break;
            case "Ambiance":
                ambianceEnabled = value;
                SoundManager.Instance.EnableAmbiance(ambianceEnabled, ambianceVolume);
                break;
        }


    }

    public void SetLanguage(Language lang)
    {
        currentLanguage = lang;
    }
}