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

        TemporaryMoneyToDonate = donation;
        Money += Mathf.Clamp(float.Parse(donation+""), 0, 100);
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
