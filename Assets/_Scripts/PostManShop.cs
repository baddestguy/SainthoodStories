using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostManShop : MonoBehaviour
{
    public ScrollRect Scroller;
    public float ScrollerContentVSize;
    public float ScrollRowSize;

    public GameObject ShopItemGo;
    public int ItemSpawnCount;
    public List<GameObject> InstantiatedGos;
    public List<PostManShopData> InstantiatedItems;
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

                pItem.Init(InstantiatedItems[i]);
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
                var itemType = Extensions.GetRandomEnumValue<CollectibleType>();

                //Todo: need to consider that the case where there could be one collectible left, this will always pass, which means we should remove items from the total list once the store picks it.
                switch (itemType)
                {
                    case CollectibleType.WORLD_TRIVIA:
                        if (GameDataManager.Instance.GetRandomWorldTrivia() == null) continue;
                        break;
                    case CollectibleType.SAINT_FRAGMENT:
                        if (GameDataManager.Instance.GetRandomSaintFragment() == null) continue;
                        break;
                    case CollectibleType.SAINT_WRITING:
                        if (GameDataManager.Instance.GetRandomSaintWriting() == null) continue;
                        break;
                    case CollectibleType.LETTER:
                        if (GameDataManager.Instance.GetRandomLetter() == null) continue;
                        break;
                }

                var item = Instantiate(ShopItemGo);
                item.transform.SetParent(Scroller.content);
                var pItem = item.GetComponent<PostManShopItem>();
                item.transform.SetParent(Scroller.content);

                var shopData = GameDataManager.Instance.PostManShopData[itemType];
                pItem.Init(shopData);
                InstantiatedGos.Add(item);
                InstantiatedItems.Add(shopData);

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
        //text animation
        PostManText.text = LocalizationManager.Instance.GetText(item.ShopData.DescriptionKey);
    }

    public void PurchaseItem(PostManShopItem item)
    {
        if (!TreasuryManager.Instance.CanAffordPostMan(item.ShopData.Price))
        {
            SoundManager.Instance.PlayOneShotSfx("LowEnergy_SFX", timeToDie: 2f);
            return;
        }

        Debug.Log("Purchased: " + item.ShopData.Id);
        SoundManager.Instance.PlayOneShotSfx("Success_SFX", timeToDie: 3f);
        TreasuryManager.Instance.SpendSpirits(item.ShopData.Price);

        InventoryManager.Instance.AddCollectibleType(item.ShopData.Id);

        InstantiatedItems.Remove(item.ShopData);
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
