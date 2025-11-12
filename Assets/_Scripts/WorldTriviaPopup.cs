using System.Collections.Generic;
using System.Linq;
using TMPro;
using Unity.Collections.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.UI;

public class WorldTriviaPopup : MonoBehaviour
{
    public Image CharPotrait;
    public TextMeshProUGUI Description;
    public TextMeshProUGUI Title;
    public ScrollRect ScrollRect;
    public List<WorldTriviaData> Data;

    private int CurrentSaintIndex = 0;

    public void Open()
    {
        CustomEventPopup.IsDisplaying = true;
        UI.Instance.EnableAllUIElements(false);
        gameObject.SetActive(true);

        //Load/Refresh data
        Data = InventoryManager.Instance.WorldTrivia;

        UpdateText();
    }

    public void Close()
    {
        CustomEventPopup.IsDisplaying = false;
        UI.Instance.EnableAllUIElements(true);
        gameObject.SetActive(false);
    }

    void UpdateText()
    {
        if (!Data.Any()) 
        {
            Description.text = LocalizationManager.Instance.GetText("No_Saints_Text");
            return;
        }

        ScrollRect.verticalNormalizedPosition = 1f;
        Title.text = LocalizationManager.Instance.GetText(Data[CurrentSaintIndex].NameKey);
        Description.text = LocalizationManager.Instance.GetText(Data[CurrentSaintIndex].DescriptionKey);
        CharPotrait.sprite = Resources.Load<Sprite>(Data[CurrentSaintIndex].IconPath);
    }

    public void NextCharacter()
    {
        if (!Data.Any()) return;

        CurrentSaintIndex = (CurrentSaintIndex + 1) % Data.Count;
        UpdateText();
    }

    public void PreviousCharacter()
    {
        if (!Data.Any()) return;
 
        CurrentSaintIndex = (CurrentSaintIndex - 1);
        if (CurrentSaintIndex < 0) CurrentSaintIndex = Data.Count - 1;
        UpdateText();
    }
}
