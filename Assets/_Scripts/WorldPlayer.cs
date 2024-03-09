using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Opsive.UltimateCharacterController.Camera;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using Opsive.UltimateCharacterController.ThirdPersonController.Camera.ViewTypes;
using UnityEngine;

public class WorldPlayer : MonoBehaviour
{
    public UltimateCharacterLocomotion MyLocomotor;
    private AnimatorMonitor MyAnimator;
    public CameraController MyCamera;
    private GameObject CriticalCircleFX;

    float BoostTimer = 5f;
    float BoostWindowTimer = 1f;
    bool BoostWindow = false;
    bool Boosted = false;

    float accelerator = 0f;
    bool CaptureTime = false;
    SacredItem CurrentSacredItem = null;

    // Start is called before the first frame update
    void Start()
    {
        MyAnimator = GetComponentInChildren<AnimatorMonitor>();
        StartCoroutine("BoostAsync");
        StartCoroutine("RestartBoostWindowAsync");
        FindObjectOfType<WorldTextDisplay>().DisplayEnergy(GameManager.Instance.PlayerEnergy);
    }

    // Update is called once per frame
    void Update()
    {
        if (MyAnimator.Moving)
        {
            accelerator = Mathf.Clamp(accelerator+0.035f, 0, 1);
        }
        else
        {
            accelerator = 0f;
        }

        if (Boosted)
            MyLocomotor.MotorAcceleration = new Vector3(MyLocomotor.MotorAcceleration.x, MyLocomotor.MotorAcceleration.y, 5f);
        else
            MyLocomotor.MotorAcceleration = new Vector3(MyLocomotor.MotorAcceleration.x, MyLocomotor.MotorAcceleration.y, 3.2f * accelerator);
    }

    public void OnBoost(Ability ability, bool trigger)
    {
        if (ability is not BoostAbility || !trigger) return;
        if (!GameManager.Instance.HasPlayerEnergy())
        {
            FindObjectOfType<WorldTextDisplay>().DisplayEnergy(GameManager.Instance.PlayerEnergy);

            return;
        }

        if (!BoostWindow)
        {
            Boosted = false;
            SoundManager.Instance.PlayOneShotSfx("Crit_Bad");
            EndBoost();
            return;
        }

        Boosted = true;
        StopCoroutine("BoostAsync");
        BoostWindow = false;
        StartCoroutine("BoostAsync");

        GameManager.Instance.UpdatePlayerEnergyFromWorld(-1);
        FindObjectOfType<WorldTextDisplay>().DisplayEnergy(GameManager.Instance.PlayerEnergy);

        SoundManager.Instance.PlayOneShotSfx("ActionButton_SFX", timeToDie: 5f);
        SoundManager.Instance.PlayOneShotSfx("Crit_Good");
        DOTween.To(() => (MyCamera.ActiveViewType as ThirdPerson).LookOffset, 
                    newValue => (MyCamera.ActiveViewType as ThirdPerson).LookOffset = newValue,
                    new Vector3(0, 0, -7f), 
                    2f);
        DOTween.To(() => (MyCamera.ActiveViewType as ThirdPerson).FieldOfView,
            newValue => (MyCamera.ActiveViewType as ThirdPerson).FieldOfView = newValue,
            80,
            2f);

    }

    public void CaptureTimeAction(Ability ability, bool trigger)
    {
        if (!CaptureTime || CurrentSacredItem == null || ability is not CaptureAbility || !trigger) return;

        var critCircleScaleX = CriticalCircleFX.transform.GetChild(0).transform.localScale.x;
        var value = 0;
        if (critCircleScaleX > 0.65f && critCircleScaleX < 0.85f)
        {
            if (CurrentSacredItem.Behaviour == SacredItemBehaviour.WANDER)
            {
                value = (int)CurrentSacredItem.transform.localScale.x;
                InventoryManager.Instance.AddWanderers(value);
            }
            FindObjectOfType<WorldTextDisplay>().Display(CurrentSacredItem, value);

            var collectibleItem = CurrentSacredItem.GetComponent<CollectibleItem>();
            if (collectibleItem != null)
            {
                MissionManager.Instance.Collect(gameObject.name + ":" + collectibleItem.Name);
            }

            CurrentSacredItem.gameObject.SetActive(false);
            CurrentSacredItem = null;
        }
        else
        {
            if (CurrentSacredItem.Behaviour == SacredItemBehaviour.CHASE)
            {
                value = (int)-CurrentSacredItem.transform.localScale.x;
                MissionManager.Instance.UpdateFaithPoints(value);
                FindObjectOfType<WorldTextDisplay>().Display(CurrentSacredItem, value);
                CurrentSacredItem.gameObject.SetActive(false);
                CurrentSacredItem = null;
            }
        }

        Time.timeScale = 1f;
        CriticalCircleFX.SetActive(false);
        CriticalCircleFX.transform.GetChild(0).transform.localScale = new Vector3(5, 5, 5);
        CaptureTime = false;
    }

    IEnumerator RestartBoostWindowAsync()
    {
        yield return new WaitForSeconds(BoostTimer);
        if (GameManager.Instance.HasPlayerEnergy())
        {
            SoundManager.Instance.PlayOneShotSfx("Notification_SFX");
            BoostWindow = true;
        }
    }

    IEnumerator BoostAsync()
    {
        if (!Boosted) yield break;

        yield return new WaitForSeconds(BoostTimer);
        if (GameManager.Instance.HasPlayerEnergy())
        {
            SoundManager.Instance.PlayOneShotSfx("Notification_SFX");
        }
        BoostWindow = true;
        yield return new WaitForSeconds(BoostWindowTimer);
        BoostWindow = false;
        EndBoost();
    }

    public void EndBoost()
    {
        DOTween.To(() => (MyCamera.ActiveViewType as ThirdPerson).LookOffset,
                  newValue => (MyCamera.ActiveViewType as ThirdPerson).LookOffset = newValue,
                  new Vector3(0, 0, -8.5f),
                  2f);
        DOTween.To(() => (MyCamera.ActiveViewType as ThirdPerson).FieldOfView,
            newValue => (MyCamera.ActiveViewType as ThirdPerson).FieldOfView = newValue,
            60,
            2f);
        StopCoroutine("BoostAsync");
        Boosted = false;
        BoostWindow = false;
        StopCoroutine("RestartBoostWindowAsync");
        StartCoroutine("RestartBoostWindowAsync");
    }

    public void CaptureItem()
    {
        StartCoroutine(CaptureItemAsync());
    }

    IEnumerator CaptureItemAsync()
    {
        CaptureTime = true;
        Time.timeScale = 0.4f;
        if (CriticalCircleFX == null)
        {
            CriticalCircleFX = Instantiate(Resources.Load<GameObject>("UI/CriticalCircle"));
        }
        CriticalCircleFX.SetActive(true);
        CriticalCircleFX.transform.SetParent(FindObjectOfType<WorldTextDisplay>().transform, true);
        CriticalCircleFX.transform.localScale = new Vector3(0.5f, 0.5f, 0.5f);
        CriticalCircleFX.transform.position = new Vector2(Screen.width * 0.5f, Screen.height * 0.5f);
        CriticalCircleFX.transform.localEulerAngles = Vector3.zero;

        var critCircle = CriticalCircleFX.transform.GetChild(0);
        critCircle.localScale = new Vector3(5, 5, 5);
        critCircle.DOScale(new Vector3(0.3f, 0.3f, 0.3f), 0.8f);

        yield return new WaitForSecondsRealtime(2f);

        //Play FX
        Time.timeScale = 1f;
        CriticalCircleFX.SetActive(false);
        critCircle.localScale = new Vector3(5, 5, 5);
        CaptureTime = false;

        if (CurrentSacredItem != null && CurrentSacredItem.Behaviour == SacredItemBehaviour.CHASE)
        {
            var value = (int)-CurrentSacredItem.transform.localScale.x;
            MissionManager.Instance.UpdateFaithPoints(value);
            FindObjectOfType<WorldTextDisplay>().Display(CurrentSacredItem, value);
            CurrentSacredItem.gameObject.SetActive(false);
            CurrentSacredItem = null;
        }

    }

    private void OnTriggerEnter(Collider other)
    {
        var sacredItem = other.GetComponent<SacredItem>();

        if (sacredItem != null)
        {
            CurrentSacredItem = sacredItem;
            CaptureItem();
        }
    }
}
