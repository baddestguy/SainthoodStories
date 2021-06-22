using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class TutorialPopup : MonoBehaviour
{
    public TextMeshProUGUI Text;
    public Image Image;

    public void Display(string locKey)
    {
        Player.LockMovement = true;
        Text.text = LocalizationManager.Instance.GetText(locKey);
        CustomEventPopup.IsDisplaying = true;
        switch (locKey) 
        {
            case "Tutorial_Instruction_1":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_1");
                break;

            case "Tutorial_Instruction_2":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_2");
                break;

            case "Tutorial_Instruction_3":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_3");
                break;

            case "Tutorial_Instruction_4":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_4");
                break;

            case "Tutorial_Instruction_5":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_5");
                break;

            case "Tutorial_Instruction_6":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_6");
                break;

            case "Tutorial_Instruction_7":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_7");
                break;
            case "Tutorial_Instruction_8":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_8");
                break;
            case "Tutorial_Instruction_9":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_9");
                break;
            case "Tutorial_Instruction_10":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_10");
                break;
            case "Tutorial_Instruction_11":
                Image.sprite = Resources.Load<Sprite>("Icons/Tut_11");
                break;
        }

    }

    public void Okay()
    {
        if (Text.isTextOverflowing && Text.textInfo.pageCount > Text.pageToDisplay)
        {
            Text.pageToDisplay++;
            return;
        }

        CustomEventPopup.IsDisplaying = false;
        gameObject.SetActive(false);
        Player.LockMovement = false;
        UI.Instance.TutorialPopupOff();
        TutorialManager.Instance.ShowTutorialArrow();
        Text.pageToDisplay = 1;
    }
}
