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

    new public void Update()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;

        Vector3 position = Spawner.CalculatePosition((float)x + (width - 1f)/2, (float)y + (height - 1f)/2 - 1, beat);
        float depth = 0.5f * duration * GlobalData.jumpSpeed;
        position.z += depth / 2;
        position.z -= .5f;

        transform.position = position;

        if (beatsTilHit + duration < -0.5f || beatsTilHit > GlobalData.HJD)
        {
            UnityEngine.Object.Destroy(this.gameObject);
        }
        else if (beatsTilHit < 0f && !transparent)
        {
            transparent = true;
            SetAlpha(0.3f);
        }
        else if (beatsTilHit > 0f && transparent)
        {
            transparent = false;
            SetAlpha(1);
        }

        SetGlow(selected);
    }

    public static explicit operator ObstacleSerial(Obstacle obstacle)
    {
        return new ObstacleSerial(obstacle.beat, obstacle.x, obstacle.y, obstacle.width, obstacle.height, obstacle.duration);
    }
}
