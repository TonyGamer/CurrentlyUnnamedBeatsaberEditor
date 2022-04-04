using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class Obstacle : Spawnable
{
    [Header("Obstacle")]
    public int height;
    public int width;
    public float duration;
    protected float depth;

    [Space]
    public SubMesh mesh;
    public Gizmo xGizmo;
    public Gizmo yGizmo;
    public Gizmo zGizmo;

    public Obstacle(float beat, int x, int y, int width, int height, float duration)
    {
        this.beat = beat;
        this.x = x;
        this.y = y;
        this.width = width;
        this.height = height;
        this.duration = duration;
    }

    public void Start()
    {
        UpdateSize();
        UpdateGizmos();
        UpdateSizeFromGizmos();
    }

    public override void Update()
    {
        mesh.transform.position = Spawner.CalculatePosition(x + (width - 1f) / 2, y + (height / 2) - 1f, beat + (duration / 2));

        CheckForDestroy();

        float beatsTilHit = beat - GlobalData.currentBeat;
        if (beatsTilHit < 0f && !transparent)
        {
            transparent = true;
            SetAlpha(0.3f);
        }
        else if (beatsTilHit >= 0f && transparent)
        {
            transparent = false;
            SetAlpha(1);
        }
    }

    public override void Changed()
    {
        UpdateSizeFromGizmos();
        UpdateGizmos();
        UpdateSize();
        base.Changed();
    }

    public void UpdateSize()
    {
        depth = Spawner.CalculateZOffset(GlobalData.currentBeat + duration);
        mesh.transform.localScale = new Vector3(width, height, depth);

        Update();
    }

    public void UpdateGizmos()
    {
        float xCenter = x + (width - 1) / 2;
        float yCenter = y + (height / 2) - 1;
        float zCenter = beat + (duration / 2);

        xGizmo.floatX = x + width;
        xGizmo.floatY = yCenter;
        xGizmo.beat = zCenter;

        yGizmo.floatX = xCenter;
        yGizmo.floatY = y + height - 1;
        yGizmo.beat = zCenter;

        zGizmo.floatX = xCenter;
        zGizmo.floatY = yCenter;
        zGizmo.beat = beat + duration + (0.0625f * Sign(duration));
    }

    public void UpdateSizeFromGizmos()
    {
        width = (int)(xGizmo.floatX - x);
        height = (int)(yGizmo.floatY + 1 - y);
        duration = zGizmo.beat - (0.0625f * Sign(duration)) - beat;
    }

    public override void SetAlpha(float alpha)
    {
        return;
    }

    public override void SetGlow(bool glow)
    {
        Material material = mesh.GetComponent<Renderer>().material;

        material.SetFloat("_commentIfZero_EnableOutlinePass", glow ? 1 : 0);
    }

    public override void CheckForDestroy()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;

        if (!selected && (beatsTilHit + duration < -0.5f || beatsTilHit > GlobalData.HJD))
        {
            Destroy(gameObject);
        }
    }

    public override void Selected(bool selected)
    {
        Debug.Log(selected);
        xGizmo.gameObject.SetActive(selected);
        yGizmo.gameObject.SetActive(selected);
        zGizmo.gameObject.SetActive(selected);
    }

    public static explicit operator ObstacleSerial(Obstacle obstacle)
    {
        return new ObstacleSerial(obstacle.beat, obstacle.x, obstacle.y, obstacle.width, obstacle.height, obstacle.duration);
    }

    private float Sign(float val)
    {
        if(Mathf.Abs(val) < (1 / 256f)){
            return 0;
        }

        return System.Math.Sign(val);
    }
}
