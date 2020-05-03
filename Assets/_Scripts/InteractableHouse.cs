using UnityEngine;

public class InteractableHouse : InteractableObject
{
    private int CountdownTimer;
    public int DeadlineTime;
    public int EnergyConsumption;
    public int OpenTime;
    public int ClosingTime;

    protected virtual void Start()
    {
        UI.Meditate += Meditated;
    }

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
            UI.Instance.DisplayMessage($"{name}: TIME UP!");
        }
    }

    public override void MissionBegin(Mission mission)
    {
        Debug.Log($"House: {gameObject.name} DEADLINE: {DeadlineTime}:00");
        CountdownTimer = DeadlineTime - mission.StartingClock;
    }

    public virtual void Meditated(InteractableHouse house)
    {
        if (house != this) return;

        GameClock clock = GameManager.Instance.GameClock;
        Player player = GameManager.Instance.Player;

        player.ConsumeEnergy(-1);
        clock.Tick();
        UI.Instance.DisplayMessage("MEDITATED!!");
    }

    private void OnDisable()
    {
        UI.Meditate -= Meditated;
    }
}
