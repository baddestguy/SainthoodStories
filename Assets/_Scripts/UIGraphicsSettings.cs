using System;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UIGraphicsSettings : MonoBehaviour
{
    public static UIGraphicsSettings Instance;

    [Header("Graphics")]
    public Toggle fullscreenToggle;
    public TMP_Dropdown quality;
    public TMP_Dropdown resolution;
    public Slider brightness;
    public Slider gamma;
    public Sprite dropdownSprite;

    private void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        InitializeGraphicsUI();
    }

    private void InitializeGraphicsUI()
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
        var resolutions = Screen.resolutions.Select(resolution => new Resolution { width = resolution.width, height = resolution.height }).Distinct().ToArray();
        for (int i = 0; i < resolutions.Length; i++)
        {
            TMP_Dropdown.OptionData data = new TMP_Dropdown.OptionData($"{resolutions[i].width} x {resolutions[i].height}", dropdownSprite);
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

    public int GetCurrentUIGraphicsQualityIndex(QualityLevel graphicsType)
    {
        return quality.options.FindIndex(x => x.text == LocalizationManager.Instance.GetText($"{graphicsType}"));
    }
    public int GetCurrentUIResolutionIndex(Resolution res)
    {
        return resolution.options.FindIndex(x => x.text == $"{res.width} x {res.height}");
    }
    public string GetSelectedResolutionFromIndex(int index)
    {
        return resolution.options[index].text;
    }

}
