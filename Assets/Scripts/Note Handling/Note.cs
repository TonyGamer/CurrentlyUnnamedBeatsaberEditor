using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    public float time;
    public int lineIndex;
    public int lineLayer;
    public int cutDirection;
    public int type = 0;

    public Note(float time, int lineIndex, int lineLayer, int cutDirection, int type)
    {
        this.time = time;
        this.lineIndex = lineIndex;
        this.lineLayer = lineLayer;
        this.cutDirection = cutDirection;
        this.type = type;
    }

    void Update()
    {
        transform.position = new Vector3(lineIndex - 1.5f, lineLayer + 0.5f, 0.5f * GlobalData.jumpSpeed * (time-GlobalData.currentBeat));

        if(GlobalData.currentBeat > time || GlobalData.currentBeat + 1.0 - GlobalData.offset < time)
        {
            Object.Destroy(this.gameObject);
        }
    }

    public static explicit operator NoteSerial(Note note)
    {
        return new NoteSerial(note.time, note.lineIndex, note.lineLayer, note.type, note.cutDirection);
    }
}
