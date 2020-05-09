using UnityEngine;
using UnityEngine.Events;

public class Energy
{
    public static UnityAction<Energy> EnergyConsumed;

    public int Amount { get; private set; }

    public Energy(int amount)
    {
        Amount = amount;
    }

    public void Consume(int amount)
    {
        Amount = Mathf.Clamp((Amount - amount), 0, 100);
        EnergyConsumed?.Invoke(this);
    }

    public bool Depleted(int consumption = 0)
    {
        return (Amount-consumption) <= 0;
    }
}
