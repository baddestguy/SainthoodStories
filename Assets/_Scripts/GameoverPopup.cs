using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameoverPopup : MonoBehaviour
{

    public void Restart()
    {
        MissionManager.Instance.RestartMission();
    }

}
