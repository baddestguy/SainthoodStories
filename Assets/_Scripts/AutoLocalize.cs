using TMPro;
using UnityEngine;

public class AutoLocalize : MonoBehaviour
{
    public string Key;

    void Start()
    {
        LocalizationManager.LanguageChanged += Localize;
        Localize(LocalizationManager.Instance.CurrentLanguage);
    }

    public void Localize(Language language)
    {
        GetComponent<TextMeshProUGUI>().text = LocalizationManager.Instance.GetText(Key);
    }

    private void OnDisable()
    {
        LocalizationManager.LanguageChanged -= Localize;
    }
}
