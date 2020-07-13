using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SoundManager : MonoBehaviour
{
    public static SoundManager Instance { get; private set; }

    public Dictionary<string, AudioClip> cachedAudioClips = new Dictionary<string, AudioClip>();
    public List<string> tracks;
    private AudioSource MusicAudioSource;
    private AudioSource AmbientAudioSource;
    private AudioSource HouseAmbience;
    private AudioSource WeatherAmbientAudioSource;
    private string AmbientTrackName;
    private AudioSource gameplayTrack;
    private AudioLowPassFilter lowPassFilter;

    public bool soundEnabled;
    public bool musicEnabled;

    Dictionary<string, AudioClip> cachedVoices = new Dictionary<string, AudioClip>();

    void Awake()
    {
        Instance = this;
    }

    // Use this for initialization
    void Start()
    {
        soundEnabled = PlayerPrefs.HasKey("soundEnabled") ? bool.Parse(PlayerPrefs.GetString("soundEnabled")) : true;
        musicEnabled = PlayerPrefs.HasKey("musicEnabled") ? bool.Parse(PlayerPrefs.GetString("musicEnabled")) : true;
 //       lowPassFilter = GetComponent<AudioLowPassFilter>();
  //      lowPassFilter.enabled = false;
    }

    public float GetBaseTrackSeconds()
    {
        return MusicAudioSource.time;
    }

    public void PlayOST(string songName = "", bool shouldLoop = false, float volume = 0.4f, float startTime = 0f)
    {
        MusicAudioSource = gameObject.AddComponent<AudioSource>();
        //		if(TutorialManager.Instance && string.IsNullOrEmpty(songName))
        //			baseTrack.clip = Resources.Load("_Sound/Music/Track1", typeof(AudioClip)) as AudioClip;
        MusicAudioSource.clip = Resources.Load("Audio/" + songName, typeof(AudioClip)) as AudioClip;
        MusicAudioSource.Play();
        MusicAudioSource.loop = shouldLoop;
        if (musicEnabled)
        {
            MusicAudioSource.volume = string.IsNullOrEmpty(songName) ? 0.7f : volume;
        }
        else
            MusicAudioSource.volume = 0f;
        MusicAudioSource.time = startTime;
    }

    public void loadOST()
    {
        gameplayTrack = gameObject.AddComponent<AudioSource>();
        gameplayTrack.clip = Resources.Load("_Sound/Music/" + tracks[Random.Range(0, tracks.Count)], typeof(AudioClip)) as AudioClip;
    }

    public void PlayWeatherAmbience(string weather, bool start, float volume = 1f)
    {
        if (start)
        {
            if(WeatherAmbientAudioSource == null)
                WeatherAmbientAudioSource = gameObject.AddComponent<AudioSource>();

            WeatherAmbientAudioSource.clip = Resources.Load("Audio/" + weather, typeof(AudioClip)) as AudioClip;
            WeatherAmbientAudioSource.Play();
            WeatherAmbientAudioSource.loop = true;
            WeatherAmbientAudioSource.volume = 0f;
            StartCoroutine(FadeAudioAsync(volume, false, WeatherAmbientAudioSource));
        }
        else
        {
            Destroy(WeatherAmbientAudioSource, 5f);
            StartCoroutine(FadeAudioAsync(0f, false, WeatherAmbientAudioSource));
        }
    }

    public void PlayAmbience(string ambience)
    {
        if (string.IsNullOrEmpty(ambience) || AmbientTrackName == ambience)
            return;

        AudioSource oldTrack = AmbientAudioSource;

        AmbientAudioSource = gameObject.AddComponent<AudioSource>();

        AmbientTrackName = ambience;
        AmbientAudioSource.clip = Resources.Load("Audio/" + ambience, typeof(AudioClip)) as AudioClip;
        AmbientAudioSource.Play();
        AmbientAudioSource.loop = true;
        AmbientAudioSource.volume = 0f;

        Destroy(oldTrack, 5);
        FadeAmbience(0.3f);
        StartCoroutine(FadeAudioAsync(0f, false, oldTrack));
    }

    public void FadeAmbience(float volume, bool stopMusic = false)
    {
        StartCoroutine(FadeAudioAsync(volume, stopMusic, AmbientAudioSource));
    }

    public void FadeMusic(float volume, bool stopMusic = false)
    {
        StartCoroutine(FadeAudioAsync(volume, stopMusic, MusicAudioSource));
    }

    IEnumerator FadeAudioAsync(float volume, bool stopMusic, AudioSource newSource = null)
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

    public void PlayOneShotSfx(string name, float volume = 1f, float timeToDie = 1f)
    {
        AudioSource track = gameObject.AddComponent<AudioSource>();

        track.clip = Resources.Load("Audio/" + name, typeof(AudioClip)) as AudioClip;
        track.Play();
        track.loop = false;
        track.volume = volume;

        Destroy(track, timeToDie);
    }

    public void PlayHouseAmbience(string name, bool start, float volume = 1f)
    {
        if(start)
        {
            HouseAmbience = gameObject.AddComponent<AudioSource>();

            HouseAmbience.clip = Resources.Load("Audio/" + name, typeof(AudioClip)) as AudioClip;
            HouseAmbience.Play();
            HouseAmbience.loop = true;
            HouseAmbience.volume = volume;
        }
        else
        {
            Destroy(HouseAmbience, 5f);
            StartCoroutine(FadeAudioAsync(0f, false, HouseAmbience));
        }
    }

    public void CrossFadeMusic(string newTrack, float endTime, bool shouldLoop, float maxVolume)
    {
        if (!musicEnabled) return;
        AudioSource oldTrack = MusicAudioSource;
        Destroy(oldTrack, endTime);

        PlayOST(newTrack, shouldLoop, 0f);
        FadeMusic(maxVolume);
        StartCoroutine(FadeAudioAsync(endTime, false, oldTrack));
    }

    public void StopFadeoutAndMaxVolume()
    {
        StopAllCoroutines();
        StartCoroutine(_StopFadeoutAndMaxVolume());
    }

    IEnumerator _StopFadeoutAndMaxVolume()
    {
        //	Debug.Log("HOL UP!");
        yield return new WaitForSeconds(0.1f);
        //	Debug.Log("DONE WAITING! SETTING MAX VOLUME!!");
        maxVolume();
    }

    public void NormalizeSoundFX()
    {
        //	AudioLowPassFilter low = FindObjectOfType<AudioLowPassFilter> ();
        lowPassFilter.cutoffFrequency = 5000;
        lowPassFilter.enabled = false;
    }

    public void LowFilterFX()
    {
        //	AudioLowPassFilter low = FindObjectOfType<AudioLowPassFilter> ();
        lowPassFilter.enabled = true;
        lowPassFilter.cutoffFrequency = 1000;
    }

    public void HighFilterFX()
    {

    }

    public void maxVolume()
    {
        if (MusicAudioSource && musicEnabled)
            MusicAudioSource.volume = 0.7f;
    }
    public void enableSound()
    {
        //TODO: SAVE THE SETTINGS!
        soundEnabled = true;
        if (AmbientAudioSource)
            AmbientAudioSource.volume = 0.3f;
        PlayerPrefs.SetString("soundEnabled", soundEnabled.ToString());
    }
    public void disableSound()
    {
        soundEnabled = !soundEnabled;
        if (AmbientAudioSource && soundEnabled)
            AmbientAudioSource.volume = 0.3f;
        else
            AmbientAudioSource.volume = 0f;

        PlayerPrefs.SetString("soundEnabled", soundEnabled.ToString());
    }
    public void enableMusic()
    {
        if (MusicAudioSource)
            MusicAudioSource.volume = 1;// GameDataManager.Instance.floatConstants["MUSIC_VOLUME"];
        musicEnabled = true;
        PlayerPrefs.SetString("musicEnabled", musicEnabled.ToString());
    }
    public void disableMusic()
    {
        musicEnabled = !musicEnabled;

        if (MusicAudioSource && musicEnabled)
            MusicAudioSource.volume = 1; // GameDataManager.Instance.floatConstants["MUSIC_VOLUME"];
        else if (MusicAudioSource && !musicEnabled) MusicAudioSource.volume = 0f;
        PlayerPrefs.SetString("musicEnabled", musicEnabled.ToString());
    }

}
