using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BurstSlider : Spawnable
{
    public int color;
    public int direction;
    public float tailBeat;
    public int tailX;
    public int tailY;
    public int sliceCount;
    public float squash;

    [Header("References")]
    public GameObject elementObject;

    [HideInInspector]
    public Vector3[] controlPoints;
    [HideInInspector]
    public GameObject[] elements;

    private int curveCount = 0;

    void Start()
    {
        Quaternion rotation = Spawner.CalculateRotation(direction, 0);
        float angle = -Mathf.Deg2Rad * rotation.eulerAngles.z;

        controlPoints = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0),
            new Vector3(tailX - x, tailY - y, 0.5f * GlobalData.jumpSpeed * (tailBeat - beat))
        };

        elements = new GameObject[sliceCount];

        curveCount = (int)controlPoints.Length / 2;

        DrawCurve();
    }

    new public void Update()
    {
        float beatsTilHit = beat - GlobalData.currentBeat;

        transform.position = Spawner.CalculatePosition(x, y, beat);

        if (beatsTilHit < -0.5f || beatsTilHit > GlobalData.HJD)
        {
            UnityEngine.Object.Destroy(this.gameObject);
        }
        else if (beatsTilHit < 0f && !transparent)
        {
            transparent = true;
            SetAlpha(0.3f);
        }
        else if (beatsTilHit >= 0f && transparent)
        {
            transparent = false;
            SetAlpha(1);
        }

        SetGlow(selected);
    }

    void DrawCurve()
    {
        for (int j = 0; j < curveCount; j++)
        {
            for (int i = 1; i <= sliceCount; i++)
            {
                float t = squash * i / (float)sliceCount;
                int nodeIndex = (int)(squash * j * 3f);
                Vector3 pixel = CalculateQuadraticBezierPoint(t, controlPoints[nodeIndex], controlPoints[nodeIndex + 1], controlPoints[nodeIndex + 2]);

                Vector3 deltaPixel = pixel - CalculateQuadraticBezierPoint(t+0.01f, controlPoints[nodeIndex], controlPoints[nodeIndex + 1], controlPoints[nodeIndex + 2]);

                deltaPixel.z = 0;

                Quaternion rotation = Quaternion.identity;
                rotation.SetLookRotation(deltaPixel);

                rotation *= Quaternion.Euler((Vector3.up + Vector3.forward)*90);

                GameObject spawnedElement = Instantiate(elementObject, pixel +  transform.position, rotation, transform);

                BurstElement elementComp = spawnedElement.GetComponent<BurstElement>();
                
                spawnedElement.GetComponent<Renderer>().material.color = Spawner.GetColor(color);
                
                elements[(int)i - 1] = spawnedElement;
            }

        }
    }

    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1 - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uuu * p0;
        p += 3 * uu * t * p1;
        p += 3 * u * tt * p2;

        return p;
    }

    new public void SetAlpha(float alpha)
    {
        List<Material> materials = gameObject.GetComponent<Renderer>().materials.ToList();

        foreach (Material material in materials)
        {
            var color = material.color;
            color.a = alpha;
            material.color = color;
        }

        for(int i = 0; i < sliceCount; i++)
        {
            elements[i].GetComponent<BurstElement>().SetAlpha(alpha);
        }
    }

    new public void SetGlow(bool glow)
    {
        Material material = gameObject.GetComponent<Renderer>().material;

        material.SetFloat("_commentIfZero_EnableOutlinePass", glow ? 1 : 0);
    }
}
