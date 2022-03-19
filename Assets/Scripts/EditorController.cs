using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class EditorController : MonoBehaviour
{
    public bool snapWhenPaused = true;

    [Header("References")]
    public MapManager mapManager;
    public Toggle snapWhenPausedToggle;

    public Spawnable hitSpawnable;
    private Spawnable lastHitSpawnable;

    private Plane hitPlane;
    private Vector3 offset;

    public SpawnableType spawnableToPlace = SpawnableType.Note;
    public int spawnableColor = 0;

    public enum SpawnableType
    {
        Note,
        Bomb,
        Wall,
        Stack,
        Rail
    }

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
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~(1 << 8)))
            {
                hitSpawnable = hit.transform.GetComponent<Spawnable>();

                if (hitSpawnable != null)
                {
                    hitSpawnable.selected = !hitSpawnable.selected;
                    hitPlane = new Plane(Vector3.forward, hit.transform.position);

                    offset = GetPlaneIntersect() - new Vector3(hitSpawnable.x, hitSpawnable.y, 0);
                }
            }
            else
            {
                hitPlane = new Plane(Vector3.forward, Vector3.zero);
                Vector3 spawnPosition = GetPlaneIntersect();
 
                hitSpawnable = mapManager.AddSpawnable(spawnableToPlace, (int)Mathf.Floor(spawnPosition.x + 2), (int)Mathf.Floor(spawnPosition.y), spawnableColor);

                if(hitSpawnable != null)
                {
                    hitSpawnable.selected = true;
                    offset = spawnPosition - new Vector3(hitSpawnable.x, hitSpawnable.y, 0);
                }
            }

            if (lastHitSpawnable != null && lastHitSpawnable != hitSpawnable)
            {
                lastHitSpawnable.selected = false;
            }

            lastHitSpawnable = hitSpawnable;
        }

        if (Input.GetMouseButton(0) && hitSpawnable != null)
        {
            if (hitSpawnable.selected)
            {
                Vector3 curPosition = GetPlaneIntersect() - offset;

                MoveCurrent((int)Mathf.Floor(curPosition.x + 0.5f), (int)Mathf.Floor(curPosition.y + 0.5f));

                hitSpawnable.Moved();
            }
        }
    }

    public void ToggleSnapWhenPaused()
    {
        snapWhenPaused = snapWhenPausedToggle.isOn;
    }

    public void RotateCurrent(float direction)
    {
        if (hitSpawnable.GetType().IsSubclassOf(typeof(Colored)) && hitSpawnable.selected)
        {
            Colored hitColored = hitSpawnable as Colored;
            hitColored.cutDirection = (((hitColored.cutDirection + (int)direction) % 8) + 8) % 8;
            hitColored.UpdateRotation();
        }
    }

    public void MoveCurrent(int x, int y)
    {
        hitSpawnable.x = x;
        hitSpawnable.y = y;
    }

    public void ChangeBeat(int beat)
    {
        hitSpawnable.beat += beat * GlobalData.beatPrecision;
        hitSpawnable.Moved();
    }

    private Vector3 GetPlaneIntersect()
    {
        float depth = 0;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hitPlane.Raycast(ray, out depth);

        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);

        return curPosition;
    }

    public void DeleteSelected()
    {
        mapManager.DeleteSpawnable(hitSpawnable);
    }
}