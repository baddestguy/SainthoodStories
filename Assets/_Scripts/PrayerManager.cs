using System;
using System.Collections;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.SceneManagement;
using Random = UnityEngine.Random;

public enum PrayerType
{
    ROSARY_JOYFUL = 0,
    ROSARY_GLORIOUS,
    ROSARY_SORROWFUL,
    ROSARY_LUMINOUS,
    DIVINE_MERCY
}


public class PrayerManager : MonoBehaviour
{
    public static PrayerManager Instance { get; private set; }

    public TextMeshProUGUI TitleText;
    public PrayerType PrayerType;
    public GameObject RosaryRing;
    public GameObject PrayerButtons;
    public GameObject PrayerButtonTypes;
    public GameObject ExitButtonsGroup;
    public Transform PrayerButtonsAnchor;
    public GameObject[] BuildingInteriors;
    public AudioSource CurrentAudioSource;

    public GameObject MainGlowFx;
    public GameObject MainGlowHitFx;

    private float prevMusicVol;
    private float prevAmbiantVol;
    private bool MoveNext;
    private Coroutine prayerCoroutine;
    public float[] RingPositions = new float[] 
    {
        327.1f,
        367f,
        399f,
        428.5f,
        460.5f,
        491.5f,
        523f,
        554.5f,
        584.5f,
        615.5f,
        647f
    };

    public bool Praying { get; private set; }


    void Awake()
    {
        Instance = this;
    }

    // Start is called before the first frame update
    void Start()
    {
        SoundManager.Instance.PlayMusic("Ave Maria (Piano Version)");
        SoundManager.Instance.PlayAmbience("SummerDay_Ambience");

        var interiorIndex = Random.Range(0, BuildingInteriors.Length+1);
        if(interiorIndex < BuildingInteriors.Length)
        {
            BuildingInteriors[interiorIndex].SetActive(true);
        }
    //    StartCoroutine(LightningThunder());
        StartCoroutine(StartPlaylistAsync());
    }

    IEnumerator StartPlaylistAsync()
    {
        while (true)
        {
            yield return new WaitForSeconds(5);
            if (SoundManager.Instance.MusicAudioSourceChannel1.isPlaying) continue;

            yield return new WaitForSeconds(Random.Range(40, 60));
            SoundManager.Instance.PlayMusic("Ave Maria (Piano Version)");
        }
    }

    public void OnRosaryBtnClick()
    {
        PrayerButtonTypes.SetActive(!PrayerButtonTypes.activeSelf);
    }

    public void SelectRosaryType(string prayerType)
    {
        PrayerButtons.transform.DOMoveY(-300f, 1f);
        PrayerButtonTypes.SetActive(false);

        TitleText.DOFade(1, 0);
        TitleText.text = "ROSARY";
        SoundManager.Instance.PlayOneShotSfx("StartGame_SFX", 1f, 10);

        PrayerType = (PrayerType)Enum.Parse(typeof(PrayerType), prayerType);
        StartPrayerSequence(PrayerType);

    }

    public void StopPrayerSequence()
    {
        if(!Praying)
        {
            transform.Find("Exit").DOMoveY(-300f, 0.2f);
            PrayerButtons.transform.DOMoveY(-300f, 0.2f);

            StartCoroutine(ScheduleCallback(() =>
            {
                SoundManager.Instance.EndAllTracks();
                GameManager.Instance.LoadScene("MainMenu", LoadSceneMode.Single);
            }, 1));

            return;
        }

        Praying = false;
        TitleText.DOFade(0, 0);
        GameSettings.Instance.SetVolume("Music", prevMusicVol);
        GameSettings.Instance.SetVolume("Ambiance", prevAmbiantVol);

        StopCoroutine(prayerCoroutine);
        StopCoroutine("HitFx");
        ExitButtonsGroup.transform.DOMoveY(PrayerButtons.transform.position.y, 1f);
        RosaryRing.transform.DOMoveY(7f, 2f);
        if(CurrentAudioSource != null)
            CurrentAudioSource.Stop();
        PrayerButtons.transform.DOMoveY(PrayerButtonsAnchor.position.y, 1f);
        MainGlowFx.SetActive(false);
    }

    public void OnMoveNext()
    {
        MoveNext = true;
        CurrentAudioSource.Stop();
    }

    public void StartPrayerSequence(PrayerType prayer)
    {
        Praying = true;
        prevMusicVol = GameSettings.Instance.musicVolume;
        prevAmbiantVol = GameSettings.Instance.ambianceVolume;
        GameSettings.Instance.SetVolume("Music", 0.25f);
        GameSettings.Instance.SetVolume("Ambiance", 0.25f);

        switch (prayer)
        {
            case PrayerType.ROSARY_JOYFUL:
            case PrayerType.ROSARY_GLORIOUS:
            case PrayerType.ROSARY_LUMINOUS:
            case PrayerType.ROSARY_SORROWFUL:
                prayerCoroutine = StartCoroutine(RosaryPrayer());
                break;

            case PrayerType.DIVINE_MERCY:
                break;
        }
    }

    IEnumerator HitFx()
    {
        MainGlowFx.SetActive(false);
        yield return new WaitForSeconds(1);
        MainGlowHitFx.SetActive(true);
        MainGlowFx.SetActive(true);
        SoundManager.Instance.PlayOneShotSfx("Walk_SFX", 0.2f);
    }

    IEnumerator RosaryPrayer()
    {
        SoundManager.Instance.PlayOneShotSfx("MassBells_SFX", timeToDie: 10f);

        yield return new WaitForSeconds(2f);

        TitleText.DOFade(0, 2);
        RosaryRing.transform.DOMoveY(-3.7f, 2f);
        RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 2f);

        yield return new WaitForSeconds(2f);

        ExitButtonsGroup.transform.DOMoveY(PrayerButtonsAnchor.position.y, 1f);
        MainGlowFx.SetActive(true);
        MainGlowHitFx.SetActive(true);
        CurrentAudioSource = SoundManager.Instance.PlayVoice("Opening");
        while(CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);

        CurrentAudioSource = SoundManager.Instance.PlayVoice("Creed");
        while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);

        if (!MoveNext)
        {
            yield return new WaitForSeconds(2f);
        }
        MoveNext = false;

        CurrentAudioSource = SoundManager.Instance.PlayVoice("LordsPrayer");
        //Loop 3 Hail Marys
        for (int i = 1; i <= 3; i++)
        {
            while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
            RosaryRing.transform.DORotate(new Vector3(270, RingPositions[i], 0), 1f);
            StartCoroutine("HitFx");
            if (!MoveNext)
            {
                yield return new WaitForSeconds(1f);
            }
            MoveNext = false;
            CurrentAudioSource = SoundManager.Instance.PlayVoice("HailMary");
        }
        while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
        RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 1f);
        StartCoroutine("HitFx");
        if (!MoveNext)
        {
            yield return new WaitForSeconds(2f);
        }
        MoveNext = false;

        CurrentAudioSource = SoundManager.Instance.PlayVoice("GloryBe");
        while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
        if (!MoveNext)
        {
            yield return new WaitForSeconds(2f);
        }
        MoveNext = false;

        for(int i = 1; i <=5; i++)
        {
            CurrentAudioSource = SoundManager.Instance.PlayVoice(PrayerType.ToString()+i);
            RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 1f);
            while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
            if (!MoveNext)
            {
                yield return new WaitForSeconds(2f);
            }
            MoveNext = false;

            CurrentAudioSource = SoundManager.Instance.PlayVoice("LordsPrayer");
            //Loop 10 Hail Marys
            for (int j = 1; j <= 10; j++)
            {
                while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
                RosaryRing.transform.DORotate(new Vector3(270, RingPositions[j], 0), 1f);
                StartCoroutine("HitFx");
                if (!MoveNext)
                {
                    yield return new WaitForSeconds(1f);
                }
                MoveNext = false;
                CurrentAudioSource = SoundManager.Instance.PlayVoice("HailMary");
            }
            while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
            RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 1f);
            StartCoroutine("HitFx");
            if (!MoveNext)
            {
                yield return new WaitForSeconds(2f);
            }
            MoveNext = false;

            CurrentAudioSource = SoundManager.Instance.PlayVoice("GloryBe");
            while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
            if (!MoveNext)
            {
                yield return new WaitForSeconds(2f);
            }
            MoveNext = false;

            CurrentAudioSource = SoundManager.Instance.PlayVoice("OMyJesus");
            while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
            if (!MoveNext)
            {
                yield return new WaitForSeconds(2f);
            }
            MoveNext = false;
        }

        CurrentAudioSource = SoundManager.Instance.PlayVoice("HailHolyQueen");
        RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 1f);
        while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
        if (!MoveNext)
        {
            yield return new WaitForSeconds(2f);
        }
        MoveNext = false;

        CurrentAudioSource = SoundManager.Instance.PlayVoice("Closing");
        RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 1f);
        while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
        if (!MoveNext)
        {
            yield return new WaitForSeconds(2f);
        }
        MoveNext = false;

        CurrentAudioSource = SoundManager.Instance.PlayVoice("Opening");
        RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 1f);
        while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
        if (!MoveNext)
        {
            yield return new WaitForSeconds(2f);
        }
        MoveNext = false;

        StopPrayerSequence();
    }

    private IEnumerator LightningThunder()
    {
        while (true)
        {
            SoundManager.Instance.PlayOneShotSfx("Thunder_SFX", 0.2f, 30);
            yield return new WaitForSeconds(Random.Range(30f, 60f));
        }
    }

    private IEnumerator ScheduleCallback(Action callback, float delay)
    {
        yield return new WaitForSeconds(delay);
        callback?.Invoke();
    }

}
