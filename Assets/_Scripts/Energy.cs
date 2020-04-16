using UnityEngine;
using UnityEngine.Events;

public class Energy
{
    public static UnityAction<int> EnergyConsumed;
    private int Amount;

    public Energy(int amount)
    {
        Amount = amount;
    }

    public void Consume(int amount)
    {
        Amount = Mathf.Max(0, (Amount - amount));
        EnergyConsumed?.Invoke(Amount);
    }

    public bool Depleted()
    {
        return Amount == 0;
    }
}
