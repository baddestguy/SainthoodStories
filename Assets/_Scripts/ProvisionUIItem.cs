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
        ProvisionName.text = LocalizationManager.Instance.GetText(prov.NameKey);
        ProvisionDescription.text = LocalizationManager.Instance.GetText(prov.DescriptionKey);
        ProvisionIcon.sprite = Resources.Load<Sprite>($"Icons/{prov.Id}");
        Type = itemType;

        TooltipMouseOver mouseOverBtn = GetComponentInChildren<TooltipMouseOver>();
        if(Type == ProvisionUIItemType.NEW)
        {
            mouseOverBtn.Loc_Key = "<b>NEW</b>\n" + LocalizationManager.Instance.GetText(prov.DescriptionKey) + "\n\n<i>" + prov.Tooltips;
        }
        else
        {
            ProvisionData previousLevel = GameDataManager.Instance.GetProvision(prov.Id, prov.Level - 1);
            mouseOverBtn.Loc_Key = $"<b>UPGRADE\tLV{previousLevel.Level} -> LV.{prov.Level}</b>\n{LocalizationManager.Instance.GetText(prov.DescriptionKey)}\n\n<i>LV.{previousLevel.Level}\n{previousLevel.Tooltips} \n<color=\"white\">--\nLV.{prov.Level}\n{prov.Tooltips}";
        }
    }

    public void OnClick()
    {
        if(Type == ProvisionUIItemType.NEW) SendMessageUpwards("AddNewProvision", Provision, SendMessageOptions.RequireReceiver);

        if(Type == ProvisionUIItemType.UPGRADE) SendMessageUpwards("UpgradeProvision", Provision, SendMessageOptions.RequireReceiver);

        ToolTipManager.Instance.ShowToolTip("");
    }
}