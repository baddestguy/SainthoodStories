using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class SaintShowcaseHandler : MonoBehaviour
{
    public static SaintShowcaseHandler Instance { get; private set; }
    public Image SaintPotrait;
    public Text Bio;
    public Text Title;
    public ScrollRect ScrollRect;

    private int CurrentSaintIndex = 0;

    private void Awake()
    {
        Instance = this;
    }

    private void OnEnable()
    {
        CustomEventPopup.IsDisplaying = true;

        ToolTipManager.Instance.ShowToolTip("");
        TooltipMouseOver.IsHovering = false;

        UI.Instance.EnableAllUIElements(false);
        ShowPanel();
    }

    private void OnDisable()
    {
        CustomEventPopup.IsDisplaying = false;
        UI.Instance.EnableAllUIElements(true);
    }

    void Update()
    {
        if (gameObject.activeSelf)
        {
            if (Input.GetKeyDown(KeyCode.RightArrow))
            {
                ShowNextSaint();
            }else 
            if (Input.GetKeyDown(KeyCode.LeftArrow))
            {
                ShowPreviusSaint();
            }
        }
    }

    public void ShowPanel()
    {
        //SaintData saintData = SaintsManager.Instance.UnlockedSaints[currentSaintIndex];
        UpdateSaint();
    }

    public void ShowNextSaint()
    {
        if (!SaintsManager.Instance.UnlockedSaints.Any()) return;
    
        CurrentSaintIndex = (CurrentSaintIndex + 1) % SaintsManager.Instance.UnlockedSaints.Count;
        //SaintData saintData = SaintsManager.Instance.UnlockedSaints[currentSaintIndex];
        UpdateSaint();
    }

    public void ShowPreviusSaint()
    {
        if (!SaintsManager.Instance.UnlockedSaints.Any()) return;

        CurrentSaintIndex = (CurrentSaintIndex - 1);
        if (CurrentSaintIndex < 0) CurrentSaintIndex = SaintsManager.Instance.UnlockedSaints.Count - 1;
        UpdateSaint();
    }

    private void UpdateSaint()
    {
        if (!SaintsManager.Instance.UnlockedSaints.Any())
        {
            Bio.text = LocalizationManager.Instance.GetText("No_Saints_Text");
            return;
        }
        
        ScrollRect.verticalNormalizedPosition = 1f;
        SaintData saintData = SaintsManager.Instance.UnlockedSaints[CurrentSaintIndex];

        //populate the saint data
        SaintPotrait.enabled = true;
        SaintPotrait.sprite = Resources.Load<Sprite>(saintData.IconPath);
        Bio.text = LocalizationManager.Instance.GetText(saintData.BioKey);
        Title.text = saintData.Name;
    }

    public void OnExit()
    {
        gameObject.SetActive(false);
    }

    [System.Serializable]
    public struct SaintsProp
    {
        public SaintID ID;
        public Sprite image;
    }
}
