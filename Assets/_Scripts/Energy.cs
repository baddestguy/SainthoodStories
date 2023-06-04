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
        SaveDataManager.Instance.SaveGame();
    }

    public bool Depleted()
    {
        return Amount == 0;
    }

    public bool CanUseEnergy(int consumption)
    {
        return (Amount - consumption) < 0;
    }

    public void OnOveride(int amount)
    {
        Amount = amount;
        EnergyConsumed?.Invoke(this);
    }
}
