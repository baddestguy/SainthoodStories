using System.Collections;
using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static System.Net.Mime.MediaTypeNames;
using Application = UnityEngine.Application;
using Image = UnityEngine.UI.Image;

public class GameFlow : MonoBehaviour
{
    [SerializeField] private TerminalController terminal;

    private int hubSelectionIndex = 0;
    private SaintData hubSelectedSaint;

    private int ChoiceSelectionIndex = 0;
    private SaintsEvent SelectedChoiceEvent;

    public GameObject StorySequenceObj;
    public TextMeshProUGUI StoryEventText;
    public Image StoryEventBackground;
    private int CurrentSaintIndex = 0;
    private int CurrentSequenceNumber = 2;
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
    
    private bool InChoiceMode;
    private int NumChoices = 0;

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

        SoundManager.Instance.PlayOneShotSfx("PC_Bootup_SFX", timeToDie: 20);
        SoundManager.Instance.PlayAmbience("Office_Ambience");

        yield return terminal.TypeLine("S A I N T H O O D   A R C H I V E   T E R M I N A L");
        yield return terminal.TypeLine("ECCLESIA BIOS v2.13");
        yield return terminal.TypeLine("Copyright (C) 1994 Ecclesia Systems");
        yield return terminal.TypeLine("All Rights Reserved.");
        yield return terminal.TypeLine("");

        yield return terminal.TypeLine("CPU ............... 486DX/66 MHz");
        yield return terminal.TypeLine("FPU ............... DETECTED");
        yield return terminal.TypeLine("BASE MEMORY ....... 640 KB");
        yield return terminal.TypeLine("EXTENDED MEMORY ... 8192 KB");
        yield return terminal.TypeLine("CACHE ............ ENABLED");
        yield return terminal.TypeLine("");

        yield return terminal.TypeLine("Loading system drivers...");
        yield return terminal.TypeLine("HIMEM.SYS ............... OK");
        yield return terminal.TypeLine("EMM386.EXE .............. OK");
        yield return terminal.TypeLine("ANSI.SYS ................ OK");
        yield return terminal.TypeLine("KEYB US ................ OK");
        yield return terminal.TypeLine("");

        yield return terminal.TypeLine("DRIVE C: ................ OK");
        yield return terminal.TypeLine("FS TYPE ................ FAT16");
        yield return terminal.TypeLine("VOLUME LABEL ............ ECCLESIA_ARCHIVE");
        yield return terminal.TypeLine("");

        yield return terminal.TypeLine("MOUNTING: C:\\ARCHIVE\\SAINTS\\");
        yield return terminal.TypeLine("INDEXING RECORDS ........ PLEASE WAIT");

        yield return terminal.TypeLine($"RECORDS FOUND ........... {SaintsManager.Instance.UnlockedSaints.Count}");
        yield return terminal.TypeLine("VERIFICATION ............ COMPLETE");
        yield return terminal.TypeLine("");

        yield return terminal.TypeLine("NOTICE: READ-ONLY SESSION");
        yield return terminal.TypeLine("MODIFICATION DISABLED");
        yield return terminal.TypeLine("DELETION DISABLED");
        yield return terminal.TypeLine("EXPORT DISABLED");
        yield return terminal.TypeLine("");

        yield return terminal.TypeLine("USER ............ ADMIN");
        SoundManager.Instance.PlayMusic("Office_Drone");

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
                    SoundManager.Instance.PlayOneShotSfx($"Key_Tap_{Random.Range(1,5)}");
                    hubSelectionIndex = Mathf.Max(0, hubSelectionIndex - 1);
                    break;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    SoundManager.Instance.PlayOneShotSfx($"Key_Tap_{Random.Range(1,5)}");
                    hubSelectionIndex = Mathf.Min(unlocked.Count - 1, hubSelectionIndex + 1);
                    break;
                }
                if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space))
                {
                    SoundManager.Instance.PlayOneShotSfx($"Key_Tap_{Random.Range(1,5)}");
                    hubSelectedSaint = unlocked[hubSelectionIndex];
                    chosen = true;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    // ESC = exit game
                    SoundManager.Instance.PlayOneShotSfx($"Key_Tap_{Random.Range(1,5)}");
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

        terminal.AppendLineInstant("");
        terminal.AppendLineInstant("UP/DOWN: NAVIGATE   ENTER: SELECT   ESC: EXIT");
    }

    // ---------------- READING ----------------
    private IEnumerator ReadSaint(SaintData saint)
    {
        terminal.Clear();
        CurrentSequenceNumber = 2;
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
        yield return terminal.WaitForContinue("Press Enter to begin, Q to quit any time...");

        terminal.Clear();
        while(CurrentSequenceNumber < EventList.Count() && !Input.GetKeyUp(KeyCode.Q) && !Input.GetKeyUp(KeyCode.Escape))
        {
            var currentEvent = EventList.ElementAt(CurrentSequenceNumber);
            yield return StartCoroutine(Proceed(currentEvent));
            
            CurrentSequenceNumber++;
            
            yield return terminal.WaitForContinue(">");

            while (InChoiceMode) yield return null;

            if (CurrentSequenceNumber >= EventList.Count())
            {
                yield return terminal.TypeLine("");
                yield return terminal.TypeLine("END OF RECORD.");
                yield return terminal.WaitForContinue("[RETURN TO DIRECTORY]");
                yield break;
            }
        }
    }

    IEnumerator Proceed(SaintsEvent currentEvent)
    {
        if (ShowingIntro && !CanSkipIntro) yield break;


    //    WeatherManager.Instance.UpdateWeather(currentEvent.Weather);

        if (currentEvent.SequenceType == StorySequenceType.CHOICE)
        {
            terminal.Clear();
            InChoiceMode = true;
            var text = LocalizationManager.Instance.GetText(currentEvent.DescriptionKey);
            yield return terminal.TypeLine($"{text}");
            StartCoroutine(ChoiceSequence(currentEvent));
        }
        else
        {
            var text = LocalizationManager.Instance.GetText(currentEvent.DescriptionKey);
            yield return terminal.TypeLine($"{text}");
        }

        var voice = LocalizationManager.Instance.GetVoice(currentEvent.DescriptionKey);
        if (CurrentAudioSource != null) CurrentAudioSource.Stop();
        CurrentAudioSource = SoundManager.Instance.PlayVoice(voice);

        //if (currentEvent.SoundEffect == "STOP")
        //    SoundManager.Instance.StopOneShotSfx();
        //else
        //    SoundManager.Instance.PlayOneShotSfx(currentEvent.SoundEffect, timeToDie: 15);

        //if (currentEvent.Music == "STOP")
        //    SoundManager.Instance.FadeMusic(0, SoundManager.Instance.MusicAudioSourceChannel1);
        //else
        //    SoundManager.Instance.PlayMusic(currentEvent.Music);

        //if (currentEvent.Ambience == "STOP")
        //    SoundManager.Instance.FadeAmbience(0, true);
        //else
        //    SoundManager.Instance.PlayAmbience(currentEvent.Ambience);

        //if (!string.IsNullOrEmpty(currentEvent.InteractionSfx))
        //    InteractionSfx = currentEvent.InteractionSfx.Split(',');
    }

    void RenderChoices(SaintsEvent currentEvent)
    {
        terminal.Clear();
        var text = LocalizationManager.Instance.GetText(currentEvent.DescriptionKey);
        terminal.AppendLineInstant($"{text}");

        string cursor = " ";
        NumChoices = 0;

        if (!string.IsNullOrEmpty(currentEvent.Choice1))
        {
            cursor = ChoiceSelectionIndex == 0 ? ">" : " ";
            terminal.AppendLineInstant($"{cursor} {LocalizationManager.Instance.GetText(currentEvent.Choice1)}");
            NumChoices++;
        }

        if (!string.IsNullOrEmpty(currentEvent.Choice2))
        {
            cursor = ChoiceSelectionIndex == 1 ? ">" : " ";
            terminal.AppendLineInstant($"{cursor} {LocalizationManager.Instance.GetText(currentEvent.Choice2)}");
            NumChoices++;
        }

        if (!string.IsNullOrEmpty(currentEvent.Choice3))
        {
            cursor = ChoiceSelectionIndex == 2 ? ">" : " ";
            terminal.AppendLineInstant($"{cursor} {LocalizationManager.Instance.GetText(currentEvent.Choice3)}");
            NumChoices++;
        }

        {
            cursor = ChoiceSelectionIndex == NumChoices ? ">" : " ";
            terminal.AppendLineInstant($"{cursor} {LocalizationManager.Instance.GetText("Skip and proceed with story.")}");
            NumChoices++;
        }

        terminal.AppendLineInstant("");
        terminal.AppendLineInstant("UP/DOWN: NAVIGATE   ENTER: SELECT   ESC: EXIT");
    }

    private IEnumerator ChoiceSequence(SaintsEvent currentEvent)
    {
        yield return new WaitForSeconds(0.1f);

        SelectedChoiceEvent = null;
        bool chosen = false;

        while (!chosen)
        {
            RenderChoices(currentEvent);

            while (true)
            {
                if (Input.GetKeyDown(KeyCode.UpArrow))
                {
                    SoundManager.Instance.PlayOneShotSfx($"Key_Tap_{Random.Range(1,5)}");
                    ChoiceSelectionIndex = Mathf.Max(0, ChoiceSelectionIndex - 1);
                    break;
                }
                if (Input.GetKeyDown(KeyCode.DownArrow))
                {
                    SoundManager.Instance.PlayOneShotSfx($"Key_Tap_{Random.Range(1,5)}");
                    ChoiceSelectionIndex = Mathf.Min(NumChoices - 1, ChoiceSelectionIndex + 1);
                    break;
                }
                if (Input.GetKeyUp(KeyCode.Return) || Input.GetKeyUp(KeyCode.Space))
                {
                    SoundManager.Instance.PlayOneShotSfx($"Key_Tap_{Random.Range(1,5)}");
                    ChoiceSelected(currentEvent, ChoiceSelectionIndex);
                    chosen = true;
                    break;
                }
                if (Input.GetKeyDown(KeyCode.Escape))
                {
                    SoundManager.Instance.PlayOneShotSfx($"Key_Tap_{Random.Range(1,5)}");
                    SelectedChoiceEvent = null;
                    chosen = true;
                    break;
                }

                yield return null;
            }

            yield return null;
        }
    }

    private void ChoiceSelected(SaintsEvent currentEvent, int selection)
    {
        var responseKey = "";
        switch (selection)
        {
            case 0: responseKey = currentEvent.Choice1Response; break;
            case 1: responseKey = currentEvent.Choice2Response; break;
            case 2: responseKey = currentEvent.Choice3Response; break;
        }

        if(selection == NumChoices-1)
        {
            InChoiceMode = false;
            return;
        }

        var newevent = GameDataManager.Instance.SaintsEvent[responseKey.Trim().Trim('"')];
        StartCoroutine(Proceed(newevent));
    }

    // ---------------- END ----------------
    private IEnumerator EndSequence()
    {
        terminal.Clear();
        yield return terminal.TypeLine("SESSION TERMINATED.");
        yield return terminal.TypeLine("POWERING DOWN...");
        yield return terminal.WaitForContinue("Press Enter to Continue...");
        Application.Quit();
    }
}
