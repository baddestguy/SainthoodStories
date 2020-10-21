using UnityEngine;
using UnityEngine.Events;

public class TreasuryManager : MonoBehaviour
{
    public static TreasuryManager Instance { get; private set; }

    public double Money { get; private set; }
    public static UnityAction<double> DonatedMoney;

    private void Awake()
    {
        Instance = this;
    }

    public bool CanAfford(double price)
    {
        return Money >= price;
    }

    public void DonateMoney(double donation)
    {
        Money += donation;
        DonatedMoney?.Invoke(donation);
    }

    public void SpendMoney(double amount)
    {
        Money -= amount;
        DonatedMoney?.Invoke(-amount);
    }
}
