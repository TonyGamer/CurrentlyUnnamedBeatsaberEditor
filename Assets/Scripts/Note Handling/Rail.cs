using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rail : Colored, HasEnd
{
    public float tailBeat { get; set; }
    public int tailX;
    public int tailY;
    public int tailDirection;
    public int lengthMultiplier;
    public int tailLengthMultiplier;
    public int anchor;

    [Header("References")]
    public GameObject railEndInstance;

    [HideInInspector]
    public Vector3[] controlPoints;
    [HideInInspector]
    public LineRenderer lineRenderer;
    [HideInInspector]
    public MeshCollider meshCollider;
    [HideInInspector]
    public RailEnd railEnd;

    private int curveCount;
    private int segmentCount;

    void Start()
    {
        if (railEnd == null)
        {
            railEnd = Instantiate(railEndInstance).GetComponent<RailEnd>();
        }

        railEnd.railStart = this;

        railEnd.x = tailX;
        railEnd.y = tailY;
        railEnd.beat = beat;
        railEnd.posBeat = tailBeat;
        railEnd.cutDirection = tailDirection;

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

    new public void SetGlow(bool glow)
    {
        Material material = gameObject.GetComponent<LineRenderer>().material;

        material.SetFloat("_commentIfZero_EnableOutlinePass", glow ? 1 : 0);
    }

    public override void UpdateRotation()
    {
        tailDirection = railEnd.cutDirection;

        Quaternion rotation = Spawner.CalculateRotation(cutDirection, 0);
        float angle = -Mathf.Deg2Rad * rotation.eulerAngles.z;

        Quaternion tailRotation = Spawner.CalculateRotation(tailDirection, 0);
        float tailAngle = Mathf.Deg2Rad * tailRotation.eulerAngles.z;

        controlPoints = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(lengthMultiplier * Mathf.Sin(angle), lengthMultiplier * Mathf.Cos(angle), 0),
            new Vector3((tailX - x) + (tailLengthMultiplier * 5 * Mathf.Sin(tailAngle)), (tailY - tailY) + (tailLengthMultiplier * 5 * Mathf.Cos(tailAngle)), 0.5f * GlobalData.jumpSpeed * (tailBeat - beat)),
            new Vector3(tailX - x, tailY - y, 0.5f * GlobalData.jumpSpeed * (tailBeat - beat))
        };

        curveCount = (int)controlPoints.Length / 3;

        DrawCurve();
    }

    new public void Moved()
    {
        tailX = railEnd.x;
        tailY = railEnd.y;
        UpdateRotation();
    }
}
