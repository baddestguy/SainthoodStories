using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaintCard : MonoBehaviour
{
    public SaintData Saint;
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI DescriptionText;
    public Image SaintIcon;

    public void Init(SaintData data)
    {
        Saint = data;
        NameText.text = Saint.Name;
        DescriptionText.text = 
            $@"{LocalizationManager.Instance.GetText("Born")}: {Saint.Birthday}
{LocalizationManager.Instance.GetText("Died")}: {Saint.Death}
{LocalizationManager.Instance.GetText("FeastDay")}: {Saint.FeastDay}
{LocalizationManager.Instance.GetText("Patron")}: {LocalizationManager.Instance.GetText(Saint.PatronKey)}";
        SaintIcon.sprite = Resources.Load<Sprite>(Saint.IconPath);
    }
}
