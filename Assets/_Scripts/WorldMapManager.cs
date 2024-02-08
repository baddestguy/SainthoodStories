using System.Collections;
using System.Collections.Generic;
using Enviro;
using UnityEngine;

public class WorldMapManager : MonoBehaviour
{
    public int EnviroMinutes;
    public int EnviroHours;
    // Start is called before the first frame update
    void Start()
    {
        var hour = (int)GameManager.Instance.GameClock.Time / 1;
        var mins = GameManager.Instance.GameClock.Time % 1 == 0 ? 0 : 30;
        EnviroManager.instance.Time.hours = hour;
        EnviroManager.instance.Time.minutes = mins;

        StartCoroutine(CheckTick());
    }

    IEnumerator CheckTick()
    {
        while (true)
        {
            yield return null;
            if (EnviroManager.instance.Time.minutes == 30 || EnviroManager.instance.Time.minutes == 0)
            {
                GameManager.Instance.GameClock.Tick();
                yield return new WaitForSeconds(1.5f);
            }
        }
    }
}
