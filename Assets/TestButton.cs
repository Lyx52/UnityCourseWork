using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.XR.Interaction.Toolkit;

public class TestButton : MonoBehaviour
{
    public void OnInteract(HoverEnterEventArgs args)
    {
        Debug.Log("test");
    }
}
