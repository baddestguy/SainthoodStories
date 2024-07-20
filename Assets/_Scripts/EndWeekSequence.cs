using System.Collections;
using System.Linq;
using Assets.Xbox;
using DG.Tweening;
using Michsky.UI.ModernUIPack;
using TMPro;
using UnityEngine;

public class EndWeekSequence : MonoBehaviour
{
    public GameObject BG;
    public GameObject CPFPObj;
    public GameObject SaintsUnlockObj;
    public GameObject CashUnlockObj;
    public GameObject ContinueObj;
    public GameObject DaysLeftObj;

    public TextMeshProUGUI Title;
    public TextMeshProUGUI SaintUnlockedTitle;
    public TextMeshProUGUI Score;
    public TextMeshProUGUI SaintScore;
    public TextMeshProUGUI CashAmount;
    public TextMeshProUGUI OldDaysleft;
    public TextMeshProUGUI NewDaysleft;

    public EndgameSaintPortrait[] SaintPortraits;
    public ProgressBar SaintProgressBar;
    private bool Continue = false;

    public IEnumerator RunSequenceAsync()
    {
        int cashAmount = MissionManager.Instance.CharityPointsPool;
        var donation = InventoryManager.Instance.GetProvision(Provision.ALLOWANCE);
        cashAmount += donation?.Value ?? 0;
        TreasuryManager.Instance.DonateMoney(cashAmount);
        SaintProgressBar.currentPercent = MissionManager.Instance.FaithPoints * 100f / GameDataManager.Instance.GetNextSaintUnlockThreshold();
        SaintProgressBar.endPercent = (MissionManager.Instance.FaithPointsPool + MissionManager.Instance.FaithPoints) * 100f / GameDataManager.Instance.GetNextSaintUnlockThreshold();
        var saintsUnlocked = MissionManager.Instance.UnlockSaints();

        BG.SetActive(true);
        CPFPObj.SetActive(true);
        Title.text = LocalizationManager.Instance.GetText("CP_ENDGAME_TITLE");
        Score.DOCounter(MissionManager.Instance.CharityPoints, MissionManager.Instance.CharityPoints + MissionManager.Instance.CharityPointsPool, 2f);
        //SoundManager.Instance.PlayOneShotSfx("EndgameCharge_SFX", timeToDie:5f);

        yield return new WaitForSeconds(2f);

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
            if (GameSettings.Instance.IsUsingController)
            {
                var pressedButton = GamePadController.GetButton();
                if (pressedButton.Button == GamePadButton.South && pressedButton.Control.wasPressedThisFrame) ContinueSequence();
            }
            
            yield return null;
        }

        if(SaintsManager.Instance.UnlockedSaints.Count < GameDataManager.TOTAL_UNLOCKABLE_SAINTS)
        {
            ContinueObj.SetActive(false);
            Continue = false;

        //    SoundManager.Instance.PlayOneShotSfx("EndgameCharge_SFX", timeToDie: 5f);
         //   SoundManager.Instance.PlayOneShotSfx("MassBells_SFX", timeToDie: 5f);
            CashUnlockObj.SetActive(false);
            Title.text = LocalizationManager.Instance.GetText("FP_ENDGAME_TITLE");
            SaintScore.DOCounter(MissionManager.Instance.FaithPoints, MissionManager.Instance.FaithPoints + MissionManager.Instance.FaithPointsPool, 2f);
            SaintsUnlockObj.SetActive(true);
            SaintProgressBar.gameObject.SetActive(true);
            SaintProgressBar.isOn = true;
            SaintProgressBar.speed = 1;

            yield return new WaitForSeconds(2f);

            var sp = SaintPortraits[1];
            if(saintsUnlocked.Any())
            {
                Title.text = "";
                Score.text = "";
                SaintUnlockedTitle.text = LocalizationManager.Instance.GetText("NEW SAINT UNLOCKED!");
                SaintProgressBar.gameObject.SetActive(false);
                SoundManager.Instance.PlayOneShotSfx("MassBegin_SFX", timeToDie: 4);
                SoundManager.Instance.PlayOneShotSfx("StartGame_SFX", 1f, 10);
                SoundManager.Instance.PlayOneShotSfx("Success_SFX", 1f, 5f);
                sp.BG.color = new Color(1, 1, 1, 1);
                sp.Saint.gameObject.SetActive(true);
                sp.SaintName.text = saintsUnlocked.ElementAt(0).Name;
                sp.Saint.sprite = Resources.Load<Sprite>(saintsUnlocked.ElementAt(0).IconPath);
                localPosition = sp.transform.localPosition.x;
                sp.gameObject.SetActive(true);
                sp.BG.gameObject.SetActive(true);
                sp.transform.localPosition = new Vector3(sp.transform.localPosition.x - 50, sp.transform.localPosition.y, sp.transform.localPosition.z);
                sp.transform.DOLocalMoveX(localPosition, 0.5f);
            }

            ContinueObj.SetActive(true);
            while (!Continue)
            {
                if (GameSettings.Instance.IsUsingController)
                {
                    var pressedButton = GamePadController.GetButton();
                    if (pressedButton.Button == GamePadButton.South && pressedButton.Control.wasPressedThisFrame) ContinueSequence();
                }
                yield return null;
            }
        }

        ContinueObj.SetActive(false);
        CashUnlockObj.SetActive(false);
        SaintsUnlockObj.SetActive(false);
        CPFPObj.SetActive(false);
        SaintProgressBar.gameObject.SetActive(false);

        DaysLeftObj.SetActive(true);
        OldDaysleft.text = $"{40 - MissionManager.Instance.CurrentMissionId+1}";
        NewDaysleft.text = $"{40 - MissionManager.Instance.CurrentMissionId}";

        yield return new WaitForSeconds(1f);

        OldDaysleft.transform.DOMoveX(OldDaysleft.transform.position.x+100, 5);
        NewDaysleft.transform.DOMoveX(OldDaysleft.transform.position.x, 5);
        OldDaysleft.DOFade(0, 5f);
        NewDaysleft.DOFade(1, 5f);

        yield return new WaitForSeconds(6f);

        DaysLeftObj.SetActive(false);
        gameObject.SetActive(false);
    }

    public void ContinueSequence()
    {
        Continue = true;
    }
}
