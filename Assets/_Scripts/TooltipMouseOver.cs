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
                ShowInfoPanel(CustomToolStats);
                ToolTipManager.Instance.ShowToolTip(Loc_Key);
            }
        }

        GamepadCursor.CursorSpeed = 500f;
    }

    public void HideToolTip()
    {
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

        TimeStatsDisplay.text = stats.Energy >= 0 ? $"+{stats.Ticks / 2}hrs" : $"{stats.Ticks}";
        EnergyStatsDisplay.text = stats.Energy >= 0 ? $"<color=#74664B>+{stats.Energy}" : $" <color=\"red\">{stats.Energy}";

        if (stats.FP != 0)
        {
            FPCPStatsDisplay.text = stats.FP >= 0 ? $"<color=#74664B>+{stats.FP}" : $" <color=\"red\">{stats.FP}";
        }
        else if (stats.CP != 0)
        {
            FPCPStatsDisplay.text = stats.CP >= 0 ? $"<color=#74664B>+{stats.CP}" : $" <color=\"red\">{stats.CP}";
        }
    }

    public void HideInfoPanel()
    {
        if (InfoPanel == null) return;

        InfoPanel.SetActive(false);
    }


    #region XboxSupport


    private Vector3? _preHoverScale;
    public void HandleControllerTooltip()
    {
        transform.DOComplete();
        _preHoverScale ??= transform.localScale;
        transform.DOScale(_preHoverScale!.Value * 1.25f, 0.5f);

        DoToolTip();
    }

    public void EndControllerTooltip()
    {
        transform.DOComplete();
        transform.DOScale(_preHoverScale!.Value, 0.5f);
        HideToolTip();
    }

    #endregion
}
