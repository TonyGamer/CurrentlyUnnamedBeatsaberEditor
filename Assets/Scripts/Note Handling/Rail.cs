using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rail : Colored
{
    [Header("Rail")]
    public float tailBeat;
    public int tailX;
    public int tailY;
    public int tailDirection;
    public int lengthMultiplier;
    public int tailLengthMultiplier;
    public int anchor;
    public int segmentCount;

    public Gizmo railEnd;

    [HideInInspector]
    public Vector3[] controlPoints;
    [HideInInspector]
    public LineRenderer lineRenderer;
    [HideInInspector]
    public MeshCollider meshCollider;

    private int curveCount;

    private const int LENGTH_CONSTANT = 3;

    public Rail(float beat, int x, int y, int color, int cutDirection, float tailBeat, int tailX, int tailY, int tailDirection, int lengthMultiplier, int tailLengthMultiplier, int anchor)
    {
        this.beat = beat;
        this.x = x;
        this.y = y;
        this.color = color;
        this.cutDirection = cutDirection;
        this.tailBeat = tailBeat;
        this.tailX = tailX;
        this.tailY = tailY;
        this.tailDirection = tailDirection;
        this.lengthMultiplier = lengthMultiplier;
        this.tailLengthMultiplier = tailLengthMultiplier;
        this.anchor = anchor;
    }

    public void Start()
    {
        if (railEnd == null)
        {
            Debug.LogError("No rail end was assigned!");
            Destroy(this);
        }

        railEnd.parent = this;

        railEnd.floatX = tailX;
        railEnd.floatY = tailY;
        railEnd.beat = tailBeat;
        railEnd.direction = tailDirection;

        lineRenderer = GetComponent<LineRenderer>();
        meshCollider = GetComponent<MeshCollider>();

        segmentCount = Mathf.Max(30, (int)(10 * Mathf.Abs(tailBeat - beat)));

        UpdateRotation();
    }

    void DrawCurve()
    {
        for (int j = 0; j < curveCount; j++)
        {
            for (int i = 1; i <= segmentCount; i++)
            {
                float t = i / (float)segmentCount;
                int nodeIndex = j * 3;
                Vector3 pixel = CalculateCubicBezierPoint(t, controlPoints[nodeIndex], controlPoints[nodeIndex + 1], controlPoints[nodeIndex + 2], controlPoints[nodeIndex + 3]);
                lineRenderer.positionCount = (j * segmentCount) + i;
                lineRenderer.SetPosition((j * segmentCount) + (i - 1), pixel);
            }
        }

        Mesh mesh = new Mesh();
        lineRenderer.BakeMesh(mesh, false);
        meshCollider.sharedMesh = mesh;
    }

    Vector3 CalculateCubicBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2, Vector3 p3)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;
        p += ttt * p3;

        return p;
    }

    new public void SetAlpha(float alpha)
    {
        List<Material> materials = gameObject.GetComponent<LineRenderer>().materials.ToList();

        foreach (Material material in materials)
        {
            var color = material.color;
            color.a = alpha;
            material.color = color;
        }
    }

    public override void SetGlow(bool glow)
    {
        railEnd.Selected(glow);

        Material material = gameObject.GetComponent<LineRenderer>().material;

        material.SetFloat("_commentIfZero_EnableOutlinePass", glow ? 1 : 0);
    }

    public override void UpdateRotation()
    {
        tailDirection = railEnd.direction;

        Quaternion rotation = Spawner.CalculateRotation(cutDirection, 0);
        float angle = -Mathf.Deg2Rad * rotation.eulerAngles.z;

        Quaternion tailRotation = Spawner.CalculateRotation(tailDirection, 0);
        float tailAngle = Mathf.Deg2Rad * tailRotation.eulerAngles.z;

        controlPoints = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(lengthMultiplier * LENGTH_CONSTANT * Mathf.Sin(angle), lengthMultiplier * LENGTH_CONSTANT * Mathf.Cos(angle), 0),
            new Vector3((tailX - x) + (tailLengthMultiplier * LENGTH_CONSTANT * Mathf.Sin(tailAngle)), (tailY - y) - (tailLengthMultiplier * LENGTH_CONSTANT * Mathf.Cos(tailAngle)), GlobalData.jumpSpeed * (tailBeat - beat)),
            new Vector3(tailX - x, tailY - y, GlobalData.jumpSpeed * (tailBeat - beat))
        };

        curveCount = (int)controlPoints.Length / 3;

        DrawCurve();
    }

    public override void Moved()
    {
        tailX = (int)railEnd.x;
        tailY = (int)railEnd.y;
        tailBeat = railEnd.beat;
        UpdateRotation();
        Changed();
    }

    public static explicit operator SliderSerial(Rail rail)
    {
        return new SliderSerial(rail.beat, rail.x, rail.y, rail.color, rail.cutDirection, rail.tailBeat, rail.tailX, rail.tailY, rail.tailDirection, rail.lengthMultiplier, rail.tailLengthMultiplier, rail.anchor);
    }

    public override void CheckForDestroy()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;
        float beatsTilEnd = tailBeat - GlobalData.currentBeat;

        if (!selected && (beatsTilEnd < -0.5f || beatsTilHit > GlobalData.HJD))
        {
            Destroy(gameObject);
        }
    }

    void OnDestroy()
    {
        if(railEnd != null)
        {
            Spawner.RemoveSpawnable(gameObject);
            Destroy(railEnd);
        }
    }
}
