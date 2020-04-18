using UnityEngine;

public class InteractableHouse : InteractableObject
{
    private int CountdownTimer;
    private int DeadlineTime;

    public void Init(int deadline, MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        DeadlineTime = deadline;        
        Init(groundTile, tileData, sprites, sortingOrder);
        Debug.Log("DEADLINE: " + deadline);
    }

    public override void Tick(int time)
    {
        CountdownTimer--;
        if (CountdownTimer <= 0 || time >= DeadlineTime)
        {
            Debug.LogError("TIME UP!");
            gameObject.SetActive(false);
        }
    }

    public override void MissionBegin(Mission mission)
    {
        CountdownTimer = DeadlineTime - mission.StartingClock;
    }

}
