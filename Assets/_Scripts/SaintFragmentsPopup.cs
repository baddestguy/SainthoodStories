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
        Proceed();
    }

    public void Proceed()
    {
        if (CurrentSequenceNumber >= EventList.Count())
        {
            StorySequenceObj.SetActive(false);
            CurrentSequenceNumber = 0;
            return;
        }

        var text = LocalizationManager.Instance.GetText(EventList.ElementAt(CurrentSequenceNumber).DescriptionKey);
        StoryEventText.text = text;
        StoryEventText.color = new Color(StoryEventText.color.r, StoryEventText.color.g, StoryEventText.color.b, 0f);
        StoryEventText.DOFade(1f, 0.5f).SetEase(Ease.Linear);
        CurrentSequenceNumber++;
        SoundManager.Instance.PlayOneShotSfx("Button_SFX");
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
