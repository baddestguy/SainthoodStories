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
        CamTransform = ExteriorCamera.Instance.Camera.transform;
    }

    public void Init(string iconName, int openTime, int closingTime, int hearts, bool isOpen)
    {
        BuildingIcon.sprite = Resources.Load<Sprite>($"Icons/{iconName}");
        OpenHours.text = $"{openTime}:{(openTime % 1 == 0 ? "00" : "30")} - {closingTime}:{(closingTime % 1 == 0 ? "00" : "30")}";
        Hearts.text = $"{hearts}";

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
            OpenHours.text = LocalizationManager.Instance.GetText("UI_OpenPopup");
        }

        IsCurrentlyOpen.text = isOpen ? LocalizationManager.Instance.GetText("UI_Open") : LocalizationManager.Instance.GetText("UI_Closed"); ;
    }

    public void UpdateReadyForConstruction()
    {
        IsCurrentlyOpen.text = LocalizationManager.Instance.GetText("UI_ConstructionReady");
    }

    void Update()
    {
        transform.forward = CamTransform.forward;
    }
}
