using System.Collections;
using System.Collections.Generic;
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

    public SaintsProp[] SaintsProps;

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
        if (!SaintsManager.Instance.UnlockedSaints.Any()) return;
        
        SaintData saintData = SaintsManager.Instance.UnlockedSaints[CurrentSaintIndex];
        SaintsProp prop = SaintsProps.Where(x => x.ID == saintData.Id).FirstOrDefault();

        //populate the saint data
        SaintPotrait.enabled = true;
        SaintPotrait.sprite = prop.image;
        Bio.text = LocalizationManager.Instance.GetText(saintData.BioKey);
        Title.text = saintData.Name;
    }

    public void OnExit()
    {
        GameManager.Instance.LoadScene("NormalLevelAjust", UnityEngine.SceneManagement.LoadSceneMode.Single);
    }

    [System.Serializable]
    public struct SaintsProp
    {
        public SaintID ID;
        public Sprite image;
    }
}
