using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorController : MonoBehaviour
{
    public bool snapWhenPaused = true;

    [Header("References")]
    public Toggle snapWhenPausedToggle;

    public Spawnable hitSpawnable;
    private Spawnable lastHitSpawnable;

    void Start()
    {
        GlobalData.paused = true;
        GlobalData.currentBeat = 0;
        GlobalData.beatPrecision = 1 / 4f;
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
                hitSpawnable = hit.transform.GetComponent<Spawnable>();

                if(hitSpawnable != null)
                {
                    hitSpawnable.selected = !hitSpawnable.selected;
                }
            } else
            {
                hitSpawnable = null;
            }

            if (lastHitSpawnable != null && lastHitSpawnable != hitSpawnable)
            {
                lastHitSpawnable.selected = false;
            }

            lastHitSpawnable = hitSpawnable;
        }
    }

    public void toggleSnapWhenPaused()
    {
        snapWhenPaused = snapWhenPausedToggle.isOn;
    }

    public void RotateCurrent(float direction)
    {
        if (hitSpawnable.GetType().IsSubclassOf(typeof(Colored)) && hitSpawnable.selected){
            Colored hitColored = hitSpawnable as Colored;
            hitColored.cutDirection += (int)direction;
            hitColored.cutDirection %= 8;
            hitColored.UpdateRotation();
        }
    }
}