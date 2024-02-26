using System.Collections;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using UnityEngine;

public class WorldPlayer : MonoBehaviour
{
    public UltimateCharacterLocomotion MyLocomotor;
    private AnimatorMonitor MyAnimator;
    float BoostTimer = 5f;
    float BoostWindowTimer = 1f;
    bool BoostWindow = false;
    bool Boosted = false;

    float accelerator = 0f;

    // Start is called before the first frame update
    void Start()
    {
        MyAnimator = GetComponentInChildren<AnimatorMonitor>();
        StartCoroutine("BoostAsync");
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
            MyLocomotor.MotorAcceleration = new Vector3(MyLocomotor.MotorAcceleration.x, MyLocomotor.MotorAcceleration.y, 3.2f);
        else
            MyLocomotor.MotorAcceleration = new Vector3(MyLocomotor.MotorAcceleration.x, MyLocomotor.MotorAcceleration.y, 1.5f * accelerator);
    }

    public void OnBoost(Ability ability, bool trigger)
    {
        if (ability is not BoostAbility || !trigger || !GameManager.Instance.HasPlayerEnergy()) return;

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
        SoundManager.Instance.PlayOneShotSfx("ActionButton_SFX", timeToDie: 5f);
        SoundManager.Instance.PlayOneShotSfx("Crit_Good");
    }

    IEnumerator BoostAsync()
    {
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
        StopCoroutine("BoostAsync");
        Boosted = false;
        BoostWindow = false;
        StartCoroutine("BoostAsync");
    }
}
