using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Audio;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public Dictionary<string, AudioClip> cachedAudioClips = new Dictionary<string, AudioClip>();
    public List<string> tracks;
    [HideInInspector]public AudioSource MusicAudioSourceChannel1;
    [HideInInspector]public AudioSource MusicAudioSourceChannel2;
    [HideInInspector]public AudioSource AmbientAudioSource;
    [HideInInspector]public AudioSource HouseAmbience;
    [HideInInspector]public AudioSource WeatherAmbientAudioSource;
    [HideInInspector]public AudioSource OneShotSource;
    private string AmbientTrackName;
    [HideInInspector]public AudioSource gameplayTrack;
    [HideInInspector]public AudioLowPassFilter lowPassFilter;

    public AudioMixer audioMixer;

    Dictionary<string, AudioClip> cachedVoices = new Dictionary<string, AudioClip>();
    public Dictionary<string, AudioMixerGroup> audioMixerGroup = new Dictionary<string, AudioMixerGroup>();


    void Awake()
    {
        Instance = this;
        audioMixerGroup = audioMixer.FindMatchingGroups("Master").ToDictionary(key => key.name, value => value);
    }

    // Use this for initialization
    void Start()
    {
        
    }

    public void PlayMusic(string songName = "", string songName2 = "", bool shouldLoop = true, float startTime = 0f)
    {
        if (string.IsNullOrEmpty(songName))
            return;

        AudioSource oldTrack = MusicAudioSourceChannel1;
     
        MusicAudioSourceChannel1 = gameObject.AddComponent<AudioSource>();
        MusicAudioSourceChannel1.outputAudioMixerGroup = audioMixerGroup["Music"];
        MusicAudioSourceChannel1.clip = Resources.Load("Audio/Music/" + songName, typeof(AudioClip)) as AudioClip;
        MusicAudioSourceChannel1.Play();
        MusicAudioSourceChannel1.loop = shouldLoop;
        MusicAudioSourceChannel1.time = startTime;

        if (!string.IsNullOrEmpty(songName2))
        {
            MusicAudioSourceChannel2 = gameObject.AddComponent<AudioSource>();
            MusicAudioSourceChannel2.outputAudioMixerGroup = audioMixerGroup["Music2"];
            MusicAudioSourceChannel2.clip = Resources.Load("Audio/Music/" + songName2, typeof(AudioClip)) as AudioClip;
            MusicAudioSourceChannel2.Play();
            MusicAudioSourceChannel2.loop = shouldLoop;
            MusicAudioSourceChannel2.time = startTime;
        }
        Destroy(oldTrack, 5);
        FadeMusic(1f);
        StartCoroutine(FadeAudioAsync(0f, oldTrack));
    }

    public void SwitchMusicChannel(bool inConvent)
    {
        if (inConvent)
        {
            FadeMusic(1f, MusicAudioSourceChannel1);
            FadeMusic(0f, MusicAudioSourceChannel2);
        }
        else
        {
            FadeMusic(1f, MusicAudioSourceChannel2);
            FadeMusic(0f, MusicAudioSourceChannel1);
        }
    }

    public void PlayAmbience()
    {
        string ambience = "";
        GameClock clock = GameManager.Instance.GameClock;
        switch (MissionManager.Instance.CurrentMission.Season)
        {
            case Season.SUMMER:
                if (clock.Time >= 21 || clock.Time < 6)
                {
                    ambience = "SummerNight_Ambience";
                }
                else if (clock.Time >= 6)
                {
                    ambience = "SummerDay_Ambience";
                }
                break;

            case Season.FALL:
                ambience = "Fall_Ambience";
                break;

            case Season.WINTER:
                ambience = "Winter_Ambience";
                break;

            case Season.SPRING:
                ambience = "Spring_Ambience";
                break;
        }

        if (string.IsNullOrEmpty(ambience) || AmbientTrackName == ambience)
            return;

        AudioSource oldTrack = AmbientAudioSource;

        AmbientAudioSource = gameObject.AddComponent<AudioSource>();
        AmbientAudioSource.outputAudioMixerGroup = audioMixerGroup["Ambience"];
        print(gameObject.name);

        AmbientTrackName = ambience;
        AmbientAudioSource.clip = Resources.Load("Audio/" + ambience, typeof(AudioClip)) as AudioClip;
        AmbientAudioSource.Play();
        AmbientAudioSource.loop = true;
        AmbientAudioSource.volume = 0f;

        Destroy(oldTrack, 5);
        FadeAmbience(0.3f);
        StartCoroutine(FadeAudioAsync(0f, oldTrack));
    }

    public void PlayWeatherAmbience(bool start)
    {
        string weather = "";

        switch (MissionManager.Instance.CurrentMission.Season)
        {
            case Season.SUMMER:
                weather = "Heatwave_Ambience";
                if (InteractableHouse.InsideHouse && WeatherAmbientAudioSource != null)
                {
                    WeatherAmbientAudioSource.volume = 0.2f;
                    return;
                }
                break;

            case Season.FALL:
                if (InteractableHouse.InsideHouse)
                    weather = "RainInterior_Ambience";
                else
                    weather = "RainExterior_Ambience";
                break;

            case Season.WINTER:
                weather = "Blizzard_Ambience";
                break;

            case Season.SPRING:
                weather = "Hail_Ambience";
                break;
        }

        if (start)
        {
            if(WeatherAmbientAudioSource == null)
            {
                WeatherAmbientAudioSource = gameObject.AddComponent<AudioSource>();
                WeatherAmbientAudioSource.outputAudioMixerGroup = audioMixerGroup["Ambience"];
            }

            WeatherAmbientAudioSource.clip = Resources.Load("Audio/" + weather, typeof(AudioClip)) as AudioClip;
            WeatherAmbientAudioSource.Play();
            WeatherAmbientAudioSource.loop = true;
            WeatherAmbientAudioSource.volume = 0f;
            StartCoroutine(FadeAudioAsync(1f, WeatherAmbientAudioSource));
        }
        else
        {
            Destroy(WeatherAmbientAudioSource, 5f);
            StartCoroutine(FadeAudioAsync(0f, WeatherAmbientAudioSource));
        }
    }

    public void FadeAmbience(float volume)
    {
        StartCoroutine(FadeAudioAsync(volume, AmbientAudioSource));
    }

    public void FadeMusic(float volume, AudioSource newSource = null)
    {
        StartCoroutine(FadeAudioAsync(volume, newSource));
    }

    IEnumerator FadeAudioAsync(float volume, AudioSource newSource = null)
    {
        AudioSource src = newSource;
        if (src)
        {
            while (src != null && Mathf.Abs(src.volume - volume) > 0.01f)
            {
                src.volume = Mathf.Lerp(src.volume, volume, Time.deltaTime);
                yield return null;
            }
        }
    }

    public void PlayOneShotSfx(string name, float volume = 1f, float timeToDie = 1f, bool modifyPitch = false)
    {
        OneShotSource = gameObject.AddComponent<AudioSource>();
        OneShotSource.outputAudioMixerGroup = audioMixerGroup["SFX"];

        OneShotSource.clip = Resources.Load("Audio/" + name, typeof(AudioClip)) as AudioClip;
        OneShotSource.Play();
        OneShotSource.loop = false;
        OneShotSource.volume = volume;

        Destroy(OneShotSource, timeToDie);
    }

    public void StopOneShotSfx(string name)
    {
        if (OneShotSource != null && OneShotSource.clip.name == name)
        {
            Destroy(OneShotSource);
        }
    }

    bool DestroyHouseAmbience;
    public void PlayHouseAmbience(string name, bool start, float volume = 1f)
    {
        var trackName = name + "_Ambience";
        if (HouseAmbience != null && HouseAmbience.clip != null && trackName == HouseAmbience.clip.name && !DestroyHouseAmbience && start) return;
        
        if(start)
        {
            AudioClip clip = Resources.Load("Audio/" + trackName, typeof(AudioClip)) as AudioClip;
            if (clip == null) return;

            HouseAmbience = gameObject.AddComponent<AudioSource>();
            HouseAmbience.outputAudioMixerGroup = audioMixerGroup["Ambience"];
            HouseAmbience.clip = clip;
            HouseAmbience.Play();
            HouseAmbience.loop = true;
            HouseAmbience.volume = volume;
            DestroyHouseAmbience = false;
        }
        else
        {
            DestroyHouseAmbience = true;
            Destroy(HouseAmbience, 5f);
            StartCoroutine(FadeAudioAsync(0f, HouseAmbience));
        }
    }

    public void EndAllTracks()
    {
        PlayHouseAmbience("", false, 0f);
        PlayWeatherAmbience(false);
        FadeMusic(1f);
        StartCoroutine(FadeAudioAsync(0f, MusicAudioSourceChannel1));
        StartCoroutine(FadeAudioAsync(0f, MusicAudioSourceChannel2));
        Destroy(MusicAudioSourceChannel1, 5);
        Destroy(MusicAudioSourceChannel2, 5);
        Destroy(AmbientAudioSource, 5);
        FadeAmbience(0.3f);
    }

    public void EnableSFX(bool value, float volumePercent = 0)
    {
        
        float val = (volumePercent * 20.0f) - 10f;
        if (volumePercent == 0) val = -80;
        audioMixer.SetFloat("SFXVolume", (value) ? val : -80.0f);
        
    }
    public void EnableMusic(bool value, float volumePercent = 0)
    {
        float val = (volumePercent * 20.0f) - 10f;
        if (volumePercent == 0) val = -80;
        audioMixer.SetFloat("MusicVolume", (value) ? val : -80.0f);
        audioMixer.SetFloat("Music2Volume", (value) ? val : -80.0f);
    }

    public void EnableAmbiance(bool value, float volumePercent = 0)
    {
        float val = (volumePercent * 20.0f) - 10f;
        if (volumePercent == 0) val = -80;
        audioMixer.SetFloat("AmbianceVolume", (value) ? val : -80.0f);
    }

    public void SetGlobalAudio(bool value, float volumePercent = 0)
    {
        
        float val = (volumePercent * 20.0f) - 10f;
        if (volumePercent == 0) val = -80;

         audioMixer.SetFloat("GlobalVolume", (value) ? val : -80.0f);
    }
}
