using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class RailEnd : Colored
{
    public float posBeat;

    [HideInInspector]
    public Rail railStart;

    void Start()
    {
        UpdateRotation();
    }

    new void Update()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;

        transform.position = Spawner.CalculatePosition(x, y, posBeat);

        if (beatsTilHit < -0.5f || beatsTilHit > GlobalData.HJD)
        {
            UnityEngine.Object.Destroy(this.gameObject);
        }

        SetGlow(selected);
    }

    public override void Moved()
    {
        railStart.Moved();
    }

    public override void UpdateRotation()
    {
        railStart.UpdateRotation();
    }
}
