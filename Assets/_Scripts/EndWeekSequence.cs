using System.Collections;
using System.Linq;
using DG.Tweening;
using TMPro;
using UnityEngine;

public class EndWeekSequence : MonoBehaviour
{
    public GameObject BG;
    public GameObject CPFPObj;
    public GameObject SaintsUnlockObj;
    public GameObject CashUnlockObj;
    public GameObject ContinueObj;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI CashAmount;

    public EndgameSaintPortrait[] SaintPortraits;
    
    private bool Continue = false;

    public IEnumerator RunSequenceAsync()
    {
        int cashAmount = Random.Range(2, 4);
        TreasuryManager.Instance.DonateMoney(cashAmount);
        var saintsUnlocked = MissionManager.Instance.UnlockSaints();

        BG.SetActive(true);
        CPFPObj.SetActive(true);
        Title.text = LocalizationManager.Instance.GetText("CP_ENDGAME_TITLE");
        Score.DOCounter(0, MissionManager.Instance.CharityPoints, 3f);
        SoundManager.Instance.PlayOneShotSfx("EndgameCharge_SFX", timeToDie:5f);

        yield return new WaitForSeconds(4f);

        Score.text = "";
        Title.text = "";
        var localPosition = CashUnlockObj.transform.localPosition.x;
        CashUnlockObj.transform.localPosition = new Vector3(CashUnlockObj.transform.localPosition.x - 50, CashUnlockObj.transform.localPosition.y, CashUnlockObj.transform.localPosition.z);
        CashUnlockObj.transform.DOLocalMoveX(localPosition, 0.5f);
        CashUnlockObj.SetActive(true);
        CashAmount.text = "+" + cashAmount.ToString();
        SoundManager.Instance.PlayOneShotSfx("Cheer_SFX", timeToDie: 5f);

        //Wait for User input
        ContinueObj.SetActive(true);
        while (!Continue)
        {
            yield return null;
        }
        ContinueObj.SetActive(false);
        Continue = false;

        CashUnlockObj.SetActive(false);
        Title.text = LocalizationManager.Instance.GetText("FP_ENDGAME_TITLE");
        Score.DOCounter(0, MissionManager.Instance.FaithPoints, 3f);
        SoundManager.Instance.PlayOneShotSfx("EndgameCharge_SFX", timeToDie: 5f);

        yield return new WaitForSeconds(4f);

        //Show Counter to a single Saint unlock at the End of the Week

        Title.text = "";
        Score.text = "";
        SaintsUnlockObj.SetActive(true);
        SoundManager.Instance.PlayOneShotSfx("MassBells_SFX", timeToDie: 5f);

        for (int i = 0; i < saintsUnlocked.Count(); i++)
        {
            SaintPortraits[i].BG.color = new Color(1, 1, 1, 1);
            SaintPortraits[i].Saint.gameObject.SetActive(true);
            SaintPortraits[i].SaintName.text = saintsUnlocked.ElementAt(i).Name;
            SaintPortraits[i].Saint.sprite = Resources.Load<Sprite>(saintsUnlocked.ElementAt(i).IconPath);
        }

        foreach (var sp in SaintPortraits)
        {
            localPosition = sp.transform.localPosition.x;
            sp.gameObject.SetActive(true);
            sp.BG.gameObject.SetActive(true);
            sp.transform.localPosition = new Vector3(sp.transform.localPosition.x - 50, sp.transform.localPosition.y, sp.transform.localPosition.z);
            sp.transform.DOLocalMoveX(localPosition, 0.5f);
            yield return new WaitForSeconds(0.25f);
        }

        yield return new WaitForSeconds(3f);

        ContinueObj.SetActive(true);
        while (!Continue)
        {
            yield return null;
        }

        gameObject.SetActive(false);
    }

    public void ContinueSequence()
    {
        Continue = true;
    }
}
