using UnityEngine;

public abstract class Colored : Spawnable
{
    [Header("Colored")]
    public int cutDirection;
    public int color;

    public override void Update()
    {
        base.Update();

        UpdateRotation();
    }

    public abstract void UpdateRotation();
}
