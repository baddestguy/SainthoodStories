using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackageItem : MonoBehaviour
{
    public ItemType Item;
    public Image PackageIcon;

    public ObjectivesData Data;

    private string HeaderColor = "<color=#3B2E1F>";
    private string SubheaderColor = "<color=#C9A963>";
    private string DescriptionColor = "<color=#74664B>";

    private string HeaderSize = "<size=15>";
    private string SubheaderSize = "<size=10>";
    private string DescriptionSize = "<size=7>";

    // Start is called before the first frame update
    void Start()
    {
        
    }

    public void Init(ObjectivesData data)
    {
        Data = data;
        switch (data.House)
        {
            case "InteractableHospital":
                Item = ItemType.MEDS;
                PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{ItemType.MEDS}");
                break;
            case "InteractableOrphanage":
                Item = ItemType.TOYS;
                PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{ItemType.TOYS}");
                break;
            case "InteractableKitchen":
                Item = ItemType.MEAL;
                PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{ItemType.MEAL}");
                break;
            case "InteractableShelter":
                Item = ItemType.GROCERIES;
                PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{ItemType.GROCERIES}");
                break;
            case "InteractableSchool":
                Item = ItemType.STATIONERY;
                PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{ItemType.STATIONERY}");
                break;
        }


        TooltipMouseOver mouseOverBtn = GetComponentInChildren<TooltipMouseOver>();

        mouseOverBtn.Loc_Key = $"InventoryTooltip_{Item}";

        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{HeaderColor}", HeaderColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{SubheaderColor}", SubheaderColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{DescriptionColor}", DescriptionColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{HeaderSize}", HeaderSize);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{SubheaderSize}", SubheaderSize);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{DescriptionSize}", DescriptionSize);

    }


    public void Select()
    {
        SendMessageUpwards("PackageSelected", this);
    }

    public void Deselect()
    {
        SendMessageUpwards("PackageDeselected", this);
    }
}
