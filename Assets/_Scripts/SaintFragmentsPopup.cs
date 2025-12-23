using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using Random = UnityEngine.Random;

public class SaintFragmentsPopup : MonoBehaviour, IDragHandler, IEndDragHandler
{
    public Dictionary<SaintID, List<SaintFragmentData>> Data;
    public Image CharPotrait;
    public TextMeshProUGUI Fragment;
    public TextMeshProUGUI SaintName;
    public Image Divider;

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

    private string[] InteractionSfx;
    public RectTransform RipplePrefab;
    [Header("Ripple Settings")]
    public float RippleDuration = 0.3f;
    public float RippleMaxScale = 1.5f;
    public float RippleStartAlpha = 0.35f;

    public GameObject ProceedBtn;
    public GameObject CloseStoryBtn;
    public ScrollRect ChoiceScroller;
    public GameObject ChoicePrefab;

    [Header("Swipe Settings")]
    public float SwipeThreshold = 100f;
    private Vector2 DragStartPos;


    public void Open()
    {
        CustomEventPopup.IsDisplaying = true;
        UI.Instance.EnableAllUIElements(false);
        gameObject.SetActive(true);
        UI.Instance.GetComponent<Canvas>().worldCamera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = false;

        Data = InventoryManager.Instance.SaintFragments;

        UpdateSaint();
        //var xPos = transform.localPosition.x;
        //transform.DOLocalMoveX(-300, 0);
        //transform.DOLocalMoveX(xPos, 0.5f);
    }

    private void UpdateSaint()
    {
        if (!SaintsManager.Instance.UnlockedSaints.Any())
        {
            return;
        }

        SaintData saintData = SaintsManager.Instance.UnlockedSaints[CurrentSaintIndex];

        //populate the saint data
        var rawPath = saintData.IconPath;

        // Strip whitespace, CR/LF, and stray quotes
        var cleanPath = rawPath?
            .Trim()    
            .Trim('"');

        CharPotrait.enabled = true;
        CharPotrait.sprite = Resources.Load<Sprite>(cleanPath);
        CharPotrait.transform.localPosition = new Vector3(-10f, CharPotrait.transform.localPosition.y, CharPotrait.transform.localPosition.z);
        CharPotrait.transform.DOLocalMoveX(0, 0.5f);
        CharPotrait.DOFade(0, 0);
        CharPotrait.DOFade(1, 0.5f);

        SaintName.text = $"<b>{LocalizationManager.Instance.GetText("Name")}:</b> {saintData.Name}\r\n<b>{LocalizationManager.Instance.GetText("Born")}:</b> {saintData.Birthday}\r\n<b>{LocalizationManager.Instance.GetText("Died")}:</b> {saintData.Death}\r\n<b>{LocalizationManager.Instance.GetText("FeastDay")}:</b> {saintData.FeastDay}\r\n<b>{LocalizationManager.Instance.GetText("Patron")}:</b> {LocalizationManager.Instance.GetText(saintData.PatronKey)}";
        SaintName.transform.localPosition = new Vector3(-10f, SaintName.transform.localPosition.y, SaintName.transform.localPosition.z);
        SaintName.transform.DOLocalMoveX(0, 0.5f);
        SaintName.DOFade(0, 0);
        SaintName.DOFade(1, 0.5f);

        Divider.transform.DOScaleY(0, 0);
        Divider.transform.DOScaleY(1, 0.5f);
    }

    public void SelectSaint()
    {
        StorySequenceObj.SetActive(true);
        
        var currentSaintId = SaintsManager.Instance.UnlockedSaints[CurrentSaintIndex].Id;
        EventList = GameDataManager.Instance.SaintsEvent.Values.Where(e => e.Id.Contains(currentSaintId.ToString()));

        prevMusicVol = GameSettings.Instance.musicVolume;
        prevAmbiantVol = GameSettings.Instance.ambianceVolume;
        //GameSettings.Instance.SetVolume("Music", 0.5f);
        //GameSettings.Instance.SetVolume("Ambiance", 0.5f);
        //GameSettings.Instance.SetVolume("SFX", 0.5f);

        Proceed();
    }

    public void Interact()
    {
        var currentEvent = EventList.ElementAt(CurrentSequenceNumber-1); //getting minus 1 since it would have already increased at the end of the Proceed()
        if (InteractionSfx.Length == 0) return;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(UI.Instance.GetComponent<RectTransform>(), Input.mousePosition, UI.Instance.GetComponent<Canvas>().worldCamera, out Vector2 screenPos);

        // Play sound
        var sfx = InteractionSfx[Random.Range(0, InteractionSfx.Length)];
        SoundManager.Instance.PlayOneShotSfx(sfx, modifyPitch:true);

        // Spawn ripple
        RectTransform ripple = Instantiate(RipplePrefab, StorySequenceObj.transform);
        ripple.localPosition = screenPos;
        ripple.localScale = Vector3.zero;

        Image img = ripple.GetComponent<Image>();
        Extensions.TryExtractColorFromRichText(currentEvent.FontColor, out Color c);
        c.a = RippleStartAlpha;
        img.color = c;

        RippleDuration = SoundManager.Instance.OneShotSource.clip.length;

        ripple.DOScale(RippleMaxScale, RippleDuration).SetEase(Ease.OutCubic);
        img.DOFade(0f, RippleDuration).SetEase(Ease.OutCubic)
            .OnComplete(() => Destroy(ripple.gameObject));
    }

    public void Proceed()
    {
        if (ShowingIntro && !CanSkipIntro) return;

        ChoiceScroller.gameObject.SetActive(false);
        StoryEventText.alignment = TextAlignmentOptions.Midline;
        StopCoroutine("SaintIntro");

        if (CurrentSequenceNumber >= EventList.Count())
        {
            CloseStory();
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

        StoryEventText.DOKill();
        StoryEventText.DOFade(1f, 1f).SetEase(Ease.Linear);

        WeatherManager.Instance.UpdateWeather(currentEvent.Weather);

        Extensions.TryExtractColorFromRichText(currentEvent.FontColor, out Color c);
        ProceedBtn.GetComponent<Image>().color = c;
        CloseStoryBtn.GetComponent<Image>().color = c;
        
        if(currentEvent.SequenceType == StorySequenceType.CHOICE)
        {
            StartCoroutine(ChoiceSequence(currentEvent));
        }

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

        if (!string.IsNullOrEmpty(currentEvent.InteractionSfx))
            InteractionSfx = currentEvent.InteractionSfx.Split(',');

        Interact();
        CurrentSequenceNumber++;
    }

    IEnumerator SaintIntro()
    {
        ShowingIntro = true;
        CloseStoryBtn.SetActive(false);
        ProceedBtn.SetActive(false);
        SoundManager.Instance.StopOneShotSfx();
        SoundManager.Instance.FadeMusic(0, SoundManager.Instance.MusicAudioSourceChannel1);
        SoundManager.Instance.FadeAmbience(0, true);
        InteractionSfx = new string[0];

        var currentEvent = EventList.ElementAt(CurrentSequenceNumber);
        CurrentSequenceNumber++;

        var text = LocalizationManager.Instance.GetText(currentEvent.DescriptionKey);
        StoryEventText.text = $"{currentEvent.FontColor}{text}";
        StoryEventText.color = new Color(StoryEventText.color.r, StoryEventText.color.g, StoryEventText.color.b, 0f);
        WeatherManager.Instance.UpdateWeather(currentEvent.Weather);

        Extensions.TryExtractColorFromRichText(currentEvent.FontColor, out Color c);
        ProceedBtn.GetComponent<Image>().color = c;
        CloseStoryBtn.GetComponent<Image>().color = c;

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

        CloseStoryBtn.SetActive(true);
        ProceedBtn.SetActive(true);
        ShowingIntro = false;
    }

    IEnumerator ChoiceSequence(SaintsEvent currentEvent)
    {
        StoryEventText.alignment = TextAlignmentOptions.Top;

        foreach (var child in ChoiceScroller.transform.GetComponentsInChildren<SaintFragmentChoiceItem>().ToList())
        {
            Destroy(child.gameObject);
        }

        yield return new WaitForSeconds(0.1f);

        ChoiceScroller.gameObject.SetActive(true);
        if (!string.IsNullOrEmpty(currentEvent.Choice1))
        {
            var go = Instantiate(ChoicePrefab);
            go.transform.SetParent(ChoiceScroller.content, false);
            go.GetComponent<SaintFragmentChoiceItem>().Init(currentEvent, currentEvent.Choice1, currentEvent.Choice1Response);
        }

        yield return new WaitForSeconds(0.1f);
        if (!string.IsNullOrEmpty(currentEvent.Choice2))
        {
            var go = Instantiate(ChoicePrefab);
            go.transform.SetParent(ChoiceScroller.content, false);
            go.GetComponent<SaintFragmentChoiceItem>().Init(currentEvent, currentEvent.Choice2, currentEvent.Choice2Response);
        }

        yield return new WaitForSeconds(0.1f);
        if (!string.IsNullOrEmpty(currentEvent.Choice3))
        {
            var go = Instantiate(ChoicePrefab);
            go.transform.SetParent(ChoiceScroller.content, false);
            go.GetComponent<SaintFragmentChoiceItem>().Init(currentEvent, currentEvent.Choice3, currentEvent.Choice3Response);
        }
    }

    public void SelectFragmentChoice(SaintsEvent currentEvent)
    {
        var text = LocalizationManager.Instance.GetText(currentEvent.DescriptionKey);
        StoryEventText.text = $"{currentEvent.FontColor}{text}";
        StoryEventText.color = new Color(StoryEventText.color.r, StoryEventText.color.g, StoryEventText.color.b, 0f);
        StoryEventText.DOFade(1f, 1f).SetEase(Ease.Linear);

        Extensions.TryExtractColorFromRichText(currentEvent.FontColor, out Color c);
        ProceedBtn.GetComponent<Image>().color = c;
        CloseStoryBtn.GetComponent<Image>().color = c;

        if (currentEvent.SequenceType == StorySequenceType.CHOICE)
        {
            StartCoroutine(ChoiceSequence(currentEvent));
        }

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

        if (!string.IsNullOrEmpty(currentEvent.InteractionSfx))
            InteractionSfx = currentEvent.InteractionSfx.Split(',');

        Interact();
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

    public void CloseStory()
    {
        CurrentSequenceNumber = 0;
        GameSettings.Instance.SetVolume("Music", prevMusicVol);
        GameSettings.Instance.SetVolume("Ambiance", prevAmbiantVol);

        SoundManager.Instance.FadeMusic(0, SoundManager.Instance.MusicAudioSourceChannel1);
        if (DateTime.Now.Hour > 19 || DateTime.Now.Hour < 6)
        {
            SoundManager.Instance.PlayAmbience("SummerNight_Ambience");
        }
        else if (DateTime.Now.Hour >= 6)
        {
            SoundManager.Instance.PlayAmbience("SummerDay_Ambience");
        }

        ShowingIntro = false;
        StorySequenceObj.SetActive(false);
    }

    public void Close()
    {
        CustomEventPopup.IsDisplaying = false;
        UI.Instance.EnableAllUIElements(true);
        UI.Instance.GetComponent<Canvas>().worldCamera.GetComponent<UniversalAdditionalCameraData>().renderPostProcessing = true;
        gameObject.SetActive(false);
    }

    // Called while dragging
    public void OnDrag(PointerEventData eventData)
    {
        // Only record start once
        if (DragStartPos == Vector2.zero)
            DragStartPos = eventData.pressPosition;
    }

    // Called when touch/mouse is released
    public void OnEndDrag(PointerEventData eventData)
    {
        Vector2 dragEndPos = eventData.position;
        float deltaX = dragEndPos.x - DragStartPos.x;

        if (Mathf.Abs(deltaX) > SwipeThreshold)
        {
            if (deltaX > 0)
                NextCharacter();
            else
                PreviousCharacter();
        }

        DragStartPos = Vector2.zero; // reset
    }
}
