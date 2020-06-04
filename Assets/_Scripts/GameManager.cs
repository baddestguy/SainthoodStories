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
    public static MissionDifficulty MissionDifficulty;

    public Mission CurrentMission;
    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SceneManager.sceneLoaded += OnLevelLoaded;
        MapTile.OnClickEvent += OnTap;
        Player.OnMoveSuccessEvent += OnPlayerMoved;
        SceneManager.LoadScene("MainMenu", LoadSceneMode.Single);
    }

    private void OnLevelLoaded(Scene scene, LoadSceneMode loadSceneMode)
    {
        if(scene.name.Contains("Level"))
        {
            Instantiate(Resources.Load("UI/UI") as GameObject);
            Player = FindObjectOfType<Player>();
            MissionManager.LoadAllMissions(CurrentMission);
            GameClock = new GameClock(MissionManager.CurrentMission.StartingClock);
            Player.GameStart(MissionManager.CurrentMission);
            MissionBegin?.Invoke(MissionManager.CurrentMission);
            UI.Instance.InitTimeEnergy(GameClock, MissionManager.CurrentMission.StartingEnergy);
        }
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
        //Trigger Status effects if any
    }


    public void SetMissionParameters(MissionDifficulty missionDifficulty)
    {
        switch (missionDifficulty)
        {
            case MissionDifficulty.EASY: 
                CurrentMission = new Mission(75, 75, 30, 6, 1); 
                SceneManager.LoadScene("NormalLevel", LoadSceneMode.Single);
                break;
            case MissionDifficulty.NORMAL: 
                CurrentMission = new Mission(50, 50, 20, 0, Random.Range(3,6)); 
                SceneManager.LoadScene("NormalLevel", LoadSceneMode.Single);
                break;
            case MissionDifficulty.HARD: 
                CurrentMission = new Mission(25, 25, 15, 0, Random.Range(7, 11));
                SceneManager.LoadScene("NormalLevel", LoadSceneMode.Single);
                break;
        }

        MissionDifficulty = missionDifficulty;
    }

    public void SaveGame()
    {

    }
}
