using UnityEngine;
using UnityEngine.Events;

public class LocalizationManager : MonoBehaviour
{
    public static LocalizationManager Instance { get; private set; }
    public static UnityAction<Language> LanguageChanged;

    public Language CurrentLanguage;

    private void Awake()
    {
        Instance = this;
        CurrentLanguage = (Language)PlayerPrefs.GetInt("Language");
    }

    public string GetText(string key)
    {
        if (!GameDataManager.Instance.LocalizationData.ContainsKey(key)) return key;
        return GetText(key, Random.Range(0, GameDataManager.Instance.LocalizationData[key].Count));
    }

    public string GetText(string key, int sequenceNumber = 0)
    {
        if (!GameDataManager.Instance.LocalizationData.ContainsKey(key)) return key;
        var localizationData = GameDataManager.Instance.LocalizationData[key][sequenceNumber];
        switch (CurrentLanguage)
        {
            case Language.ENGLISH: return localizationData.English;
            case Language.FRENCH: return FixHTMLTags(localizationData.French);
            case Language.BRPT: return FixHTMLTags(localizationData.BrPt);
            case Language.FILIPINO: return FixHTMLTags(localizationData.Filipino);
            case Language.LATAMSPANISH: return FixHTMLTags(localizationData.LatAmSpanish);
            case Language.ITALIAN: return FixHTMLTags(localizationData.Italian);
            case Language.GERMAN: return FixHTMLTags(localizationData.German);
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
        PlayerPrefs.SetInt("Language", (int)newLanguage);
        LanguageChanged?.Invoke(CurrentLanguage);
    }

    public string FixHTMLTags(string data)
    {
        if (CurrentLanguage == Language.ENGLISH) return data;

        data = data.Replace("<B> ", "<b>");
        data = data.Replace("<b> ", "<b>");
        data = data.Replace("</ b>", "</b>");
        data = data.Replace("<color = « red »>", "<color=\"red\">");
        data = data.Replace("<color = „red“>", "<color=\"red\">");
        data = data.Replace("<color = \"red\">", "<color=\"red\">");
        data = data.Replace("<color = \"green\">", "<color=\"green\">");
        data = data.Replace("<Color = \"blanc\">", "<color=\"white\">");
        data = data.Replace("<color = \"blanc\">", "<color=\"white\">");
        data = data.Replace("<Color = \"white\">", "<color=\"white\">");
        data = data.Replace("<color = \"white\">", "<color=\"white\">");
        data = data.Replace("<Color = \"blanco\">", "<color=\"white\">");
        data = data.Replace("<color = \"blanco\">", "<color=\"white\">");
        data = data.Replace("<Color = \"bianco\">", "<color=\"white\">");
        data = data.Replace("<color = \"bianco\">", "<color=\"white\">");
        data = data.Replace("<Color = \"puti\">", "<color=\"white\">");
        data = data.Replace("<color = \"puti\">", "<color=\"white\">");
        data = data.Replace("(FR) ", "(EN)");
        data = data.Replace("(ES) ", "(EN)");
        data = data.Replace("(DE) ", "(EN)");

        return data;
    }
}
