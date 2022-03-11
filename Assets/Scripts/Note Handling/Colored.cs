using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class Colored : Spawnable
{
    [Header("Colored")]
    public int cutDirection;
    public int color;

    public abstract void UpdateRotation();
}
