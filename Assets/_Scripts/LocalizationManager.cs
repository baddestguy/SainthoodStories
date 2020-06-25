using UnityEngine;
using UnityEngine.Events;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    public static UnityEvent<Language> LanguageChanged;

    public Language CurrentLanguage;

    private void Awake()
    {
        Instance = this;
    }

    public string GetText(string key)
    {
        return GetText(key, Random.Range(0, GameDataManager.Instance.LocalizationData[key].Count));
    }

    public string GetText(string key, int sequenceNumber = 0)
    {
        var localizationData = GameDataManager.Instance.LocalizationData[key][sequenceNumber];

        switch (CurrentLanguage)
        {
            case Language.ENGLISH: return localizationData.English;
            case Language.FRENCH: return localizationData.French;
            case Language.BRPT: return localizationData.BrPt;
            case Language.FILIPINO: return localizationData.Filipino;
            case Language.LATAMSPANISH: return localizationData.LatAmSpanish;
            case Language.ITALIAN: return localizationData.Italian;
            case Language.GERMAN: return localizationData.German;
        }

        return key;
    }

    public int GetTotalSequences(string key)
    {
        return GameDataManager.Instance.LocalizationData[key].Count;
    }

    public void ChangeLanguage(Language newLanguage)
    {
        CurrentLanguage = newLanguage;

        //Fire event to update all text on all UIs in the game
        LanguageChanged?.Invoke(CurrentLanguage);
    }
}
