using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class EditorController : MonoBehaviour
{
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
        } else
        {
            GlobalData.currentBeat = Mathf.Round(GlobalData.currentBeat/GlobalData.beatPrecision)*GlobalData.beatPrecision;
        }

        GlobalData.currentBeat = Mathf.Max(0, GlobalData.currentBeat);
    }
}