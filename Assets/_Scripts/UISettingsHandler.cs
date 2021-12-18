using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

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
        //Language
        InitalizeLanguage();

    }


    //Edit as needed
   

    //Edit as needed
    

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



    public int GetCurrentUILanguageIndex(Language lang)
    {
        return language.options.FindIndex(x => x.text == lang.ToString());
    }
}
