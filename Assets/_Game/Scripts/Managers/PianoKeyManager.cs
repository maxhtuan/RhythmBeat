using UnityEngine;
using System;
using System.Collections.Generic;
using System.Linq;

public class PianoKeyManager : MonoBehaviour, IService
{
    public void Initialize()
    {
        Debug.Log("PianoKeyManager: Initialized");

        var pianoKeys = GameObject.FindObjectsByType<PianoKey>(FindObjectsSortMode.None);

        foreach (var pianoKey in pianoKeys)
        {
            pianoKey.Initialize();
        }
    }

    public void Cleanup()
    {
        Debug.Log("PianoKeyManager: Cleaned up");
    }
}