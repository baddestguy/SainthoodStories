using UnityEngine;

public class InteractableMarket : InteractableHouse
{
    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            if (clock.Time >= OpenTime && clock.Time <= ClosingTime)
            {
                Debug.LogWarning("PICKED UP FOOD!");
                GameManager.Instance.Player.AddToInventory(new PlayerItem(ItemType.FOOD));
            }
            else
            {
                Debug.LogWarning("SHOP CLOSED!");
            }
        }
    }
}
