using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public abstract class SpawnableSerial : IComparable
{
    public float b; // Beat
    public int x;   // LineIndex
    public int y;   // LineLayer

    int IComparable.CompareTo(object obj)
    {
        SpawnableSerial spawnableSerial = obj as SpawnableSerial;

        return (int)(-Mathf.Sign(spawnableSerial.b - this.b));
    }
}
