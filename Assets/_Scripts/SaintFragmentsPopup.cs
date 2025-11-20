using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaintFragmentsPopup : MonoBehaviour
{
    public Dictionary<SaintID, List<SaintFragmentData>> Data;
    public Image CharPotrait;
    public TextMeshProUGUI Fragment;
    public TextMeshProUGUI SaintName;
    public ScrollRect ScrollRect;

    //Story Sequence
    public GameObject StorySequenceObj;
    public TextMeshProUGUI StoryEventText;
    public Image StoryEventBackground;
    private int CurrentSaintIndex = 0;
    private int CurrentSequenceNumber = 0;
    private IEnumerable<SaintsEvent> EventList;
    private bool CanSkipIntro;
    private bool ShowingIntro;

    public AudioSource CurrentAudioSource;
    private float prevMusicVol;
    private float prevAmbiantVol;

    public void Open()
    {
        CustomEventPopup.IsDisplaying = true;
        UI.Instance.EnableAllUIElements(false);
        gameObject.SetActive(true);

        Data = InventoryManager.Instance.SaintFragments;

        UpdateSaint();
    }

    private void UpdateSaint()
    {
        if (!SaintsManager.Instance.UnlockedSaints.Any())
        {
            return;
        }

        SaintData saintData = SaintsManager.Instance.UnlockedSaints[CurrentSaintIndex];

        //populate the saint data
        CharPotrait.enabled = true;
        CharPotrait.sprite = Resources.Load<Sprite>(saintData.IconPath);
        SaintName.text = saintData.Name;
    }

    public void SelectSaint()
    {
        StorySequenceObj.SetActive(true);
        
        var currentSaintId = SaintsManager.Instance.UnlockedSaints[CurrentSaintIndex].Id;
        EventList = GameDataManager.Instance.SaintsEvent.Values.Where(e => e.Id.Contains(currentSaintId.ToString()));

        prevMusicVol = GameSettings.Instance.musicVolume;
        prevAmbiantVol = GameSettings.Instance.ambianceVolume;
        GameSettings.Instance.SetVolume("Music", 0.5f);
        GameSettings.Instance.SetVolume("Ambiance", 0.5f);
        GameSettings.Instance.SetVolume("SFX", 0.5f);

        Proceed();
    }

    public void Proceed()
    {
        if (ShowingIntro && !CanSkipIntro) return;

        StopCoroutine("SaintIntro");
        if (CurrentSequenceNumber >= EventList.Count())
        {
            StorySequenceObj.SetActive(false);
            CurrentSequenceNumber = 0;
            GameSettings.Instance.SetVolume("Music", prevMusicVol);
            GameSettings.Instance.SetVolume("Ambiance", prevAmbiantVol);
            return;
        }

        if(CurrentSequenceNumber == 0)
        {
            StartCoroutine("SaintIntro");
            return;
        }

        var currentEvent = EventList.ElementAt(CurrentSequenceNumber);

        var text = LocalizationManager.Instance.GetText(currentEvent.DescriptionKey);
        StoryEventText.text = $"{currentEvent.FontColor}{text}";
        StoryEventText.color = new Color(StoryEventText.color.r, StoryEventText.color.g, StoryEventText.color.b, 0f);
        StoryEventText.DOFade(1f, 1f).SetEase(Ease.Linear);

        var voice = LocalizationManager.Instance.GetVoice(currentEvent.DescriptionKey);
        if (CurrentAudioSource != null) CurrentAudioSource.Stop();
        CurrentAudioSource = SoundManager.Instance.PlayVoice(voice);

        Color newColor;
        if (ColorUtility.TryParseHtmlString(currentEvent.BackgroundColor, out newColor))
        {
            StoryEventBackground.DOColor(newColor, 1f);
        }

        if (currentEvent.SoundEffect == "STOP")
            SoundManager.Instance.StopOneShotSfx();
        else
            SoundManager.Instance.PlayOneShotSfx(currentEvent.SoundEffect, timeToDie: 15);

        if (currentEvent.Music == "STOP")
            SoundManager.Instance.FadeMusic(0, SoundManager.Instance.MusicAudioSourceChannel1);
        else
            SoundManager.Instance.PlayMusic(currentEvent.Music);

        if (currentEvent.Ambience == "STOP")
            SoundManager.Instance.FadeAmbience(0, true);
        else
            SoundManager.Instance.PlayAmbience(currentEvent.Ambience);


        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
        CurrentSequenceNumber++;
    }

    IEnumerator SaintIntro()
    {
        ShowingIntro = true;
        SoundManager.Instance.StopOneShotSfx();
        SoundManager.Instance.FadeMusic(0, SoundManager.Instance.MusicAudioSourceChannel1);
        SoundManager.Instance.FadeAmbience(0, true);

        var currentEvent = EventList.ElementAt(CurrentSequenceNumber);
        CurrentSequenceNumber++;

        var text = LocalizationManager.Instance.GetText(currentEvent.DescriptionKey);
        StoryEventText.text = $"{currentEvent.FontColor}{text}";
        StoryEventText.color = new Color(StoryEventText.color.r, StoryEventText.color.g, StoryEventText.color.b, 0f);

        Color newColor;
        if (ColorUtility.TryParseHtmlString(currentEvent.BackgroundColor, out newColor))
        {
            StoryEventBackground.DOColor(newColor, 1f);
        }

        SoundManager.Instance.PlayOneShotSfx(currentEvent.SoundEffect, timeToDie: 15);
        SoundManager.Instance.PlayMusic(currentEvent.Music);

        yield return new WaitForSeconds(2f);
        var voice = LocalizationManager.Instance.GetVoice(currentEvent.DescriptionKey);
        if (CurrentAudioSource != null) CurrentAudioSource.Stop();
        CurrentAudioSource = SoundManager.Instance.PlayVoice(voice);
        StoryEventText.DOFade(1f, 7f).SetEase(Ease.OutSine);
        yield return new WaitForSeconds(5f);

        ShowingIntro = false;
    }

    public void NextCharacter()
    {
        CurrentSaintIndex = (CurrentSaintIndex + 1) % SaintsManager.Instance.UnlockedSaints.Count;
        UpdateSaint();
    }

    public void PreviousCharacter()
    {
        CurrentSaintIndex = (CurrentSaintIndex - 1);
        if (CurrentSaintIndex < 0) CurrentSaintIndex = SaintsManager.Instance.UnlockedSaints.Count - 1;
        UpdateSaint();
    }


    public void Close()
    {
        CustomEventPopup.IsDisplaying = false;
        UI.Instance.EnableAllUIElements(true);
        gameObject.SetActive(false);
    }
}
