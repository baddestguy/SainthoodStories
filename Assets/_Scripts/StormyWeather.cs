using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class StormyWeather : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public float StartSizeMin;
    public float StartSizeMax;

    private bool Running;

    public void StartStorm()
    {
        Running = true;
        StartCoroutine(RunStormAsync());
    }

    public void StopStorm()
    {
        Running = false;
    }

    private IEnumerator RunStormAsync()
    {
        var pMain = ParticleSystem.main;
        var minMax = new MinMaxCurve();

        while (Running)
        {
            minMax.constantMin = Mathf.Lerp(minMax.constantMin, StartSizeMin, Time.deltaTime);
            minMax.constantMax = Mathf.Lerp(minMax.constantMax, StartSizeMax, Time.deltaTime);
            pMain.startSize = minMax;
            yield return null;
        }

        while (Mathf.Abs(minMax.constantMax - 0) > 0.01f)
        {
            minMax.constantMin = Mathf.Lerp(minMax.constantMin, 0, Time.deltaTime);
            minMax.constantMax = Mathf.Lerp(minMax.constantMax, 0, Time.deltaTime);
            pMain.startSize = minMax;
            yield return null;
        }

        gameObject.SetActive(false);
    }
}
