using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class TooltipMouseOver : MonoBehaviour
{
    public string Loc_Key;
    public string ButtonName;
    public string HouseName;
    private InteractableHouse House;

    public static UnityAction OnHover;
    public static bool IsHovering;
    public TooltipStats CustomToolStats = null;

    public TextMeshProUGUI TimeStatsDisplay;
    public TextMeshProUGUI EnergyStatsDisplay;
    public TextMeshProUGUI FPCPStatsDisplay;
    public GameObject InfoPanel;

    public void ShowToolTip()
    {
        transform.DOComplete();
        transform.DOPunchScale(transform.localScale * 0.15f, 0.5f, elasticity: 0f);

        DoToolTip();
    }

    private void DoToolTip()
    {
        IsHovering = true;

        switch (HouseName)
        {
            case "House":
                House = transform.GetComponentInParent<InteractableHouse>();
                break;
            case "Church":
                House = FindObjectOfType<InteractableChurch>();
                break;
            case "Hospital":
                House = FindObjectOfType<InteractableHospital>();
                break;
            case "Orphanage":
                House = FindObjectOfType<InteractableOrphanage>();
                break;
            case "School":
                House = FindObjectOfType<InteractableSchool>();
                break;
            case "Kitchen":
                House = FindObjectOfType<InteractableKitchen>();
                break;
            case "Shelter":
                House = FindObjectOfType<InteractableShelter>();
                break;
            case "Clothes":
                House = FindObjectOfType<InteractableClothesBank>();
                break;
            case "Market":
                House = FindObjectOfType<InteractableMarket>();
                break;
            case "RejectEvent":
                if (GameManager.Instance.Player.CurrentBuilding.BuildingState == BuildingState.HAZARDOUS)
                {
                    Loc_Key = "Building will be destroyed";
                }
                var customEvent = FindObjectOfType<CustomEventPopup>().EventData;
                if (customEvent.RewardType == CustomEventRewardType.FP)
                    CustomToolStats = new TooltipStats() { CP = 0, Energy = 0, FP = -(int)customEvent.RejectionCost, Ticks = 0 };
                else
                    CustomToolStats = new TooltipStats() { CP = -(int)customEvent.RejectionCost, Energy = 0, FP = 0, Ticks = 0 };
                break;
        }

        if (House != null)
        {
            ShowInfoPanel(House.GetTooltipStatsForButton(ButtonName));
            //   ToolTipManager.Instance.ShowToolTip(Loc_Key, House.GetTooltipStatsForButton(ButtonName));
        }
        else if (CustomToolStats != null)
        {
            //ShowInfoPanel(CustomToolStats);
            ToolTipManager.Instance.ShowToolTip(Loc_Key, CustomToolStats);
        }
        else
        {
            TextMeshProUGUI provisionDescription = GameObject.Find("ProvisionDescription")?.GetComponent<TextMeshProUGUI>();
            if (provisionDescription != null)
            {
                provisionDescription.text = LocalizationManager.Instance.GetText(Loc_Key);
            }
            else
            {
                TextMeshProUGUI textHover = gameObject.GetComponentInChildren<TextMeshProUGUI>();

                if (textHover != null)
                {
                    
                    var toolTipMouseOvers = UI.Instance.CustomEventPopup?.GetComponentsInChildren<TooltipMouseOver>(false)?.Where(x => x.HasControllerHover)?.ToArray();
                    if(toolTipMouseOvers != null)
                    {
                        foreach (var tooltipMouseOver in toolTipMouseOvers)
                        {
                            tooltipMouseOver.HandleControllerExit();
                        }
                    }

                    textHover.fontStyle = FontStyles.Bold | FontStyles.Underline;
                }
                ShowInfoPanel(CustomToolStats);
                ToolTipManager.Instance.ShowToolTip(Loc_Key);
            }
        }

        GamepadCursor.CursorSpeed = 500f;
    }

    public void HideToolTip()
    {
        TextMeshProUGUI textHover = gameObject.GetComponentInChildren<TextMeshProUGUI>();

        if (textHover != null)
        {
            textHover.fontStyle = FontStyles.Normal;
        }
        ToolTipManager.Instance.ShowToolTip("");
        IsHovering = false;
        transform.DOComplete();
        EventSystem.current.SetSelectedGameObject(null);
        GamepadCursor.CursorSpeed = 2000f;
        HideInfoPanel();
    }

    public void ShowInfoPanel(TooltipStats stats)
    {
        if (InfoPanel == null) return;

        InfoPanel.SetActive(true);

        if (stats == null) return;

        if(stats.Ticks / 4 == 1)
        {
            TimeStatsDisplay.text = $"+{stats.Ticks / 4}hr";
        }
        else
        {
            TimeStatsDisplay.text = $"+{stats.Ticks / 4}hrs";
        }

        EnergyStatsDisplay.text = stats.Energy >= 0 ? $"<color=#74664B>+{stats.Energy}" : $" <color=\"red\">{stats.Energy}";

        if (stats.FP != 0)
        {
            FPCPStatsDisplay.text = stats.FP >= 0 ? $"<color=#74664B>+{stats.FP}" : $" <color=\"red\">{stats.FP}";
        }
        else if (stats.CP != 0)
        {
            FPCPStatsDisplay.text = stats.CP >= 0 ? $"<color=#74664B>+{stats.CP}" : $" <color=\"red\">{stats.CP}";
        }
        else
        {
            FPCPStatsDisplay.text = $"<color=#74664B>+0";
        }

        if (stats.Spirits != 0)
        {
            TimeStatsDisplay.text = $"{stats.Spirits}";
        }
        if(stats.Coin != 0)
        {
            EnergyStatsDisplay.text = stats.Coin >= 0 ? $"<color=#74664B>+{stats.Coin}" : $" <color=\"red\">{stats.Coin}";
        }
        if (stats.RP != 0) 
        {
            FPCPStatsDisplay.text = stats.RP >= 0 ? $"<color=#74664B>+{stats.RP}" : $" <color=\"red\">{stats.RP}";
        }
    }

    public void HideInfoPanel()
    {
        if (InfoPanel == null) return;

        InfoPanel.SetActive(false);
    }


    #region XboxSupport

    private const float ScaleValue = 1.25f;
    public bool HasControllerHover;

    public void HandleControllerHover()
    {
        if (HasControllerHover) return;

        transform.DOComplete();
        transform.DOScale(transform.localScale * ScaleValue, 0.5f);

        DoToolTip();
        HasControllerHover = true;
    }

    public void HandleControllerExit()
    {
        if (!HasControllerHover) return;

        transform.DOComplete();
        transform.DOScale(transform.localScale / ScaleValue, 0.5f);
        HideToolTip();
        HasControllerHover = false;
    }

    #endregion
}
