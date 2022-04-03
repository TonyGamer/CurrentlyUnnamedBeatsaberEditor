using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BurstSlider : Colored, HasEnd
{
    public float tailBeat { get; set; }
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

    public BurstSlider(float beat, int x, int y, int color, int cutDirection, float tailBeat, int tailX, int tailY, int sliceCount, float squash)
    {
        this.beat = beat;
        this.x = x;
        this.y = y;
        this.color = color;
        this.cutDirection = cutDirection;
        this.tailBeat = tailBeat;
        this.tailX = tailX;
        this.tailY = tailY;
        this.sliceCount = sliceCount;
        this.squash = squash;
    }

    void Start()
    {
        elements = new GameObject[sliceCount];

        for(int i = 0; i < sliceCount; i++)
        {
            elements[i] = Instantiate(elementObject, transform);
            elements[i].GetComponent<Renderer>().material.color = Spawner.GetColor(color);
        }

        UpdateRotation();
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

    public override void UpdateRotation()
    {
        transform.rotation = Spawner.CalculateRotation(cutDirection, 0);
        float angle = -Mathf.Deg2Rad * transform.rotation.eulerAngles.z;

        controlPoints = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0),
            new Vector3(tailX - x, tailY - y, GlobalData.jumpSpeed * (tailBeat - beat))
        };

        DrawCurve();

        Changed();
    }

    void DrawCurve()
    {
        for (int i = 1; i <= sliceCount; i++)
        {
            float t = squash * i / (float)sliceCount;
            Vector3 pixel = CalculateQuadraticBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2]);

            Vector3 deltaPixel = pixel - CalculateQuadraticBezierPoint(t+0.01f, controlPoints[0], controlPoints[1], controlPoints[2]);

            deltaPixel.z = 0;

            Quaternion rotation = Quaternion.identity;
            rotation.SetLookRotation(deltaPixel);

            rotation *= Quaternion.Euler((Vector3.up + Vector3.forward)*90);

            elements[(int)i - 1].transform.position = pixel + transform.position;
            elements[(int)i - 1].transform.rotation = rotation;
        }
    }

    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;
        float uuu = uu * u;
        float ttt = tt * t;

        Vector3 p = uu * p0; //first term
        p += 2 * u * t * p1; //second term
        p += tt * p2; //third term

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

    public static explicit operator BurstSliderSerial(BurstSlider bs)
    {
        return new BurstSliderSerial(bs.beat, bs.x, bs.y, bs.color, bs.cutDirection, bs.tailBeat, bs.tailX, bs.tailY, bs.sliceCount, bs.squash);
    }
}
