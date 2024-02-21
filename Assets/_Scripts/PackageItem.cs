using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PackageItem : MonoBehaviour
{
    public ItemType Item;
    public Image PackageIcon;

    public ObjectivesData Data;

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
