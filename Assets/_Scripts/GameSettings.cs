using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public bool StoryToggle;
    public bool CustomEventsToggle;
    public bool ProvisionsToggle;
    public bool FTUE; //First Time User Experience!
    public bool TutorialToggle;

    private void Awake()
    {
        Instance = this;
    }

    private void Start()
    {
    }
}