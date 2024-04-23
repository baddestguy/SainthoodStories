using DG.Tweening;
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
    public bool HasProvision => Provision != null;

    private string HeaderColor = "<color=#3B2E1F>";
    private string SubheaderColor = "<color=#C9A963>";
    private string DescriptionColor = "<color=#74664B>";

    private string HeaderSize = "<size=15>";
    private string SubheaderSize = "<size=10>";
    private string DescriptionSize = "<size=7>";


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
            mouseOverBtn.Loc_Key = $"<b>{HeaderSize}{HeaderColor}{LocalizationManager.Instance.GetText(prov.NameKey)}{SubheaderSize}</b>{SubheaderColor}\n<b>NEW</b>\n{DescriptionSize}{DescriptionColor}{LocalizationManager.Instance.GetText(prov.DescriptionKey)}\n\n<i> {prov.Tooltips}";
        }
        else
        {
            ProvisionData nextLevel = GameDataManager.Instance.GetProvision(prov.Id, prov.Level + 1);
            if(nextLevel == null)
            {
                mouseOverBtn.Loc_Key = $"<b>{HeaderSize}{HeaderColor}{ LocalizationManager.Instance.GetText(prov.NameKey)}{SubheaderSize} LV.{prov.Level}</b>{SubheaderColor}\n<b>MAX LEVEL</b>\n{DescriptionSize}{DescriptionColor}{LocalizationManager.Instance.GetText(prov.DescriptionKey)}\n\n<i>LV.{prov.Level}\n{prov.Tooltips}";
            }
            else
            {
                mouseOverBtn.Loc_Key = $"<b>{HeaderSize}{HeaderColor}{ LocalizationManager.Instance.GetText(prov.NameKey)}{SubheaderSize} LV.{prov.Level}</b>{SubheaderColor}\n<b>UPGRADE</b>\n{DescriptionSize}{DescriptionColor}{LocalizationManager.Instance.GetText(nextLevel.DescriptionKey)}\n\n<i>LV.{prov.Level}\n{prov.Tooltips} \n{DescriptionColor}{DescriptionSize}--\nLV.{nextLevel.Level}\n{nextLevel.Tooltips}";
            }
        }
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{HeaderColor}", HeaderColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{SubheaderColor}", SubheaderColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{DescriptionColor}", DescriptionColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{HeaderSize}", HeaderSize);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{SubheaderSize}", SubheaderSize);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{DescriptionSize}", DescriptionSize);
    }

    public void OnClick()
    {
        if (Provision == null) return;
        if (Type == ProvisionUIItemType.NEW) SendMessageUpwards("AddNewProvision", Provision, SendMessageOptions.RequireReceiver);

        if (Type == ProvisionUIItemType.UPGRADE) SendMessageUpwards("UpgradeProvision", Provision, SendMessageOptions.RequireReceiver);

        ToolTipManager.Instance.ShowToolTip("");
    }

    #region XboxSupport

    private Vector3? _preHoverScale;


    /// <summary>
    /// Enlarge an action button to visibly show a user that it is the current action button that will be triggered by the controller
    /// </summary>
    public void HandleControllerHover()
    {
        GetComponent<TooltipMouseOver>().HandleControllerTooltip();
    }
    /// <summary>
    /// Reset an action button to visibly show a user that it is no longer the current action button that will be triggered by the controller
    /// </summary>
    public void EndControllerHover()
    {
        GetComponent<TooltipMouseOver>().EndControllerTooltip();
    }
    #endregion
}