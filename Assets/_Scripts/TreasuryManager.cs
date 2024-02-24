using System.Collections;
using UnityEngine;
using UnityEngine.Events;

public class TreasuryManager : MonoBehaviour
{
    public static TreasuryManager Instance { get; private set; }

    public double Money { get; set; }
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

    public bool CanAfford(double price)
    {
        return Money >= price;
    }

    public void DonateMoney(double donation)
    {
        if (donation == 0) return;

        TemporaryMoneyToDonate += donation;
        DonatedMoney?.Invoke(donation);
        SaveDataManager.Instance.SaveGame();
    }

    public void DepositMoney()
    {
        StartCoroutine(DepositMoneyAsync());
        Money += Mathf.Clamp(float.Parse(TemporaryMoneyToDonate + ""), 0, 100);
        TemporaryMoneyToDonate = 0;
        SaveDataManager.Instance.SaveGame();
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
        SaveDataManager.Instance.SaveGame();
    }

    private void OnDisable()
    {
        EventsManager.EventExecuted -= OnEventExecuted;
    }
}
