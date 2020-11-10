using UnityEngine;

public class TooltipMouseOver : MonoBehaviour
{
    public string Loc_Key;
    public string ButtonName;
    public string HouseName;
    private InteractableHouse House;

    public void ShowToolTip()
    {
        TooltipStats customToolStats = null;
        switch (HouseName) 
        {
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
                var customEvent = FindObjectOfType<CustomEventPopup>().EventData;
                if(customEvent.RewardType == CustomEventRewardType.FP)
                    customToolStats = new TooltipStats() { CP = 0, Energy = 0, FP = -(int)customEvent.RejectionCost, Ticks = 0 };
                else
                    customToolStats = new TooltipStats() { CP = -(int)customEvent.RejectionCost, Energy = 0, FP = 0, Ticks = 0 };
                break;
        }

        if(House != null)
        {
            ToolTipManager.Instance.ShowToolTip(Loc_Key, House.GetTooltipStatsForButton(ButtonName));
        }
        else if(customToolStats != null)
        {
            ToolTipManager.Instance.ShowToolTip(Loc_Key, customToolStats);
        }
        else
        {
            ToolTipManager.Instance.ShowToolTip(Loc_Key);
        }
    }

    public void HideToolTip()
    {
        ToolTipManager.Instance.ShowToolTip("");
    }
}
