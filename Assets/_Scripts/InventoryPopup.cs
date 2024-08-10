using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class InventoryPopup : MonoBehaviour
{
    public PackageItem[] ItemList;
    public ProvisionUIItem[] UpgradeProvisionUIItems;
    public ScrollRect Scroller;
    public HouseObjectiveUIItem[] Objs;
    public List<SacredItemUIItem> ArtifactObjs = new List<SacredItemUIItem>();

    public TextMeshProUGUI ArtifactTitle;
    public TextMeshProUGUI ArtifactDescription;
    public ScrollRect ArtifactScroller;
    public float ArtifactScrollerContentVSize;
    public GameObject ArtifactObj;

    public GameObject[] Tabs;
    private int TabIndex;

    public static bool Open;

    // Start is called before the first frame update
    void OnEnable()
    {
        Open = true;
        UI.Instance.EnableAllUIElements(false);
        Player.LockMovement = true;
        for (int i = 0; i < InventoryManager.Instance.Items.Count; i++)
        {
            ItemList[i].PackageIcon.gameObject.SetActive(true);
            ItemList[i].PackageIcon.sprite = Resources.Load<Sprite>($"Icons/{InventoryManager.Instance.Items[i]}");
        }

        for (int i = 0; i < InventoryManager.Instance.Provisions.Count; i++)
        {
            UpgradeProvisionUIItems[i].Init(InventoryManager.Instance.Provisions[i], ProvisionUIItemType.UPGRADE);
        }

        var houses = GameManager.Instance.Houses;
        
        //Sort to always place convent first
        for (int i = 0; i < houses.Length; i++)
        {
            if (houses[i].HouseName.Contains("Church"))
            {
                var temp = houses[0];
                houses[0] = houses[i];
                houses[i] = temp;
            }
        }

        for (int i = 0; i < houses.Length; i++)
        {
            if (houses[i].MyObjective == null) continue;

            Objs[i].gameObject.SetActive(true);
            Objs[i].Text.text = $"{LocalizationManager.Instance.GetText(houses[i].MyObjective.MissionDescription)}";
            Objs[i].Image.sprite = Resources.Load<Sprite>($"Icons/{houses[i].HouseName}");

            if (houses[i].MyObjective.Event == BuildingEventType.COOK || houses[i].MyObjective.Event == BuildingEventType.COOK_URGENT
                || houses[i].MyObjective.Event == BuildingEventType.DELIVER_ITEM 
                || houses[i].MyObjective.Event == BuildingEventType.DELIVER_ITEM_URGENT
                || houses[i].MyObjective.Event == BuildingEventType.DELIVER_MEAL
                || houses[i].MyObjective.Event == BuildingEventType.DELIVER_MEAL_URGENT)
            {
                Objs[i].Text.text = $"{LocalizationManager.Instance.GetText(houses[i].MyObjective.MissionDescription)}: {houses[i].MyObjective.RequiredAmount - houses[i].RequiredItems}/{houses[i].MyObjective.RequiredAmount}";
            }

            if (houses[i].MyObjective.Event == BuildingEventType.VOLUNTEER 
                || houses[i].MyObjective.Event == BuildingEventType.VOLUNTEER_URGENT
                || houses[i].MyObjective.Event == BuildingEventType.PRAY 
                || houses[i].MyObjective.Event == BuildingEventType.PRAY_URGENT)
            {
                Objs[i].Text.text = $"{LocalizationManager.Instance.GetText(houses[i].MyObjective.MissionDescription)}: {houses[i].VolunteerProgress}/{houses[i].MyObjective.RequiredAmount}";
            }
        }

        int counter = 0;
        var scrollerContentRect = ArtifactScroller.content.GetComponent<RectTransform>();
        ArtifactScrollerContentVSize = scrollerContentRect.sizeDelta.y;
        foreach (var col in InventoryManager.Instance.Collectibles)
        {
            var artifact = Instantiate(ArtifactObj);
            artifact.transform.SetParent(ArtifactObj.transform.parent);
            artifact.SetActive(true);
            artifact.GetComponent<SacredItemUIItem>().Init(col.Split(':')[1]);
            if (counter > 6) //Expand scroll view if items spawned go beyond the current single-page view
            {
                scrollerContentRect.sizeDelta = new Vector2(scrollerContentRect.sizeDelta.x, scrollerContentRect.sizeDelta.y + 41);
            }
            ArtifactScroller.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
            ArtifactObjs.Add(artifact.GetComponent<SacredItemUIItem>());
            counter++;
        }
    }

    public void NextTab()
    {
        if (TabIndex+1 >= Tabs.Length) return;

        Tabs[TabIndex].SetActive(false);
        TabIndex++;
        Tabs[TabIndex].SetActive(true);
    }

    public void PrevTab()
    {
        if (TabIndex-1 < 0) return;

        Tabs[TabIndex].SetActive(false);
        TabIndex--;
        Tabs[TabIndex].SetActive(true);
    }

    public void ShowArtifact(string itemName)
    {
        var item = GameDataManager.Instance.GetCollectibleData(itemName);
        ArtifactTitle.text = item.Name;
        ArtifactDescription.text = item.Description;
    }

    private void OnDisable()
    {
        Open = false;

        for (int i = 0; i < InventoryManager.Instance.Items.Count; i++)
        {
            ItemList[i].PackageIcon.gameObject.SetActive(false);
        }
        for (int i = 0; i < Objs.Length; i++)
        {
            Objs[i].gameObject.SetActive(false);
        }

        for (int i = 0; i < ArtifactObjs.Count; i++)
        {
            ArtifactObjs[i].Remove();
        }
        ArtifactObjs.Clear();

        var scrollerContentRect = ArtifactScroller.content.GetComponent<RectTransform>();
        scrollerContentRect.sizeDelta = new Vector2(scrollerContentRect.sizeDelta.x, ArtifactScrollerContentVSize);

        Player.LockMovement = false;
        UI.Instance.EnableAllUIElements(true);
    }
}
