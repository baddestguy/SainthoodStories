using UnityEngine;

public class UI : MonoBehaviour
{
    void Start()
    {
        Player.OnMoveSuccessEvent += OnPlayerMoved;
        Energy.EnergyConsumed += OnEnergyConsumed;
        MissionManager.MissionComplete += MissionComplete;
        GameClock.Ticked += OnTick;
    }

    private void OnPlayerMoved(Energy energy, MapTile tile)
    {

    }

    private void OnEnergyConsumed(Energy energy)
    {
        Debug.Log("Current Energy: " + energy.Amount);
    }

    private void MissionComplete(bool complete)
    {
        if (complete)
        {
            Debug.Log("Mission Copmlete!!");
        }
        else
        {
            Debug.Log("Mission Failed!");
        }
    }

    private void OnTick(int time)
    {
        Debug.Log($"TIME: {time}:00");
    }
}
