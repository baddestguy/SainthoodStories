using System.Collections;
using System.Collections.Generic;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEditor.Overlays;
using UnityEngine;
using UnityEngine.UI;

public class GameFlow : MonoBehaviour
{
    [SerializeField] private TerminalController terminal;

    private int hubSelectionIndex = 0;
    private SaintData hubSelectedSaint;

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

    private void Start()
    {
        StartCoroutine(RunGame());
    }

    private IEnumerator RunGame()
    {
        yield return BootSequence();
        while (true)
        {
            var unlocked = SaintsManager.Instance.UnlockedSaints;
            if (unlocked.Count == 0)
            {
                yield return EndSequence();
                yield break;
            }

            yield return StartCoroutine(HubSelection(unlocked));

            var selected = hubSelectedSaint;

            if (selected == null)
            {
                yield return EndSequence();
                yield break;
            }

            yield return ReadSaint(selected);
        }
    }

    // ---------------- BOOT ----------------
    private IEnumerator BootSequence()
    {
        terminal.Clear();

        yield return terminal.TypeLine("S A I N T H O O D   A R C H I V E   T E R M I N A L");
        yield return terminal.TypeLine("BIOS v2.13  (c) 1994  ECCLESIA SYSTEMS");
        yield return terminal.TypeLine("MEM CHECK ............... OK");
        yield return terminal.TypeLine("DRIVE C: ................. OK");
        yield return terminal.TypeLine("MOUNTING: C:\\ARCHIVE\\SAINTS\\");
        yield return terminal.TypeLine("");
        yield return terminal.TypeLine("NOTICE: READ-ONLY SESSION");
        yield return terminal.TypeLine("USER: AUTHORIZED VIEWER");
        yield return terminal.TypeLine("ACCESS: TESTIMONY RECORDS");
        yield return terminal.WaitForContinue();
    }

    // ---------------- HUB ----------------
    private IEnumerator HubSelection(List<SaintData> unlocked)
    {
        yield return new WaitForSeconds(0.1f);

        hubSelectedSaint = null;
        bool chosen = false;

        while (!chosen)
        {
            RenderHub(unlocked);

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    hubSelectionIndex = Mathf.Max(0, hubSelectionIndex - 1);
                    break;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    hubSelectionIndex = Mathf.Min(unlocked.Count - 1, hubSelectionIndex + 1);
                    break;
                }
                if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space))
                {
                    hubSelectedSaint = unlocked[hubSelectionIndex];
                    chosen = true;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    // ESC = exit game
                    hubSelectedSaint = null;
                    chosen = true;
                    break;
                }

                yield return null;
            }

            yield return null;
        }
    }

    private void RenderHub(List<SaintData> unlocked)
    {
        terminal.Clear();

        terminal.AppendLineInstant(@"C:\ARCHIVE\SAINTS\");
        terminal.AppendLineInstant("");
        terminal.AppendLineInstant("DIR /W");
        terminal.AppendLineInstant("");

        // show unlocked
        for (int i = 0; i < unlocked.Count; i++)
        {
            var s = unlocked[i];

            string cursor = (i == hubSelectionIndex) ? ">" : " ";
            string ext = ".LOG"; // simple flavor
            terminal.AppendLineInstant($"{cursor} {s.Id}{ext}");
            //if (!string.IsNullOrWhiteSpace(s.Preview))
            //    terminal.AppendLineInstant($"   {s.Preview}");
        }

        // show locked count
        int lockedCount = SaintsManager.Instance.UnlockedSaints.Count - unlocked.Count;
        if (lockedCount > 0)
        {
            terminal.AppendLineInstant("");
            terminal.AppendLineInstant($"{lockedCount} FILE(S) LOCKED. CONDITION: READ MORE TESTIMONIES.");
        }

        terminal.AppendLineInstant("");
        terminal.AppendLineInstant("UP/DOWN: SELECT   ENTER: OPEN   ESC: EXIT");
    }

    // ---------------- READING ----------------
    private IEnumerator ReadSaint(SaintData saint)
    {
        terminal.Clear();
        var currentSaintId = SaintsManager.Instance.UnlockedSaints[hubSelectionIndex].Id;
        EventList = GameDataManager.Instance.SaintsEvent.Values.Where(e => e.Id.Contains(currentSaintId.ToString()));

        // header
        yield return terminal.TypeLine($"OPENING FILE: {saint.Id}.LOG");
        yield return terminal.TypeLine("----------------------------------------");
        yield return terminal.TypeLine($"{LocalizationManager.Instance.GetText("Name")}: {saint.Name}");
        yield return terminal.TypeLine($"{LocalizationManager.Instance.GetText("Born")}: {saint.Birthday}");
        yield return terminal.TypeLine($"{LocalizationManager.Instance.GetText("Died")}: {saint.Death}");
        yield return terminal.TypeLine($"{LocalizationManager.Instance.GetText("FeastDay")}: {saint.FeastDay}");
        yield return terminal.TypeLine($"{LocalizationManager.Instance.GetText("Patronage")}: {LocalizationManager.Instance.GetText(saint.PatronKey)}");
        yield return terminal.TypeLine("----------------------------------------");
        yield return terminal.WaitForContinue("Press Q to quit any time, Enter to begin...");

        while(CurrentSequenceNumber < EventList.Count())
        {
            yield return StartCoroutine(Proceed());
            yield return terminal.WaitForContinue("");
        }
    }

    IEnumerator Proceed()
    {
        if (ShowingIntro && !CanSkipIntro) yield break;

        //ChoiceScroller.gameObject.SetActive(false);
        //StoryEventText.alignment = TextAlignmentOptions.Midline;
        StopCoroutine("SaintIntro");

        if (CurrentSequenceNumber >= EventList.Count())
        {
            yield return terminal.TypeLine("");
            yield return terminal.TypeLine("END OF RECORD.");
            yield return terminal.WaitForContinue("[RETURN TO DIRECTORY]");
            yield break;
        }

        var currentEvent = EventList.ElementAt(CurrentSequenceNumber);

        var text = LocalizationManager.Instance.GetText(currentEvent.DescriptionKey);
        yield return terminal.TypeLine($"{text}");

        //StoryEventText.text = $"{currentEvent.FontColor}{text}";
        //StoryEventText.color = new Color(StoryEventText.color.r, StoryEventText.color.g, StoryEventText.color.b, 0f);

        //StoryEventText.DOKill();
        //StoryEventText.DOFade(1f, 1f).SetEase(Ease.Linear);

        WeatherManager.Instance.UpdateWeather(currentEvent.Weather);

        //Extensions.TryExtractColorFromRichText(currentEvent.FontColor, out Color c);
        //ProceedBtn.GetComponentsInChildren<Image>()[1].color = c;
        //CloseStoryBtn.GetComponent<Image>().color = c;

        if (currentEvent.SequenceType == StorySequenceType.CHOICE)
        {
            StartCoroutine(ChoiceSequence(currentEvent));
        }

        var voice = LocalizationManager.Instance.GetVoice(currentEvent.DescriptionKey);
        if (CurrentAudioSource != null) CurrentAudioSource.Stop();
        CurrentAudioSource = SoundManager.Instance.PlayVoice(voice);

        //Color newColor;
        //if (ColorUtility.TryParseHtmlString(currentEvent.BackgroundColor, out newColor))
        //{
        //    StoryEventBackground.DOColor(newColor, 1f);
        //}

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

        CurrentSequenceNumber++;
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
    private string Redact(string text)
    {
        // crude redaction effect: replace letters with ? except spaces/punct
        char RedactChar(char c)
        {
            if (char.IsWhiteSpace(c)) return c;
            if (char.IsPunctuation(c)) return c;
            return '?';
        }

        var arr = text.ToCharArray();
        for (int i = 0; i < arr.Length; i++)
            arr[i] = RedactChar(arr[i]);

        return new string(arr);
    }

    // ---------------- END ----------------
    private IEnumerator EndSequence()
    {
        terminal.Clear();
        yield return terminal.TypeLine("NO FURTHER RECORDS ARE AVAILABLE.");
        yield return terminal.TypeLine("");
        yield return terminal.TypeLine("SESSION TERMINATED.");
        yield return terminal.TypeLine("POWERING DOWN...");
        yield return terminal.WaitForContinue("[PRESS ENTER]");
        Application.Quit();
    }
}
