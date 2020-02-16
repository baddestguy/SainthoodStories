using System;
using System.Collections;
using System.Collections.Generic;
using EventCallbacks;
using UnityEngine;

public class TouchControl : MonoBehaviour
{
    private void Awake(){
        Physics.queriesHitTriggers = true;
    }

    // Update is called once per frame
    void Update()
    {
        TriggerTouchComplete();
    }

    void TriggerTouchComplete(){
        if (Input.GetMouseButtonUp(0)){
            OnTouchComplete touchComplete = new OnTouchComplete();
            touchComplete.FireEvent();
        }
    }
}
