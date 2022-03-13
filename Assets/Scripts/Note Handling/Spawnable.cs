using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Spawnable : MonoBehaviour
{
    [Header("Spawnable")]
    public int index;
    public float beat;
    public int x;
    public int y;

    public bool selected = false;

    protected bool transparent;

    public void Update()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;

        transform.position = Spawner.CalculatePosition(x, y, beat);

        if (!selected && (beatsTilHit < -0.5f || beatsTilHit > GlobalData.HJD))
        {
            UnityEngine.Object.Destroy(this.gameObject);
        }
        else if (beatsTilHit < 0f && !transparent)
        {
            transparent = true;
            SetAlpha(0.3f);
        }
        else if (beatsTilHit >= 0f && transparent)
        {
            transparent = false;
            SetAlpha(1);
        }

        SetGlow(selected);
    }

    void OnDestroy()
    {
        Spawner.RemoveSpawnable(gameObject);
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

    public virtual void Moved()
    {
        Changed();
    }

    public virtual void Changed()
    {
        MapManager.UpdateSpawnable(this);
    }
}
