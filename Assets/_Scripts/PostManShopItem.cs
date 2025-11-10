using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PostManShopItem : MonoBehaviour
{
    public PostManShopData Data;
    public Image Icon;
    public TextMeshProUGUI PriceDisplay;

    public void Init(PostManShopData data)
    {
        Data = data;
        Icon = Resources.Load<Image>(data.IconPath);
        PriceDisplay.text = data.Price.ToString();
    }

    public void Purchase()
    {
        SendMessageUpwards("PurchaseItem", this);
    }
}
