using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableChurch : InteractableHouse
{
    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock clock = GameManager.Instance.GameClock;
        if (tile.GetInstanceID() == GetInstanceID())
        {
            //TODO: Pop up button to choose between End the Day/Attend Service/Wait One Hour
            if (clock.Time >= OpenTime && clock.Time <= ClosingTime)
            {
                Debug.Log("ATTENDED SERVICE!");
                GameManager.Instance.Player.AddToInventory(new PlayerItem(ItemType.FOOD));
                energy.Consume(-RestEnergy);
            }
        } 
    }
}
