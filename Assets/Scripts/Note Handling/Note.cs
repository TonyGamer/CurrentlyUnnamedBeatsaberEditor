using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Note : MonoBehaviour
{
    [Header("Note Type")]
    public float beat;
    public int x;
    public int y;
    public int cutDirection;
    public int color = 0;
    public int angleOffset;

    public bool selected = false;

    private bool transparent;

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

    void Update()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;

        transform.position = Spawner.calculatePosition(x, y, beat);

        if (beatsTilHit < -0.5f || beatsTilHit > GlobalData.HJD)
        {
            Object.Destroy(this.gameObject);
        }
        else if (beatsTilHit < 0f && !transparent)
        {
            transparent = true;
            SetAlpha(0.3f);
        }
        else if (beatsTilHit > 0f && transparent)
        {
            transparent = false;
            SetAlpha(1);
        }

        SetGlow(selected);
    }

    public static explicit operator NoteSerial(Note note)
    {
        return new NoteSerial(note.beat, note.x, note.y, note.color, note.cutDirection, note.angleOffset);
    }

    private void SetAlpha(float alpha)
    {
        List<Material> materials = gameObject.GetComponent<Renderer>().materials.ToList();

        foreach (Material material in materials)
        {
            var color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }

    private void SetGlow(bool glow)
    {
        Material material = gameObject.GetComponent<Renderer>().material;

        material.SetFloat("_commentIfZero_EnableOutlinePass", glow ? 1 : 0);
    }
}
