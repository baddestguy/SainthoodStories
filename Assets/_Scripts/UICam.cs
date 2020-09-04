using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UICam : MonoBehaviour
{
    public static UICam Instance { get; private set; }
    public Camera Camera;

    private void Awake()
    {
        Instance = this;
    }
}
