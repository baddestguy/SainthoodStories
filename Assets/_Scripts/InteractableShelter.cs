using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteractableShelter : InteractableHouse
{
    private bool Done;

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if (Done)
        {
            gameObject.SetActive(false);
        }

        if (tile.GetInstanceID() == GetInstanceID())
        {
            Player player = GameManager.Instance.Player;
            PlayerItem item = player.GetItem(ItemType.FOOD);

            if(item != null)
            {
                Debug.Log("FED THE HUNGRY!");
                Done = true;
            }
            else
            {
                Debug.Log("YOU HAVE NO FOOD TO GIVE!");
            }
        }
    }
}
