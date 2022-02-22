﻿using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorController : MonoBehaviour
{
    public bool snapWhenPaused = true;
    public Toggle snapWhenPausedToggle;

    void Start()
    {
        GlobalData.paused = true;
        GlobalData.currentBeat = 0;
        GlobalData.beatPrecision = 1/8f;
    }

    void Update()
    {
        if (!GlobalData.paused)
        {
            GlobalData.currentBeat += GlobalData.bpm*Time.deltaTime/60;
        } else if (snapWhenPaused)
        {
            GlobalData.currentBeat = Mathf.Round(GlobalData.currentBeat/GlobalData.beatPrecision)*GlobalData.beatPrecision;
        }

        GlobalData.currentBeat = Mathf.Max(0, GlobalData.currentBeat);
    }

    public void toggleSnapWhenPaused()
    {
        snapWhenPaused = snapWhenPausedToggle.isOn;
    }
}