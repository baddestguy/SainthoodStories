using UnityEngine;
using UnityEngine.Events;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    public static UnityAction<Mission> MissionBegin;

    public MissionManager MissionManager;
    [HideInInspector]
    public MapGenerator MapGenerator;
    [HideInInspector]
    public GameMap Map;
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
        SceneManager.LoadScene("Level1", LoadSceneMode.Single);
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        // MapGenerator.DisplayMap(0, OnMapGenerated);
        Player = FindObjectOfType<Player>();
        MissionManager.LoadAllMissions();
        GameClock = new GameClock(MissionManager.CurrentMission.StartingClock);
        Player.GameStart(MissionManager.CurrentMission);
        MissionBegin?.Invoke(MissionManager.CurrentMission);
    }

    private void OnMapGenerated()
    {
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
        //MapGenerator.LoadAllMaps();
    }

    public void SaveGame()
    {

    }
}
