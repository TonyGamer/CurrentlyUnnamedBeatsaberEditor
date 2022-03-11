using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]
public class Rail : Spawnable
{
    public int color;
    public int direction;
    public float tailBeat;
    public int tailX;
    public int tailY;
    public int tailDirection;
    public int lengthMultiplier;
    public int tailLengthMultiplier;
    public int anchor;

    [HideInInspector]
    public Vector3[] controlPoints;
    [HideInInspector]
    public LineRenderer lineRenderer;
    [HideInInspector]
    public MeshCollider meshCollider;

    private int curveCount = 0;
    private int layerOrder = 0;
    private int SEGMENT_COUNT = 50;

    void Start()
    {
        Quaternion rotation = Spawner.CalculateRotation(direction, 0);
        float angle = -Mathf.Deg2Rad * rotation.eulerAngles.z;

        Quaternion tailRotation = Spawner.CalculateRotation(tailDirection, 0);
        float tailAngle = Mathf.Deg2Rad * tailRotation.eulerAngles.z;

        controlPoints = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(lengthMultiplier * Mathf.Sin(angle), lengthMultiplier * Mathf.Cos(angle), 0),
            new Vector3((tailX - x) + (tailLengthMultiplier * 5 * Mathf.Sin(tailAngle)), (tailY - tailY) + (tailLengthMultiplier * 5 * Mathf.Cos(tailAngle)), 0.5f * GlobalData.jumpSpeed * (tailBeat - beat)),
            new Vector3(tailX - x, tailY - y, 0.5f * GlobalData.jumpSpeed * (tailBeat - beat))
        };

        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.sortingLayerID = layerOrder;
        curveCount = (int)controlPoints.Length / 3;

        meshCollider = GetComponent<MeshCollider>();

        DrawCurve();
    }

    void DrawCurve()
    {
        for (int j = 0; j < curveCount; j++)
        {
            for (int i = 1; i <= SEGMENT_COUNT; i++)
            {
                float t = i / (float)SEGMENT_COUNT;
                int nodeIndex = j * 3;
                Vector3 pixel = CalculateCubicBezierPoint(t, controlPoints[nodeIndex], controlPoints[nodeIndex + 1], controlPoints[nodeIndex + 2], controlPoints[nodeIndex + 3]);
                lineRenderer.positionCount = (j * SEGMENT_COUNT) + i;
                lineRenderer.SetPosition((j * SEGMENT_COUNT) + (i - 1), pixel);
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
}
