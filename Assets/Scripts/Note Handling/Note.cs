using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Note Type")]
    public float time;
    public int lineIndex;
    public int lineLayer;
    public int cutDirection;
    public int type = 0;

    [Header("References")]
    public ParticleSystem particle;

    public Note(float time, int lineIndex, int lineLayer, int cutDirection, int type)
    {
        this.time = time;
        this.lineIndex = lineIndex;
        this.lineLayer = lineLayer;
        this.cutDirection = cutDirection;
        this.type = type;
    }

    void Start()
    {
        var startColor = gameObject.GetComponent<Renderer>().material.color;
        var endColor = startColor;
        endColor.a = 0;

        var main = particle.main;
        main.startColor = new ParticleSystem.MinMaxGradient(startColor, endColor);
    }

    void Update()
    {
        transform.position = new Vector3(lineIndex - 1.5f, lineLayer + 0.5f, 0.5f * GlobalData.jumpSpeed * (time - GlobalData.currentBeat));

        if(GlobalData.currentBeat > time || GlobalData.currentBeat < time - GlobalData.HJD)
        {
            Object.Destroy(this.gameObject);
        }
    }

    public static explicit operator NoteSerial(Note note)
    {
        return new NoteSerial(note.time, note.lineIndex, note.lineLayer, note.type, note.cutDirection);
    }
}
