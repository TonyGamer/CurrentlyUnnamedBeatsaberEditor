using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorController : MonoBehaviour
{
    public bool snapWhenPaused = true;
    public Toggle snapWhenPausedToggle;

    private Note hitNote;
    private Note lastHitNote;

    void Start()
    {
        GlobalData.paused = true;
        GlobalData.currentBeat = 0;
        GlobalData.beatPrecision = 1 / 8f;
    }

    void Update()
    {
        if (!GlobalData.paused)
        {
            GlobalData.currentBeat += GlobalData.bpm * Time.deltaTime / 60;
        }
        else if (snapWhenPaused)
        {
            GlobalData.currentBeat = Mathf.Round(GlobalData.currentBeat / GlobalData.beatPrecision) * GlobalData.beatPrecision;
        }

        GlobalData.currentBeat = Mathf.Max(0, GlobalData.currentBeat);

        if (Input.GetMouseButtonDown(0))
        {
            Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
            RaycastHit hit;
            if (Physics.Raycast(ray, out hit))
            {
                hitNote = hit.transform.GetComponent<Note>();

                if(hitNote != null)
                {
                    hitNote.selected = !hitNote.selected;
                }
            } else
            {
                hitNote = null;
            }

            if (lastHitNote != null && lastHitNote != hitNote)
            {
                lastHitNote.selected = false;
            }

            lastHitNote = hitNote;
        }
    }

    public void toggleSnapWhenPaused()
    {
        snapWhenPaused = snapWhenPausedToggle.isOn;
    }
}