using UnityEngine;

public class StormFX : MonoBehaviour
{
    public ParticleSystem FX;
    public GameObject Trees;

    private void OnEnable()
    {
        WeatherManager.WeatherForecastActive += WeatherAlert;
    }

    private void WeatherAlert(WeatherType weather, GameClock start, GameClock end)
    {
        var renderers = Trees.GetComponentsInChildren<Renderer>();

        if (weather == WeatherType.RAIN || weather == WeatherType.SNOW)
        {
            foreach(Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
            FX.Play();
            foreach (var rend in renderers)
            {
                if (!rend.name.ToLower().Contains("tree")) continue;
                if (rend.materials.Length > 1)
                    rend.materials[1].SetFloat("_BendAmount", 0.75f);
                else
                    rend.materials[0].SetFloat("_BendAmount", 0.75f);
            }
        }
        else
        {
            foreach (Transform child in transform)
            {
                child.gameObject.SetActive(true);
            }
            FX.Stop();
            foreach (var rend in renderers)
            {
                if (!rend.name.ToLower().Contains("tree")) continue;
                if (rend.materials.Length > 1)
                    rend.materials[1].SetFloat("_BendAmount", 0);
                else
                    rend.materials[0].SetFloat("_BendAmount", 0);
            }
        }
    }

    private void OnDisable()
    {
        WeatherManager.WeatherForecastActive -= WeatherAlert;
    }
}
