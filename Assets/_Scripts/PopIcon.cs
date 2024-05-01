using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PopIcon : MonoBehaviour
{
    public Image BuildingIcon;
    public TextMeshProUGUI ItemsRequiredDisplay;
    public TextMeshProUGUI DeadlineDisplay;
    public GameObject ClockIcon;

    private Transform CamTransform;
    private bool PoppedUI;

    private void Start()
    {
        CamTransform = ExteriorCamera.Instance.Camera.transform;
    }

    public void Init(string sprite, int items, GameClock deadline)
    {
        if (PoppedUI)
        {
            gameObject.SetActive(false);
            return;
        }

        BuildingIcon.sprite = Resources.Load<Sprite>($"Icons/{sprite}");
        ItemsRequiredDisplay.text = $"{items}";
        DeadlineDisplay.text = $"{(int)deadline.Time}:{(deadline.Time % 1 == 0 ? "00" : "30")}";

    //    ItemsRequiredDisplay.gameObject.SetActive(items > 0);
   //     DeadlineDisplay.gameObject.SetActive(deadline.Time >= 0);
    //    ClockIcon.SetActive(deadline.Time >= 0);

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

    public void UIPopped(bool active)
    {
        PoppedUI = active;
        if (!active) return;

        gameObject.SetActive(false);
    //    BuildingIcon.transform.localPosition += new Vector3(0, active ? 1 : -1, 0);
    //    ItemsRequiredDisplay.transform.localPosition += new Vector3(0, active ? 1 : -1, 0);
    }

    void Update()
    {
        transform.forward = CamTransform.forward;
    }
}
