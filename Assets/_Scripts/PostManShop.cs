using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Experimental.AssetDatabaseExperimental.AssetDatabaseCounters;

public class PostManShop : MonoBehaviour
{
    public ScrollRect Scroller;
    public float ScrollerContentVSize;
    public float ScrollRowSize;

    public GameObject ShopItemGo;
    public int ItemSpawnCount;
    public List<GameObject> InstantiatedGos;
    public List<PostManShopItem> InstantiatedItems;
    public TextMeshProUGUI PostManText;
    public bool VisitedToday;

    public void OnEnable()
    {
        StartCoroutine(InitAsync());
        UI.Instance.EnableAllUIElements(false);
    }

    IEnumerator InitAsync()
    {
        PostManText.text = LocalizationManager.Instance.GetText("POSTMAN_STORE_GREETING");


        var scrollerContentRect = Scroller.content.GetComponent<RectTransform>();
        ScrollerContentVSize = scrollerContentRect.sizeDelta.y;
        if (InstantiatedItems.Any())
        {
            for (int i = 0; i < InstantiatedItems.Count; i++)
            {
                var item = Instantiate(ShopItemGo);
                item.transform.SetParent(Scroller.content);
                var pItem = item.GetComponent<PostManShopItem>();
                item.transform.SetParent(Scroller.content);

                var itemType = Extensions.GetRandomEnumValue<CollectibleType>();
                var shopData = GameDataManager.Instance.PostManShopData[itemType];
                pItem.Init(InstantiatedItems[i].Data);
                InstantiatedGos.Add(item);

                if (i > 3) //Expand scroll view if items spawned go beyond the current single-page view
                {
                    scrollerContentRect.sizeDelta = new Vector2(scrollerContentRect.sizeDelta.x, scrollerContentRect.sizeDelta.y + ScrollRowSize);
                }
                Scroller.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;
             
                yield return new WaitForSeconds(0.1f);
            }
        }
        else if(!VisitedToday)
        {
            VisitedToday = true;
            //Before spawning, you need to check if player has collected all of that type
            for (int i = 0; i < ItemSpawnCount; i++)
            {
                var item = Instantiate(ShopItemGo);
                item.transform.SetParent(Scroller.content);
                var pItem = item.GetComponent<PostManShopItem>();
                item.transform.SetParent(Scroller.content);

                var itemType = Extensions.GetRandomEnumValue<CollectibleType>();
                var shopData = GameDataManager.Instance.PostManShopData[itemType];
                pItem.Init(shopData);
                InstantiatedGos.Add(item);
                InstantiatedItems.Add(pItem);

                if (i > 3) //Expand scroll view if items spawned go beyond the current single-page view
                {
                    scrollerContentRect.sizeDelta = new Vector2(scrollerContentRect.sizeDelta.x, scrollerContentRect.sizeDelta.y + ScrollRowSize);
                }
                Scroller.GetComponent<ScrollRect>().verticalNormalizedPosition = 1f;

                yield return new WaitForSeconds(0.1f);
            }
        }
    }

    public void UpdatePostManText(PostManShopItem item)
    {
        PostManText.text = LocalizationManager.Instance.GetText(item.Data.DescriptionKey);
    }

    public void PurchaseItem(PostManShopItem item)
    {
        if (!TreasuryManager.Instance.CanAffordPostMan(item.Data.Price))
        {
            SoundManager.Instance.PlayOneShotSfx("LowEnergy_SFX", timeToDie: 2f);
            return;
        }

        Debug.Log("Purchased: " + item.Data.Id);
        SoundManager.Instance.PlayOneShotSfx("Success_SFX", timeToDie: 3f);
        TreasuryManager.Instance.SpendSpirits(item.Data.Price);
        InstantiatedItems.Remove(item);
        InstantiatedGos.Remove(item.gameObject);
        Destroy(item.gameObject);
    }

    public void Close()
    {
        for (int i = 0; i < InstantiatedGos.Count; i++)
        {
            Destroy(InstantiatedGos[i]);
        }

        InstantiatedGos.Clear();
        gameObject.SetActive(false);
        UI.Instance.EnableAllUIElements(true);

        var scrollerContentRect = Scroller.content.GetComponent<RectTransform>();
        scrollerContentRect.sizeDelta = new Vector2(scrollerContentRect.sizeDelta.x, ScrollerContentVSize);
    }
}
