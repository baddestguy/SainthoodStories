using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class PackageItem : MonoBehaviour
{
    public ItemType Item;
    public Image PackageIcon;

    public HouseObjectivesData Data;

    private string HeaderColor = "<color=#3B2E1F>";
    private string SubheaderColor = "<color=#C9A963>";
    private string DescriptionColor = "<color=#74664B>";

    private string HeaderSize = "<size=15>";
    private string SubheaderSize = "<size=10>";
    private string DescriptionSize = "<size=7>";

    public bool PackageSelectorIsNew = false;

    // Start is called before the first frame update
    void Start()
    {

    }

    public void Init(HouseObjectivesData data)
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
                Item = ItemType.KITCHEN_INGREDIENTS;
                PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{ItemType.KITCHEN_INGREDIENTS}");
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

        SetLocalizedText(Item);
    }

    public void SetLocalizedText(ItemType item)
    {
        Debug.Log($"ELTEE: {nameof(SetLocalizedText)}");
        var houseName = ShopItemData.HouseNameForItemType(item); 
        Debug.Log($"ELTEE: {nameof(SetLocalizedText)} - House Name is  {houseName}"); //InteractableShelter
        Debug.Log($"ELTEE: {nameof(SetLocalizedText)} - Search the following house names: {string.Join("||", GameManager.Instance.Houses.Select(x => x.HouseName))}");
        //InteractableOrphanage||InteractableClothesBank||InteractableChurch||InteractableKitchen||InteractableHospital||InteractableShelter||InteractableSchool||InteractableMarket

        var house = GameManager.Instance.Houses.Where(h => h.HouseName == houseName).First();
        Debug.Log($"ELTEE: {nameof(SetLocalizedText)} - Found House Name is  {house.HouseName} "); //InteractableShelter

        TooltipMouseOver mouseOverBtn = GetComponentInChildren<TooltipMouseOver>();
        if (mouseOverBtn == null)
        {
            Debug.LogError("TooltipMouseOver not found");
        }

        if(house.MyObjective == null)
        {
            Debug.LogError("House Objective not found"); //BINGO!!!!!
        }

        Debug.Log($"ELTEE: {nameof(SetLocalizedText)} -Mission description is  {house.MyObjective.MissionDescription}");

        mouseOverBtn.Loc_Key = house.MyObjective.MissionDescription;


        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{HeaderColor}", HeaderColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{SubheaderColor}", SubheaderColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{DescriptionColor}", DescriptionColor);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{HeaderSize}", HeaderSize);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{SubheaderSize}", SubheaderSize);
        mouseOverBtn.Loc_Key = mouseOverBtn.Loc_Key.Replace("{DescriptionSize}", DescriptionSize);
    }

    public void Select()
    {
        if (InventoryPopup.Open) return;
        PackageSelectorIsNew = true;
        SendMessageUpwards("PackageSelected", this);
    }

    public void Deselect()
    {
        if (InventoryPopup.Open) return;
        PackageSelectorIsNew = false;
        SendMessageUpwards("PackageDeselected", this);
    }
}
