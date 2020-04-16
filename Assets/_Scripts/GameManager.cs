using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    public MissionManager MissionManager;
    public MapGenerator MapGenerator;
    public Player Player;
    public int GameTimer;

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
        Player.GameStart(MissionManager.CurrentMission);
    }

    private void OnTap(MapTile tile)
    {
        Player.OnInteract(tile);
    }

    private void OnPlayerMoved(Energy energy, MapTile tile)
    {
        GameTimer++; //Create class
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
