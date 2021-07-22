using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class OverideManager : MonoBehaviour
{

    public int DayOverride;
    public double TimeOverride;

    public int FP, CP;
    public int energy;
    public int FutureStartTime, FutureEndTime;
    public int Money;
    public ItemType Inventory;
    public Provision provisions;
    public WeatherType weatherType;
    public SaintID NewSaint;
    public bool UnlockedSaints;

    [SerializeField]
    public bool showDayTimeUI;
    [SerializeField]
    public bool showCpFpEnUI;
    [SerializeField]
    public bool showInvProUI;

    public void OverideTime()
    {
        TutorialManager.Instance.OnOveride();
        EventsManager.Instance.OnOveride();
        GameManager.Instance.GameClock.OnOveride(DayOverride, TimeOverride);
        TryOveride();
    }

    public void OverrideWeather()
    {
        WeatherManager.Instance.OnOverride(weatherType,FutureStartTime, FutureEndTime);
    }

    private void TryOveride()
    {
        CustomEventPopup cev = FindObjectOfType<CustomEventPopup>();
        if (cev != null)
            cev.OnOveride();
    }

    public void SkipEvent()
    {
        EventsManager.Instance.OnOveride();
        TryOveride();
    }

    public void Overide_CP_FP_ENG(bool _cp, bool _fp , bool _energy)
    {
        if(_cp)
            MissionManager.Instance.OverideCP(CP);
        if(_fp)
            MissionManager.Instance.OverideFP(FP);
        if(_energy)
            GameManager.Instance.Player.Energy.OnOveride(energy);
    }

    public void OverrideProvitionInventory(bool provision, bool inventory, bool adding)
    {
        if(inventory)
            InventoryManager.Instance.OnInventoryOverride(adding ,Inventory);
        if(provision)
            InventoryManager.Instance.OnProvisionsOverride(provisions);
    }

    public void OverideSaint()
    {
        SaintsManager.Instance.OnOverride(NewSaint);
    }

    public void AddMoney()
    {
        TreasuryManager.Instance.DonateMoney(Money);
    }
}
