using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Note : MonoBehaviour
{
    [Header("Note Type")]
    public float time;
    public int lineIndex;
    public int lineLayer;
    public int cutDirection;
    public int type = 0;

    private bool transparent;

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
        float beatsTilHit = time - GlobalData.currentBeat;

        transform.position = new Vector3(lineIndex - 1.5f, lineLayer + 0.5f, 0.5f * GlobalData.jumpSpeed * beatsTilHit);

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
    }

    public static explicit operator NoteSerial(Note note)
    {
        return new NoteSerial(note.time, note.lineIndex, note.lineLayer, note.type, note.cutDirection);
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
}
