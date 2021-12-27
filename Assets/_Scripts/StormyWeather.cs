using System.Collections;
using UnityEngine;
using static UnityEngine.ParticleSystem;

public class StormyWeather : MonoBehaviour
{
    public ParticleSystem ParticleSystem;
    public float StartSizeMin;
    public float StartSizeMax;

    public static bool Running;

    public void StartStorm()
    {
        Running = true;
        StartCoroutine(RunStormAsync());
        StartCoroutine(LightningThunder());
        SoundManager.Instance.FadeAmbience(0.1f);
    }

    public void StopStorm()
    {
        Running = false;
        SoundManager.Instance.FadeAmbience(0.3f);
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

        Destroy(gameObject);
    }

    private IEnumerator LightningThunder()
    {
        while (Running)
        {
            SoundManager.Instance.PlayOneShotSfx("Thunder_SFX", 1f, 30);
            yield return new WaitForSeconds(30f);
        }
    }
}
