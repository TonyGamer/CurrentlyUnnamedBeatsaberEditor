using UnityEngine;

public abstract class Selectable : MonoBehaviour
{
    [Header("Selectable")]
    protected bool selected = false;

    public virtual void SetGlow(bool glow)
    {
        Material material = gameObject.GetComponent<Renderer>().material;

        material.SetFloat("_commentIfZero_EnableOutlinePass", glow ? 1 : 0);
    }

    public virtual void Selected(bool selected)
    {
        this.selected = selected;
        SetGlow(selected);
    }

    public virtual bool GetSelected()
    {
        return selected;
    }

    public abstract Spawnable GetRoot();
}
