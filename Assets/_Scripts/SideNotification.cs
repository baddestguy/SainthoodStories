using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SideNotification : MonoBehaviour
{
    public Image BuildingIcon;
    public TextMeshProUGUI ItemsRequiredDisplay;
    public TextMeshProUGUI DeadlineDisplay;

    public void Init(string sprite, int items, GameClock deadline)
    {

        BuildingIcon.sprite = Resources.Load<Sprite>($"Icons/{sprite}");
        ItemsRequiredDisplay.text = $"{(items > 0 ? items.ToString() : "")}";
        DeadlineDisplay.text = "";

        if (deadline != null) 
        {
            if(deadline.Time != -1)
                DeadlineDisplay.text = $"{(int)deadline.Time}:{(deadline.Time % 1 == 0 ? "00" : "30")}";
            GameClock clock = GameManager.Instance.GameClock;
            if (clock.TimeDifference(deadline) <= 1.5)
            {
                DeadlineDisplay.color = Color.red;
            }
            else
            {
                DeadlineDisplay.color = Color.white;
            }
        }
    }
}
