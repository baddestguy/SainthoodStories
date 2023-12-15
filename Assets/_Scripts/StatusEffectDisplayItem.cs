using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class StatusEffectDisplayItem : MonoBehaviour
{
    private StatusEffectData StatusEffectData;
    public TextMeshProUGUI StatusEffectName;
    public Image StatusEffectIcon;
    public TextMeshProUGUI StatusEffectDescription;

    public void Init(PlayerStatusEffect effectId)
    {
        StatusEffectData = GameDataManager.Instance.GetStatusEffectData(effectId);
        if (StatusEffectData == null) return;

        StatusEffectName.text = LocalizationManager.Instance.GetText(StatusEffectData.NameKey);
        StatusEffectDescription.text = LocalizationManager.Instance.GetText(StatusEffectData.DescriptionKey);
        StatusEffectIcon.sprite = Resources.Load<Sprite>($"Icons/Ailment");

        TooltipMouseOver mouseOverBtn = GetComponentInChildren<TooltipMouseOver>();
        mouseOverBtn.Loc_Key = $"<b>{LocalizationManager.Instance.GetText(StatusEffectData.NameKey)}</b>\n{LocalizationManager.Instance.GetText(StatusEffectData.DescriptionKey)}\n\n<i>{StatusEffectData.Tooltips}";
    }
}
