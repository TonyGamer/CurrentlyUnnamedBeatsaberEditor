using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Spawnable : MonoBehaviour
{
    [Header("Spawnable")]
    public float beat;
    public int x;
    public int y;

    public bool selected = false;

    protected bool transparent;

    public void Update()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;

        transform.position = Spawner.calculatePosition(x, y, beat);

        if (beatsTilHit < -0.5f || beatsTilHit > GlobalData.HJD)
        {
            UnityEngine.Object.Destroy(this.gameObject);
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

    public void SetAlpha(float alpha)
    {
        List<Material> materials = gameObject.GetComponent<Renderer>().materials.ToList();

        foreach (Material material in materials)
        {
            var color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }

    public void SetGlow(bool glow)
    {
        Material material = gameObject.GetComponent<Renderer>().material;

        material.SetFloat("_commentIfZero_EnableOutlinePass", glow ? 1 : 0);
    }
}
