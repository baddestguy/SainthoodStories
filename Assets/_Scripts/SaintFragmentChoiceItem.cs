using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaintFragmentChoiceItem : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public string ResponseKey;

    public void Init(SaintsEvent currentEvent, string choiceKey, string response)
    {
        var text = LocalizationManager.Instance.GetText(choiceKey);
        Text.text = $"{currentEvent.FontColor}{text}";
        Text.color = new Color(Text.color.r, Text.color.g, Text.color.b, 0f);
        Text.DOFade(1f, 1f).SetEase(Ease.Linear);
        
        Extensions.TryExtractColorFromRichText(currentEvent.FontColor, out Color c);
        
        var img = GetComponent<Image>();
        img.color = new Color(c.r, c.g, c.b, 0f);
        img.DOFade(1f, 1f).SetEase(Ease.Linear);

        ResponseKey = response;
    }

    public void Select()
    {
        var newevent = GameDataManager.Instance.SaintsEvent[ResponseKey];
        SendMessageUpwards("SelectFragmentChoice", newevent);
    }
}
