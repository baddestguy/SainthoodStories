using System;
using TMPro;
using UnityEngine;

public class MarketPopUI : PopUI
{
    public TextMeshProUGUI GroceriesDisplayPrice;
    public TextMeshProUGUI ClothesDisplayPrice;
    public TextMeshProUGUI ToysDisplayPrice;
    public TextMeshProUGUI StationeryDisplayPrice;
    public TextMeshProUGUI MedsDisplayPrice;

    public override void Init(Action<string> callback, string sprite, int items, GameClock deadline, InteractableHouse house, CameraControls cameraControls = null)
    {
        Vector3 BuildingIconPos = BuildingIcon.transform.localPosition;
        base.Init(callback, sprite, items, deadline, house, cameraControls);
        BuildingIcon.transform.localPosition = BuildingIconPos;

        SetDisplayPrice(ItemType.DRUGS, GroceriesDisplayPrice);
        SetDisplayPrice(ItemType.CLOTHES, ClothesDisplayPrice);
        SetDisplayPrice(ItemType.TOYS, ToysDisplayPrice);
        SetDisplayPrice(ItemType.ENERGY_BOOST, StationeryDisplayPrice);
        SetDisplayPrice(ItemType.MEDS, MedsDisplayPrice);

        //Tried to hide buttons only if the building has not yet been constructed!
        foreach(var b in Buttons)
        {
            if (!b.Enabled)
            {
                //b.RefreshButton(house?.CanDoAction(b.ButtonName) ?? false);

                //b.gameObject.SetActive(false);
            }
        }
    }

    private void SetDisplayPrice(ItemType type, TextMeshProUGUI display)
    {
        var originalPrice = GameDataManager.Instance.ShopItemData[type].Price;
        var moddedPrice = InteractableMarket.ApplyDiscount(originalPrice);
        display.text = moddedPrice.ToString();

        if (!TreasuryManager.Instance.CanAfford(moddedPrice)) display.color = Color.red;
        else if (moddedPrice < originalPrice) display.color = Color.green;
        else if (moddedPrice > originalPrice) display.color = Color.magenta;
        else if (moddedPrice == originalPrice) display.color = Color.white;
    }
}
