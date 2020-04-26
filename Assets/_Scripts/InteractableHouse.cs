using UnityEngine;

public class InteractableHouse : InteractableObject
{
    private int CountdownTimer;
    public int DeadlineTime;
    public int RestEnergy;

    public void Init(int deadline, MapTile groundTile, TileData tileData, Sprite[] sprites, int sortingOrder = 0)
    {
        DeadlineTime = deadline;        
        Init(groundTile, tileData, sprites, sortingOrder);
    }

    public override void Tick(int time)
    {
        CountdownTimer--;
        if (CountdownTimer <= 0 || time >= DeadlineTime)
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

    public override void OnPlayerMoved(Energy energy, MapTile tile)
    {
        if(tile.GetInstanceID() == GetInstanceID())
        {
            energy.Consume(-RestEnergy);
            GameManager.Instance.GameClock.Tick();
            GameManager.Instance.GameClock.Tick();
            GameManager.Instance.GameClock.Tick();
            tile.gameObject.SetActive(false); //TODO: TEMPORARY!
        }
    }
}
