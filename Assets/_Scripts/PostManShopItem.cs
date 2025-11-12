using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class PostManShopItem : MonoBehaviour, IPointerEnterHandler
{
    public PostManShopData ShopData;
    public Image Icon;
    public TextMeshProUGUI PriceDisplay;

    public void Init(PostManShopData data)
    {
        ShopData = data;
        Icon.sprite = Resources.Load<Sprite>(data.IconPath);
        PriceDisplay.text = data.Price.ToString();
    }

    public void Purchase()
    {
        SendMessageUpwards("PurchaseItem", this);
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SendMessageUpwards("UpdatePostManText", this);
    }
}
