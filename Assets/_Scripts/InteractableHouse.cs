using UnityEngine;

public class InteractableHouse : InteractableObject
{
    private int CountdownTimer;
    public int DeadlineTime;
    public int RestEnergy;
    public int OpenTime;
    public int ClosingTime;

    public void Init(int deadline, MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        DeadlineTime = deadline;        
        Init(groundTile, tileData, sprites, sortingOrder);
    }

    public override void Tick(int time, int day)
    {
        CountdownTimer--;
        if ((DeadlineTime != -1) && (CountdownTimer <= 0 || time >= DeadlineTime))
        {
            Debug.LogError("TIME UP!");
            MissionManager.MissionComplete?.Invoke(false);
            gameObject.SetActive(false);
        }
    }

    public override void MissionBegin(Mission mission)
    {
        Debug.Log($"House: {gameObject.name} DEADLINE: {DeadlineTime}:00");
        CountdownTimer = DeadlineTime - mission.StartingClock;
    }

}
