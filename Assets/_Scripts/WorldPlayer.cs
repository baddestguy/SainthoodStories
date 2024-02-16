using System.Collections;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.Character;
using Opsive.UltimateCharacterController.Character.Abilities;
using UnityEngine;

public class WorldPlayer : MonoBehaviour
{
    public UltimateCharacterLocomotion MyLocomotor;
    float BoostTimer = 5f;
    float BoostWindowTimer = 1f;
    bool BoostWindow = false;
    bool Boosted = false;

    // Start is called before the first frame update
    void Start()
    {
        MyLocomotor.MotorAcceleration = new Vector3(MyLocomotor.MotorAcceleration.x, MyLocomotor.MotorAcceleration.y, 1f);
        StartCoroutine("BoostAsync");
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void OnBoost(Ability ability, bool trigger)
    {
        if (ability is not BoostAbility) return;

        if (!BoostWindow)
        {
            Boosted = false;
            SoundManager.Instance.PlayOneShotSfx("Crit_Bad");
            EndBoost();
            return;
        }

        Boosted = true;
        MyLocomotor.MotorAcceleration = new Vector3(MyLocomotor.MotorAcceleration.x, MyLocomotor.MotorAcceleration.y, 3.2f);
        SoundManager.Instance.PlayOneShotSfx("ActionButton_SFX", timeToDie: 5f);
        SoundManager.Instance.PlayOneShotSfx("Crit_Good");
    }

    IEnumerator BoostAsync()
    {
  //      while (true)
        {
            yield return new WaitForSeconds(BoostTimer);
            SoundManager.Instance.PlayOneShotSfx("Notification_SFX");
            BoostWindow = true;
            yield return new WaitForSeconds(BoostWindowTimer);
            BoostWindow = false;
            EndBoost();
            //if (!Boosted)
            //{
            //    SoundManager.Instance.PlayOneShotSfx("Crit_Bad");
            //}
            Boosted = false;
        }
    }

    public void EndBoost()
    {
        StopCoroutine("BoostAsync");
        BoostWindow = false;
        StartCoroutine("BoostAsync");

        if (Boosted) return;
        
        //Fail Boost FX
        MyLocomotor.MotorAcceleration = new Vector3(MyLocomotor.MotorAcceleration.x, MyLocomotor.MotorAcceleration.y, 1f);
    }
}
