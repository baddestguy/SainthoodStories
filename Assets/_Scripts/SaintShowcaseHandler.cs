using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SaintShowcaseHandler : MonoBehaviour
{

    public Image SaintPotrait;
    public TextMeshProUGUI Bio;
    public TextMeshProUGUI Title;    

    private int CurrentSaintIndex = 0;

    private void Start()
    {
        ShowPanel();
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
        
        SaintData saintData = SaintsManager.Instance.UnlockedSaints[CurrentSaintIndex];

        //populate the saint data
        SaintPotrait.enabled = true;
        SaintPotrait.sprite = Resources.Load<Sprite>(saintData.IconPath);
        Bio.text = LocalizationManager.Instance.GetText(saintData.BioKey);
        Title.text = saintData.Name;
    }

    public void OnExit()
    {
        SoundManager.Instance.EndAllTracks();
        //Load game data after saits scene is exited.. Modify to what is expected
        SaveDataManager.Instance.LoadGame((data, newgame) => {
            GameManager.Instance.SaveData = data;
            GameManager.Instance.CurrentMission = new Mission(data.FP, data.FPPool, data.CP, data.Energy, data.Time, data.Day, data.Week);
            TutorialManager.Instance.CurrentTutorialStep = data.TutorialSteps;
            GameSettings.Instance.FTUE = false;
        }, false, true);
        GameManager.Instance.LoadScene(MissionManager.Instance.CurrentMission.SeasonLevel, UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    [System.Serializable]
    public struct SaintsProp
    {
        public SaintID ID;
        public Sprite image;
    }
}
