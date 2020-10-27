using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance { get; private set; }
    
    public int CurrentTutorialStep = 0;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        EventsManager.EventDialogTriggered += FinishedTalking;
    }

    public List<MapTile> GetTutorialMapTiles()
    {
        switch (CurrentTutorialStep)
        {
            case 0: return new List<MapTile>() { TutorialMapTileGroups.Instance.ChurchMapTile };
            case 2: return TutorialMapTileGroups.Instance.Group1;
            case 6: return TutorialMapTileGroups.Instance.Group2;
            case 8: return TutorialMapTileGroups.Instance.Group3;
            case 10: return TutorialMapTileGroups.Instance.Group4;
            case 12: return TutorialMapTileGroups.Instance.Group5;
        }

        return new List<MapTile>();
    }

    public void RemoveTileFromGroup()
    {
        switch (CurrentTutorialStep)
        {
            case 2: 
                TutorialMapTileGroups.Instance.Group1.RemoveAt(0);
                break;
            case 6: 
                TutorialMapTileGroups.Instance.Group2.RemoveAt(0);
                break;
            case 8: 
                TutorialMapTileGroups.Instance.Group3.RemoveAt(0);
                break;
            case 10:
                TutorialMapTileGroups.Instance.Group4.RemoveAt(0);
                break;
            case 12:
                TutorialMapTileGroups.Instance.Group5.RemoveAt(0);
                break;
        }
    }

    public void NextTutorialStep()
    {
        CurrentTutorialStep++;
    }

    private void FinishedTalking(bool started)
    {
        if (!started)
        {
            if (GameSettings.Instance.FTUE && CurrentTutorialStep >= 20)
            {
                GameSettings.Instance.FTUE = false;
                SaveDataManager.Instance.SaveGame();
            }
        }
    }

    public bool CheckTutorialButton(string button)
    {
        switch (button)
        {
            case "PRAY": 
                return GameManager.Instance.Player.CurrentBuilding != null && GameManager.Instance.Player.CurrentBuilding.GetType() == typeof(InteractableChurch) && 
                    (CurrentTutorialStep < 2 || 
                    (CurrentTutorialStep >= 6 && CurrentTutorialStep <= 7) || 
                    (CurrentTutorialStep >= 12 && CurrentTutorialStep <= 13) ||
                    CurrentTutorialStep >= 18);
            case "SLEEP": 
                return CurrentTutorialStep >= 14 && CurrentTutorialStep <= 17;
            case "BABY":
                return false;
            case "VOLUNTEER":
            case "GROCERIES":
            case "CLOTHES":
            case "TOYS":
            case "STATIONERY":
                return false;
            case "MEDS":
                return GameManager.Instance.Player.CurrentBuilding != null && ((GameManager.Instance.Player.CurrentBuilding.GetType() == typeof(InteractableMarket) && CurrentTutorialStep >= 8 && CurrentTutorialStep <= 9) ||
                    (GameManager.Instance.Player.CurrentBuilding.GetType() == typeof(InteractableHospital) && CurrentTutorialStep >= 10 && CurrentTutorialStep <= 11));
        }

        return true;
    }
}
