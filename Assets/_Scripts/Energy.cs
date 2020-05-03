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
        Amount = Mathf.Clamp((Amount - amount), 0, 50);
        EnergyConsumed?.Invoke(this);
    }

    public bool Depleted()
    {
        return Amount == 0;
    }
}
