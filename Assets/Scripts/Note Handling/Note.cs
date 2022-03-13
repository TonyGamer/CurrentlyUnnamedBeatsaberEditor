using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Note : Colored
{
    [Header("Note")]
    public int angleOffset;

    [Header("References")]
    public ParticleSystem particle;

    public Note(float beat, int x, int y, int cutDirection, int color, int angleOffset)
    {
        this.beat = beat;
        this.x = x;
        this.y = y;
        this.cutDirection = cutDirection;
        this.color = color;
        this.angleOffset = angleOffset;
    }

    void Start()
    {
        var startColor = gameObject.GetComponent<Renderer>().material.color;
        var endColor = startColor;
        endColor.a = 0;

        var main = particle.main;
        main.startColor = new ParticleSystem.MinMaxGradient(startColor, endColor);
    }


    public override void UpdateRotation()
    {
        transform.rotation = Spawner.CalculateRotation(cutDirection, angleOffset);
        Changed();
    }

    public static explicit operator NoteSerial(Note note)
    {
        return new NoteSerial(note.beat, note.x, note.y, note.color, note.cutDirection, note.angleOffset);
    }
}
