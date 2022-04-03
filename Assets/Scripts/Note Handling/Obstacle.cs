using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Collider))]
public class Obstacle : Spawnable
{
    [Header("Obstacle")]
    public int height;
    public int width;
    public float duration;

    public Obstacle(float beat, int x, int y, int width, int height, float duration)
    {
        this.beat = beat;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.duration = duration;
    }

    public override void Update()
    {
        base.Update();

        float depth = 0.5f * duration * GlobalData.jumpSpeed;
        transform.localScale = new Vector3(width * 100, height * 100, depth * 100);

        Vector3 position = transform.position;
        position.z += depth / 2;
        transform.position = position;
    }

    public override void CheckForDestroy()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;

        if (!selected && (beatsTilHit + duration < -0.5f || beatsTilHit > GlobalData.HJD))
        {
            Destroy(gameObject);
        }
    }

    public static explicit operator ObstacleSerial(Obstacle obstacle)
    {
        return new ObstacleSerial(obstacle.beat, obstacle.x, obstacle.y, obstacle.width, obstacle.height, obstacle.duration);
    }
}
