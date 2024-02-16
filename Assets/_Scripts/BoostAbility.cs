using System.Collections;
using System.Collections.Generic;
using Opsive.UltimateCharacterController.Character.Abilities;
using UnityEngine;

public class BoostAbility : Ability
{
    public override bool IsConcurrent { get { return true; } }

}
