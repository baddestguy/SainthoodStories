using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSettings : MonoBehaviour
{
    public static GameSettings Instance { get; private set; }

    public bool StoryMode;

    private void Awake()
    {
        Instance = this;
    }
}
