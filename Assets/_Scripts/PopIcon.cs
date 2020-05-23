using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopIcon : MonoBehaviour
{
    public Image BuildingIcon;
    public TextMeshProUGUI ItemsRequiredDisplay;
    public TextMeshProUGUI DeadlineDisplay;

    private Transform CamTransform;

    private void Start()
    {
        CamTransform = Camera.main.transform;
    }

    public void Init(string sprite, int items, GameClock deadline)
    {
        BuildingIcon.sprite = Resources.Load<Sprite>($"Icons/{sprite}");
        ItemsRequiredDisplay.text = $"{items}";
        DeadlineDisplay.text = $"{(int)deadline.Time}:{(deadline.Time % 1 == 0 ? "00" : "30")}";

        if (items <= 0)
        {
            ItemsRequiredDisplay.gameObject.SetActive(false);
        }
        else
        {
            ItemsRequiredDisplay.gameObject.SetActive(true);
        }

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

    void Update()
    {
        transform.forward = CamTransform.forward;
    }
}
