using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    private void Awake()
    {
        Instance = this;
    }

    void Start()
    {
        SceneManager.LoadScene("TestGame", LoadSceneMode.Additive);
    }

    public void LoadGame()
    {

    }

    public void SaveGame()
    {

    }
}
