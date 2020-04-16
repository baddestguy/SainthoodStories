using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

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
        Player.OnMoveSuccessEvent += OnPlayerMoved;
        SceneManager.LoadScene("TestGame", LoadSceneMode.Single);
    }

    public void OnLevelLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        MapGenerator.DisplayMap(0, OnMapGenerated);
    }

    private void OnMapGenerated()
    {
        Player.GameStart(new Mission(1, 5, 1, MapGenerator.MapTiles[37]));
    }

    public void OnPlayerMoved(Energy energy)
    {
        GameTimer++;
        if (energy.Depleted())
        {
            Debug.LogError("GAME OVER!!");
        }
        //Debug.Log("GAMEMANAGER TRIGGERD MOVED!");
        //Trigger Status effects if any
        //Check if House reached, Check against mission requirements
    }

    public void Undo()
    {

    }

    public void LoadGame()
    {
        MapGenerator.LoadAllMaps();
    }

    public void SaveGame()
    {

    }
}
