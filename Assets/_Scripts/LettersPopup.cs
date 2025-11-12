using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class LettersPopup : MonoBehaviour
{
    public Image CharPotrait;
    public TextMeshProUGUI Letter;
    public TextMeshProUGUI CharName;
    public ScrollRect ScrollRect;
    public List<LetterData> Data;

    public void Open()
    {
        CustomEventPopup.IsDisplaying = true;
        UI.Instance.EnableAllUIElements(false);
        gameObject.SetActive(true);

        //Load/Refresh data
        Data = InventoryManager.Instance.Letters;
    }

    public void Close()
    {
        CustomEventPopup.IsDisplaying = false;
        UI.Instance.EnableAllUIElements(true);
        gameObject.SetActive(false);
    }

    public void NextCharacter()
    {
        ScrollRect.verticalNormalizedPosition = 1f;

    }

    public void PreviousCharacter()
    {
        ScrollRect.verticalNormalizedPosition = 1f;

    }
}
