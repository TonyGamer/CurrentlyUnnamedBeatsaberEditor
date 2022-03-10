using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Bomb : Spawnable
{
    public Bomb(float beat, int x, int y)
    {
        this.beat = beat;
        this.x = x;
        this.y = y;
    }

    public static explicit operator BombSerial(Bomb bomb)
    {
        return new BombSerial(bomb.beat, bomb.x, bomb.y);
    }
}
