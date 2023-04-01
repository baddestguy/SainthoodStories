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
        mouseOverBtn.Loc_Key = prov.DescriptionKey;
    }

    public void OnClick()
    {
        if(Type == ProvisionUIItemType.NEW) SendMessageUpwards("AddNewProvision", Provision, SendMessageOptions.RequireReceiver);

        if(Type == ProvisionUIItemType.UPGRADE) SendMessageUpwards("UpgradeProvision", Provision, SendMessageOptions.RequireReceiver);
    }
}