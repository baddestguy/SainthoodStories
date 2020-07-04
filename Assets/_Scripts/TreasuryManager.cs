using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TreasuryManager : MonoBehaviour
{
    public static TreasuryManager Instance { get; private set; }

    public double Money { get; private set; }

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
    }

    public void SpendMoney(double amount)
    {
        Money -= amount;
    }
}
