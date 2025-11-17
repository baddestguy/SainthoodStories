using System.Collections.Generic;
using System.Linq;
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

    private int CurrentSaintIndex = 0;

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
