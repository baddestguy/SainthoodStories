using UnityEngine;

public class ProvisionDisplayItem : MonoBehaviour
{
    ProvisionData Provision;
    public void Init(ProvisionData prov)
    {
        Provision = prov;
        TooltipMouseOver mouseOverBtn = GetComponentInChildren<TooltipMouseOver>();
        mouseOverBtn.Loc_Key = $"<b><u>{LocalizationManager.Instance.GetText(Provision.NameKey)}: LV.{Provision.Level}</b></u>\n\n{LocalizationManager.Instance.GetText(Provision.DescriptionKey)} \n<i>{Provision.Tooltips}";
    }
}
