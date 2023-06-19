using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProvisionUIItem : MonoBehaviour
{
    private ProvisionData Provision;
    public TextMeshProUGUI ProvisionName;
    public Image ProvisionIcon;
    public TextMeshProUGUI ProvisionDescription;
    public ProvisionUIItemType Type;

    public void Init(ProvisionData prov, ProvisionUIItemType itemType)
    {
        Provision = prov;
        //ProvisionName.text = LocalizationManager.Instance.GetText(prov.NameKey);
        //ProvisionDescription.text = LocalizationManager.Instance.GetText(prov.DescriptionKey);
        ProvisionIcon.gameObject.SetActive(true);
        ProvisionIcon.sprite = Resources.Load<Sprite>($"Icons/{prov.Id}");
        Type = itemType;

        TooltipMouseOver mouseOverBtn = GetComponentInChildren<TooltipMouseOver>();
        if(Type == ProvisionUIItemType.NEW)
        {
            mouseOverBtn.Loc_Key = "<b>"+LocalizationManager.Instance.GetText(prov.NameKey) + "</b>\n\n<b>NEW</b>\n" + LocalizationManager.Instance.GetText(prov.DescriptionKey) + "\n\n<i>" + prov.Tooltips;
        }
        else
        {
            ProvisionData nextLevel = GameDataManager.Instance.GetProvision(prov.Id, prov.Level + 1);
            if(nextLevel == null)
            {
                mouseOverBtn.Loc_Key = $"<b>{ LocalizationManager.Instance.GetText(prov.NameKey)}</b>\n\n<b>MAX LEVEL</b>\n{LocalizationManager.Instance.GetText(prov.DescriptionKey)}\n\n<i>LV.{prov.Level}\n{prov.Tooltips}";
            }
            else
            {
                mouseOverBtn.Loc_Key = $"<b>{ LocalizationManager.Instance.GetText(prov.NameKey)}</b>\n\n<b>UPGRADE\tLV{prov.Level} -> LV.{nextLevel.Level}</b>\n{LocalizationManager.Instance.GetText(nextLevel.DescriptionKey)}\n\n<i>LV.{prov.Level}\n{prov.Tooltips} \n<color=\"white\">--\nLV.{nextLevel.Level}\n{nextLevel.Tooltips}";
            }
        }
    }

    public void OnClick()
    {
        if (Provision == null) return;
        if (Type == ProvisionUIItemType.NEW) SendMessageUpwards("AddNewProvision", Provision, SendMessageOptions.RequireReceiver);

        if (Type == ProvisionUIItemType.UPGRADE) SendMessageUpwards("UpgradeProvision", Provision, SendMessageOptions.RequireReceiver);

        ToolTipManager.Instance.ShowToolTip("");
    }
}