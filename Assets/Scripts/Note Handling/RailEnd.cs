using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RailEnd : Colored
{
    public float posBeat;

    private float prevBeat;

    [HideInInspector]
    public Rail railStart;

    void Start()
    {
        UpdateRotation();
    }

    public override void Moved()
    {
        posBeat += prevBeat - beat;
        beat = prevBeat;
        railStart.Moved();
    }

    public override void UpdateRotation()
    {
        railStart.UpdateRotation();
    }

    public override void SetGlow(bool glow)
    {
        return;
    }

    public override void SetAlpha(float alpha)
    {
        return;
    }
}
