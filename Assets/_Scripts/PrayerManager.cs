using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Newtonsoft.Json.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.Rendering.PostProcessing;

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
    public TextMeshProUGUI TitleText;
    public PrayerType PrayerType;
    public GameObject RosaryRing;
    public GameObject PrayerButtons;
    public GameObject ExitButtonsGroup;
    public Transform PrayerButtonsAnchor;
    public GameObject[] BuildingInteriors;
    public AudioSource CurrentAudioSource;

    private float prevMusicVol;
    private float prevAmbiantVol;
    private bool MoveNext;
    private Coroutine prayerCoroutine;
    public float[] RingPositions = new float[] 
    {
        327.1f,
        364.7f,
        399f,
        428.5f,
        454.5f,
        488.5f,
        518.5f,
        554.5f,
        584.5f,
        618.5f,
        644.5f
    };

    // Start is called before the first frame update
    void Start()
    {
        SoundManager.Instance.PlayAmbience("SummerNight_Ambience");
        SoundManager.Instance.PlayMusic("Ave Maria (Piano Version)");

        var interiorIndex = Random.Range(0, BuildingInteriors.Length+1);
        if(interiorIndex < BuildingInteriors.Length)
        {
            BuildingInteriors[interiorIndex].SetActive(true);
        }
    }

    public void OnRosaryBtnClick()
    {
        //Popup more options for choosing a Mystery

        PrayerButtons.transform.DOMoveY(-300f, 1f);

        TitleText.DOFade(1,0);
        TitleText.text = "ROSARY";
        SoundManager.Instance.PlayOneShotSfx("StartGame_SFX", 1f, 10);
      
        PrayerType = PrayerType.ROSARY_JOYFUL;
        StartPrayerSequence(PrayerType.ROSARY_JOYFUL);
    }

    public void StopPrayerSequence()
    {
        GameSettings.Instance.SetVolume("Music", prevMusicVol);
        GameSettings.Instance.SetVolume("Ambiance", prevAmbiantVol);

        StopCoroutine(prayerCoroutine);
        ExitButtonsGroup.transform.DOMoveY(PrayerButtons.transform.position.y, 1f);
        RosaryRing.transform.DOMoveY(2f, 2f);
        CurrentAudioSource.Stop();
        PrayerButtons.transform.DOMoveY(PrayerButtonsAnchor.position.y, 1f);
    }

    public void OnMoveNext()
    {
        MoveNext = true;
        CurrentAudioSource.Stop();
    }

    public void StartPrayerSequence(PrayerType prayer)
    {
        prevMusicVol = GameSettings.Instance.musicVolume;
        prevAmbiantVol = GameSettings.Instance.ambianceVolume;
        GameSettings.Instance.SetVolume("Music", 0.25f);
        GameSettings.Instance.SetVolume("Ambiance", 0.25f);

        switch (prayer)
        {
            case PrayerType.ROSARY_JOYFUL:
                prayerCoroutine = StartCoroutine(RosaryPrayer());
                break;

            case PrayerType.DIVINE_MERCY:
                break;
        }
    }

    IEnumerator RosaryPrayer()
    {
        var rosaryRingIndex = 0;

        yield return new WaitForSeconds(2f);

        TitleText.DOFade(0, 2);
        RosaryRing.transform.DOMoveY(-3.7f, 2f);
        RosaryRing.transform.DORotate(new Vector3(270, RingPositions[rosaryRingIndex], 0), 2f);

        yield return new WaitForSeconds(2f);

        ExitButtonsGroup.transform.DOMoveY(PrayerButtonsAnchor.position.y, 1f);
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
            if (!MoveNext)
            {
                yield return new WaitForSeconds(1f);
            }
            MoveNext = false;
            CurrentAudioSource = SoundManager.Instance.PlayVoice("HailMary");
            RosaryRing.transform.DORotate(new Vector3(270, RingPositions[i], 0), 1f);
        }
        while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
        if (!MoveNext)
        {
            yield return new WaitForSeconds(2f);
        }
        MoveNext = false;

        CurrentAudioSource = SoundManager.Instance.PlayVoice("GloryBe");
        RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 1f);
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
                if (!MoveNext)
                {
                    yield return new WaitForSeconds(1f);
                }
                MoveNext = false;
                CurrentAudioSource = SoundManager.Instance.PlayVoice("HailMary");
                RosaryRing.transform.DORotate(new Vector3(270, RingPositions[j], 0), 1f);
            }
            while (CurrentAudioSource.isPlaying && !MoveNext) yield return new WaitForSeconds(0.5f);
            if (!MoveNext)
            {
                yield return new WaitForSeconds(2f);
            }
            MoveNext = false;

            CurrentAudioSource = SoundManager.Instance.PlayVoice("GloryBe");
            RosaryRing.transform.DORotate(new Vector3(270, RingPositions[0], 0), 1f);
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

        StopPrayerSequence();
    }

}
