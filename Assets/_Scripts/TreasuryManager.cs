using System.Collections;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.Assertions.Must;
using UnityEngine.Events;

public class TreasuryManager : MonoBehaviour
{
    public static TreasuryManager Instance { get; private set; }

    public double Money { get; set; }
    public int WanderingSpirits { get; set; }
    public double TemporaryMoneyToDonate;
    public static UnityAction<double> DonatedMoney;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
        EventsManager.EventExecuted += OnEventExecuted;
    }

    private void OnEventExecuted(CustomEventData e)
    {
        StartCoroutine(OnEventExecutedAsync(e));
    }

    private IEnumerator OnEventExecutedAsync(CustomEventData e) 
    {
        while (EventsManager.Instance.EventInProgress) yield return null;

        if (e.Id == CustomEventType.VANDALISM)
        {
            DonateMoney(e.Cost);
        }
    }

    public void Load(SaveObject data)
    {
        Money = data.Money;
        WanderingSpirits = data.WanderingSpirits;
    }

    public void AddWanderers(int amount)
    {
        WanderingSpirits += amount;
        UI.Instance.RefreshWanderingSpiritsBalance(amount);
    }

    public bool CanAfford(double price)
    {
        return Money >= price;
    }

    public bool CanAffordPostMan(int price)
    {
        return WanderingSpirits >= price;
    }

    public void SpendSpirits(int amount)
    {
        WanderingSpirits -= amount;
        UI.Instance.RefreshWanderingSpiritsBalance(-amount);
    }

    public void DonateMoney(double donation)
    {
        if (donation == 0) return;

        TemporaryMoneyToDonate += donation;
        DonatedMoney?.Invoke(donation);
    }

    public void DepositMoney()
    {
        StartCoroutine(DepositMoneyAsync());
        Money += Mathf.Clamp(float.Parse(TemporaryMoneyToDonate + ""), 0, 100);
        TemporaryMoneyToDonate = 0;
    }

    public void LoseTempMoney()
    {
        TemporaryMoneyToDonate = 0;
    }

    IEnumerator DepositMoneyAsync()
    {
        var donation = TemporaryMoneyToDonate;
        yield return new WaitForSeconds(3f);
        DonatedMoney?.Invoke(donation);
    }

    public void SpendMoney(double amount)
    {
        Money -= amount;
        DonatedMoney?.Invoke(-amount);
    }

    private void OnDisable()
    {
        EventsManager.EventExecuted -= OnEventExecuted;
    }
}
