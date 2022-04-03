using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Spawnable : Selectable
{
    [Header("Spawnable")]
    public int index;
    public float beat;
    public int x;
    public int y;

    protected bool transparent;

    public virtual void Update()
    {
        transform.position = Spawner.CalculatePosition(x, y, beat);

        CheckForDestroy();

        float beatsTilHit = beat - GlobalData.currentBeat;
        if (beatsTilHit < 0f && !transparent)
        {
            transparent = true;
            SetAlpha(0.3f);
        }
        else if (beatsTilHit >= 0f && transparent)
        {
            transparent = false;
            SetAlpha(1);
        }
    }

    void OnDestroy()
    {
        Spawner.RemoveSpawnable(gameObject);
    }

    public virtual void SetAlpha(float alpha)
    {
        List<Material> materials = gameObject.GetComponent<Renderer>().materials.ToList();

        foreach (Material material in materials)
        {
            var color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }

    public virtual void Moved()
    {
        Changed();
    }

    public virtual void Changed()
    {
        MapManager.UpdateSpawnable(this);
    }

    public virtual void CheckForDestroy()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;
        if (!selected && (beatsTilHit < -0.5f || beatsTilHit > GlobalData.HJD))
        {
            Spawner.Destroy(gameObject);
        }
    }

    public override Spawnable GetRoot()
    {
        return this;
    }
}
