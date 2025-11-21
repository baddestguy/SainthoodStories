using System;
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
    [HideInInspector]public AudioSource VoiceOverSource;
    private string AmbientTrackName;
    [HideInInspector]public AudioSource gameplayTrack;
    [HideInInspector]public AudioLowPassFilter lowPassFilter;
    public string[] MusicPlaylist;

    public AudioMixer audioMixer;

    Dictionary<string, AudioClip> cachedVoices = new Dictionary<string, AudioClip>();
    public Dictionary<string, AudioMixerGroup> audioMixerGroup = new Dictionary<string, AudioMixerGroup>();

    Coroutine WeatherCoroutine;
    Coroutine AmbienceCoroutine;

    void Awake()
    {
        Instance = this;
        audioMixerGroup = audioMixer.FindMatchingGroups("Master").ToDictionary(key => key.name, value => value);
    }

    // Use this for initialization
    void Start()
    {
        
    }

    public void SongSelection()
    {
        //Keeping this just in case we want to have a summer/fall/winter playlist
        var season = MissionManager.Instance.CurrentMission.Season;
        switch (season)
        {
            case Season.SPRING:
            case Season.SUMMER:
            case Season.FALL:
                break;

            case Season.WINTER:
                break;
        }
    }

    public void StartPlaylist(bool daytime = true)
    {
        if (daytime)
        {
            if (MusicPlaylist.Contains("Convent_Music")) return;
            MusicPlaylist = new string[] 
            { 
                "Convent_Music", 
                "Field_Music",
                "Adoro Te Devote",
                "Ave Maria",
                "Away In A Manger",
                "Curoo Curoo",
                "Kyrie",
                "O Salutaris Hostia",
                "Sanctus",
                "Still, Still, Still",
                "Adoro Te Devote (Piano Version)",
                "Ave Maria (Piano Version)",
                "Away In A Manger (Piano Version)",
                "Curoo Curoo (Piano Version)",
                "Kyrie (Piano Version)",
                "O Salutaris Hostia (Piano Version)",
                "Sanctus (Piano Version)",
                "Still, Still, Still (Piano Version)",
                "Away In A Manger 2",
                "Away In A Manger 2 (Piano Version)"
            };
        }
        else
        {
            if (MusicPlaylist.Contains("Night")) return;

            MusicPlaylist = new string[] { "Night" };
        }
        MusicPlaylist.Shuffle();
        PlayMusic(MusicPlaylist[0]);
        StopCoroutine("StartPlaylistAsync");
        StartCoroutine("StartPlaylistAsync");
    }

    IEnumerator StartPlaylistAsync()
    {
        var index = 1;
        if (index >= MusicPlaylist.Length) index = 0;

        while (true)
        {
            yield return new WaitForSeconds(5);
            if (MusicAudioSourceChannel1.isPlaying) continue;

            yield return new WaitForSeconds(UnityEngine.Random.Range(40, 60));
            if (index >= MusicPlaylist.Length) index = 0;
            PlayMusic(MusicPlaylist[index]);
            index++;
            if (index == MusicPlaylist.Length)
            {
                index = 0;
                MusicPlaylist.Shuffle();
            }
        }
    }

    public void PlayMusic(string songName = "", float loopDelay = 90)
    {
        if (string.IsNullOrEmpty(songName))
            return;

        AudioSource oldTrack = MusicAudioSourceChannel1;

        MusicAudioSourceChannel1 = gameObject.AddComponent<AudioSource>();
        MusicAudioSourceChannel1.outputAudioMixerGroup = audioMixerGroup["Music"];
        MusicAudioSourceChannel1.clip = Resources.Load("Audio/Music/" + songName, typeof(AudioClip)) as AudioClip;
        MusicAudioSourceChannel1.Play();

        Destroy(oldTrack, 5);
        FadeMusic(1f);
        StartCoroutine(FadeAudioAsync(0f, oldTrack));
    }

    public void PlayAmbience(string ambience = "")
    {
        if (string.IsNullOrEmpty(ambience) || AmbientTrackName == ambience)
            return;

        AudioSource oldTrack = AmbientAudioSource;

        AmbientAudioSource = gameObject.AddComponent<AudioSource>();
        AmbientAudioSource.outputAudioMixerGroup = audioMixerGroup["Ambience"];
    //    print(gameObject.name);

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
        if(MissionManager.Instance.CurrentMission == null) return;
        string weather = "";

        switch (MissionManager.Instance.CurrentMission.Season)
        {
                //weather = "Heatwave_Ambience";
                //if (start && InteractableHouse.InsideHouse && WeatherAmbientAudioSource != null)
                //{
                //    if (WeatherCoroutine != null) StopCoroutine(WeatherCoroutine);
                //    WeatherAmbientAudioSource.volume = 0.2f;
                //    return;
                //}
                //break;

            case Season.SUMMER:
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
            if (WeatherCoroutine != null) StopCoroutine(WeatherCoroutine);
            WeatherCoroutine = StartCoroutine(FadeWeatherAudioAsync(1f, WeatherAmbientAudioSource));
        }
        else
        {
            Destroy(WeatherAmbientAudioSource, 5f);
            WeatherCoroutine = StartCoroutine(FadeWeatherAudioAsync(0f, WeatherAmbientAudioSource));
        }
    }

    public void FadeAmbience(float volume, bool destroyCurrentSource = false)
    {
        if (AmbienceCoroutine != null) StopCoroutine(AmbienceCoroutine);
        if (destroyCurrentSource)
            AmbientTrackName = "";

        AmbienceCoroutine = StartCoroutine(FadeAudioAsync(volume, AmbientAudioSource));
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
            if (src != null) src.volume = volume;
        }
    }

    IEnumerator FadeWeatherAudioAsync(float volume, AudioSource newSource = null)
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

    public AudioSource PlayVoice(string name)
    {
        if (VoiceOverSource == null)
        {
            VoiceOverSource = gameObject.AddComponent<AudioSource>();
            VoiceOverSource.outputAudioMixerGroup = audioMixerGroup["VO"];
        }

        VoiceOverSource.clip = Resources.Load("Audio/Narration/" + name, typeof(AudioClip)) as AudioClip;
        VoiceOverSource.Play();
        VoiceOverSource.loop = false;

        return VoiceOverSource;
    }

    public void PlayOneShotSfx(string name, float volume = 1f, float timeToDie = 1f, bool modifyPitch = false)
    {
        OneShotSource = gameObject.AddComponent<AudioSource>();
        OneShotSource.outputAudioMixerGroup = audioMixerGroup["SFX"];

        OneShotSource.clip = Resources.Load("Audio/" + name, typeof(AudioClip)) as AudioClip;
        OneShotSource.Play();
        OneShotSource.loop = false;
        OneShotSource.volume = volume;

        if (modifyPitch)
            OneShotSource.pitch = UnityEngine.Random.Range(0.5f, 1.5f);

        Destroy(OneShotSource, timeToDie == 1f ? OneShotSource.clip.length : timeToDie);
    }

    public void StopOneShotSfx(string name = "")
    {
        if (OneShotSource != null && (string.IsNullOrEmpty(name) || OneShotSource.clip.name == name))
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
        StopAllCoroutines();
        PlayHouseAmbience("", false, 0f);
        PlayWeatherAmbience(false);
        FadeMusic(1f);
        StartCoroutine(FadeAudioAsync(0f, MusicAudioSourceChannel1));
        StartCoroutine(FadeAudioAsync(0f, MusicAudioSourceChannel2));
        Destroy(MusicAudioSourceChannel1, 5);
        Destroy(MusicAudioSourceChannel2, 5);
        Destroy(AmbientAudioSource, 5);
        AmbientTrackName = "";
        FadeAmbience(0.3f);
    }

    public void EnableSFX(bool value, float volumePercent = 0)
    {
        
        float val = (volumePercent * 40.0f) - 40f;
        if (volumePercent == 0) val = -80;
        audioMixer.SetFloat("SFXVolume", (value) ? val : -80.0f);
        
    }
    public void EnableMusic(bool value, float volumePercent = 0)
    {
        float val = (volumePercent * 40.0f) - 40f;
        if (volumePercent == 0) val = -80;
        audioMixer.SetFloat("MusicVolume", (value) ? val : -80.0f);
        audioMixer.SetFloat("Music2Volume", (value) ? val : -80.0f);
    }

    public void EnableAmbiance(bool value, float volumePercent = 0)
    {
        float val = (volumePercent * 40.0f) - 40f;
        if (volumePercent == 0) val = -80;
        audioMixer.SetFloat("AmbianceVolume", (value) ? val : -80.0f);
    }

    public void SetGlobalAudio(bool value, float volumePercent = 0)
    {
        
        float val = (volumePercent * 40f) - 40f;
        if (volumePercent == 0) val = -80;

         audioMixer.SetFloat("GlobalVolume", (value) ? val : -80.0f);
    }
}
