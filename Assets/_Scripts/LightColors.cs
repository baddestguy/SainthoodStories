using UnityEngine;

public class LightColors : MonoBehaviour
{
    public Light Light;
    public ParticleSystem Particles;
    public float Alpha = 0;

    void OnEnable()
    {
        GameClock.Ticked += OnTick;
    }

    private void OnTick(double time, int day)
    {
        if(time >= 21 || time < 6)
        {
            if(Light)
                Light.color = Color.cyan;

            if (Particles)
            {
                var main = Particles.main;
                main.startColor = Color.cyan;
                if (Alpha > 0)
                {
                    Color c = main.startColor.color;
                    c.a = Alpha;
                    main.startColor = c;
                }
            }
        }
        else if(time >= 16)
        {
            if (Light)
                Light.color = new Color(1, 0.95f, 0.39f);
            if (Particles)
            {
                var main = Particles.main;
                main.startColor = new Color(1, 0.95f, 0.39f);
                if (Alpha > 0)
                {
                    Color c = main.startColor.color;
                    c.a = Alpha;
                    main.startColor = c;
                }
            }
        }
        else if(time >= 10)
        {
            if(Light)
                Light.color = Color.white;
            if (Particles)
            {
                var main = Particles.main;
                main.startColor = Color.white;
                if (Alpha > 0)
                {
                    Color c = main.startColor.color;
                    c.a = Alpha;
                    main.startColor = c;
                }
            }
        }
        else if(time >= 6)
        {
            if(Light)
                Light.color = Color.cyan;
            if (Particles)
            {
                var main = Particles.main;
                main.startColor = Color.cyan;
                if (Alpha > 0)
                {
                    Color c = main.startColor.color;
                    c.a = Alpha;
                    main.startColor = c;
                }
            }
        }
    }

    private void OnDisable()
    {
        GameClock.Ticked -= OnTick;
    }
}
