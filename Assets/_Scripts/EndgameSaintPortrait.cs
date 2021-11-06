using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class EndgameSaintPortrait : MonoBehaviour
{
    public Image BG;
    public Image Saint;
    public GameObject HoverObj;
    public TextMeshProUGUI SaintName;

    public void OnHoverEnter()
    {
        HoverObj.SetActive(true);
    }

    public void OnHoverExit()
    {
        HoverObj.SetActive(false);
    }
}
