using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static UnityAction<Mission> MissionBegin;

    public MissionManager MissionManager;
    public MapGenerator MapGenerator;
    public Player Player;
    public GameClock GameClock;

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        LoadGame();
        SceneManager.sceneLoaded += OnLevelLoaded;
        MapTile.OnClickEvent += OnTap;
        Player.OnMoveSuccessEvent += OnPlayerMoved;
        SceneManager.LoadScene("TestGame", LoadSceneMode.Single);
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        MapGenerator.DisplayMap(0, OnMapGenerated);
    }

    private void OnMapGenerated()
    {
        MissionManager.LoadAllMissions();
        GameClock = new GameClock(MissionManager.CurrentMission.StartingClock);
        MissionBegin?.Invoke(MissionManager.CurrentMission);
    }

    private void OnTap(MapTile tile)
    {
        Player.OnInteract(tile);
    }

    private void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameClock.Tick();
        MissionManager.MissionUpdate(tile);
        //Trigger Status effects if any
    }


    public void LoadGame()
    {
        MapGenerator.LoadAllMaps();
    }

    public void SaveGame()
    {

    }
}
