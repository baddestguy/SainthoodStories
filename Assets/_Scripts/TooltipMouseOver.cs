using DG.Tweening;
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

    public void ShowToolTip()
    {
        transform.DOComplete();
        transform.DOPunchScale(transform.localScale * 0.15f, 0.5f, elasticity: 0f);
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
                if(customEvent.RewardType == CustomEventRewardType.FP)
                    CustomToolStats = new TooltipStats() { CP = 0, Energy = 0, FP = -(int)customEvent.RejectionCost, Ticks = 0 };
                else
                    CustomToolStats = new TooltipStats() { CP = -(int)customEvent.RejectionCost, Energy = 0, FP = 0, Ticks = 0 };
                break;
        }

        if(House != null)
        {
            ToolTipManager.Instance.ShowToolTip(Loc_Key, House.GetTooltipStatsForButton(ButtonName));
        }
        else if(CustomToolStats != null)
        {
            ToolTipManager.Instance.ShowToolTip(Loc_Key, CustomToolStats);
        }
        else
        {
            ToolTipManager.Instance.ShowToolTip(Loc_Key);
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
    }
}
