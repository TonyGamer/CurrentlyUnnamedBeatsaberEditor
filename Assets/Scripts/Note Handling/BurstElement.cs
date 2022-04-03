using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class BurstElement : Selectable
{
    public BurstSlider head;

    public BurstElement(BurstSlider head)
    {
        this.head = head;
    }

    public override void Selected(bool selected)
    {
        head.Selected(selected);
    }

    public void SliderSelect(bool selected)
    {
        base.Selected(selected);
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

    public override Spawnable GetRoot()
    {
        return head;
    }
}
