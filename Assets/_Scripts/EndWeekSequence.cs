using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Assets._Scripts.Xbox;
using DG.Tweening;
using Michsky.MUIP;
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
    public ProgressBar TotalCPProgress;
    public TextMeshProUGUI SaintScore;
    public TextMeshProUGUI CashAmount;
    public TextMeshProUGUI OldDaysleft;
    public TextMeshProUGUI NewDaysleft;

    public EndgameSaintPortrait[] SaintPortraits;
    public ProgressBar SaintProgressBar;
    public ProgressBar TotalFPProgress;
    private bool Continue = false;

    public IEnumerator RunSequenceAsync(int fp, int fpPool, int fpTarget, int cp, int cpPool, IEnumerable<SaintData> saintsUnlocked, int missionId)
    {
        int cashAmount = Mathf.Clamp(cpPool, 0, 100000000);
        var donation = InventoryManager.Instance.GetProvision(Provision.ALLOWANCE);
        cashAmount += donation?.Coin ?? 0;
        TreasuryManager.Instance.DonateMoney(cashAmount);   //Will not get this if player closes before RunSequence

        if (SaintsManager.Instance.UnlockedSaints.Count < GameDataManager.TOTAL_UNLOCKABLE_SAINTS)
        {
            SaintProgressBar.currentPercent = fp * 100f / fpTarget;
        //    SaintProgressBar.endPercent = (fpPool + fp) * 100f / fpTarget;
        }
        else
        {
            SaintProgressBar.currentPercent = 0;
        //    SaintProgressBar.endPercent = 0;
        }

        BG.SetActive(true);
        CPFPObj.SetActive(true);
        Title.text = LocalizationManager.Instance.GetText("CP_ENDGAME_TITLE");
        Score.DOCounter(cp, cp + cpPool, 2f);
        TotalCPProgress.currentPercent = cp * 100 / MissionManager.TOTAL_CP_TARGET;
      //  TotalCPProgress.endPercent = Mathf.Ceil((cp + cpPool) * 100 / MissionManager.TOTAL_CP_TARGET)+1;
        //SoundManager.Instance.PlayOneShotSfx("EndgameCharge_SFX", timeToDie:5f);

        yield return new WaitForSeconds(2f);

        Score.text = "";
        Title.text = "";
        var localPosition = CashUnlockObj.transform.localPosition.x;
        CashUnlockObj.transform.localPosition = new Vector3(CashUnlockObj.transform.localPosition.x - 50, CashUnlockObj.transform.localPosition.y, CashUnlockObj.transform.localPosition.z);
        CashUnlockObj.transform.DOLocalMoveX(localPosition, 0.5f);
        CashUnlockObj.SetActive(true);
        TotalCPProgress.gameObject.SetActive(false);
        CashAmount.text = "+" + cashAmount.ToString();
        SoundManager.Instance.PlayOneShotSfx("Cheer_SFX", timeToDie: 5f);

        //Wait for User input
        ContinueObj.SetActive(true);
        while (!Continue)
        {
            if (GameSettings.Instance.IsUsingController)
            {
                var pressedButton = GamePadController.GetButton();
                if (pressedButton.Button == GamePadButton.South && pressedButton.Control.WasPressedThisFrame) ContinueSequence();
            }
            
            yield return null;
        }

        {
            ContinueObj.SetActive(false);
            Continue = false;

        //    SoundManager.Instance.PlayOneShotSfx("EndgameCharge_SFX", timeToDie: 5f);
         //   SoundManager.Instance.PlayOneShotSfx("MassBells_SFX", timeToDie: 5f);
            CashUnlockObj.SetActive(false);
            Title.text = LocalizationManager.Instance.GetText("FP_ENDGAME_TITLE");
            SaintScore.DOCounter(fp, fp + fpPool, 2f);
            SaintsUnlockObj.SetActive(SaintsManager.Instance.UnlockedSaints.Count < GameDataManager.TOTAL_UNLOCKABLE_SAINTS);
            SaintProgressBar.gameObject.SetActive(true);
            SaintProgressBar.isOn = SaintsManager.Instance.UnlockedSaints.Count < GameDataManager.TOTAL_UNLOCKABLE_SAINTS;
            SaintProgressBar.speed = 1;

            TotalFPProgress.currentPercent = fp * 100f / MissionManager.TOTAL_FP_TARGET;
          //  TotalFPProgress.endPercent = Mathf.Ceil((fpPool + fp) * 100f / MissionManager.TOTAL_FP_TARGET)+1;
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
                    if (pressedButton.Button == GamePadButton.South && pressedButton.Control.WasPressedThisFrame) ContinueSequence();
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
        OldDaysleft.text = $"{GameDataManager.MAX_MISSION_ID - missionId + 1}";
        NewDaysleft.text = $"{GameDataManager.MAX_MISSION_ID - missionId}";

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
