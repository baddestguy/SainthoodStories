using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BuildingInformationPopup : MonoBehaviour
{
    private Transform CamTransform;
    public Image BuildingIcon;
    public TextMeshProUGUI OpenHours;
    public TextMeshProUGUI Hearts;
    public GameObject HeartsGO;
    public TextMeshProUGUI IsCurrentlyOpen;

    void Start()
    {
        CamTransform = Camera.main.transform;
    }

    public void Init(string iconName, int openTime, int closingTime, int hearts, bool isOpen)
    {
        BuildingIcon.sprite = Resources.Load<Sprite>($"Icons/{iconName}");
        OpenHours.text = $"{openTime}:{(openTime % 1 == 0 ? "00" : "30")} - {closingTime}:{(closingTime % 1 == 0 ? "00" : "30")}";
        Hearts.text = $"+{hearts}";

        if(hearts <= 0)
        {
            BuildingIcon.transform.localPosition = new Vector3(BuildingIcon.transform.localPosition.x, 0.3f, 0);
            OpenHours.transform.localPosition = new Vector3(OpenHours.transform.localPosition.x, -0.7f, 0);
            Hearts.gameObject.SetActive(false);
            HeartsGO.SetActive(false);
        }
        else
        {
            BuildingIcon.transform.localPosition = new Vector3(BuildingIcon.transform.localPosition.x, 0.45f, 0);
            OpenHours.transform.localPosition = new Vector3(OpenHours.transform.localPosition.x, -0.4f, 0);
            Hearts.gameObject.SetActive(true);
            HeartsGO.SetActive(true);
        }

        if(closingTime - openTime >= 23)
        {
            OpenHours.text = LocalizationManager.Instance.GetText("Open 24 hrs");
        }

        IsCurrentlyOpen.text = isOpen ? "(Open)" : "(Closed)";
    }

    void Update()
    {
        transform.forward = CamTransform.forward;
    }
}
