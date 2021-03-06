using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BurstSlider : Colored
{
    [Header("Burst Slider")]
    public float tailBeat;
    public int tailX;
    public int tailY;
    public int sliceCount;
    public float squash;

    [Header("References")]
    public GameObject elementObject;
    public Note head;

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

    public void Start()
    {
        if(elements.Length != sliceCount - 1)
        {
            elements = new GameObject[sliceCount - 1];

            for (int i = 0; i < (sliceCount - 1); i++)
            {
                elements[i] = Instantiate(elementObject, transform);
                elements[i].GetComponent<Renderer>().material.color = Spawner.GetColor(color);
                elements[i].GetComponent<SubMesh>().head = this;
            }

            UpdateRotation();
        }
    }

    public override void Update()
    {
        base.Update();

        SetHead(head);
    }

    void OnDestroy()
    {
        Spawner.RemoveSpawnable(gameObject);

        if (head != null)
        {
            head.SetHead(false);
        }
    }

    public override void UpdateRotation()
    {
        if(head != null)
        {
            head.cutDirection = this.cutDirection;
            head.UpdateRotation();
        }

        transform.rotation = Spawner.CalculateRotation(cutDirection, 0);
        float angle = -Mathf.Deg2Rad * transform.rotation.eulerAngles.z;

        controlPoints = new Vector3[] {
            new Vector3(0, 0, 0),
            new Vector3(Mathf.Sin(angle), Mathf.Cos(angle), 0),
            new Vector3(tailX - x, tailY - y, GlobalData.jumpSpeed * (tailBeat - beat))
        };

        DrawCurve();
    }

    public override void Moved()
    {
        if(head != null)
        {
            head.x = this.x;
            head.y = this.y;
        }
    }

    public override void Changed()
    {
        base.Changed();

        if(head != null)
        {
            head.Changed();
        }
    }

    void DrawCurve()
    {
        for (int i = 1; i <= sliceCount - 1; i++)
        {
            float t = squash * i / (sliceCount - 1);
            Vector3 pixel = CalculateQuadraticBezierPoint(t, controlPoints[0], controlPoints[1], controlPoints[2]);

            Vector3 lookDir = CalculateQuadraticBezierPointDerivative(t+.01f, controlPoints[0], controlPoints[1], controlPoints[2]);

            float angle = 90 + Mathf.Atan2(lookDir.y, lookDir.x) * Mathf.Rad2Deg;

            Quaternion rotation = Quaternion.Euler(0, 0, angle);

            elements[i - 1].transform.position = pixel + transform.position;
            elements[i - 1].transform.rotation = rotation;
        }
    }

    Vector3 CalculateQuadraticBezierPoint(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1f - t;
        float tt = t * t;
        float uu = u * u;

        Vector3 p = uu * p0; //first term
        p += 2 * u * t * p1; //second term
        p += tt * p2; //third term

        return p;
    }
    Vector3 CalculateQuadraticBezierPointDerivative(float t, Vector3 p0, Vector3 p1, Vector3 p2)
    {
        float u = 1f - t;

        Vector3 p = 2 * u * (p1 - p0); //first term
        p += 2 * t * (p2 - p1); // second term

        return p;
    }

    public override void SetAlpha(float alpha)
    {
        List<Material> materials = gameObject.GetComponent<Renderer>().materials.ToList();

        foreach (Material material in materials)
        {
            var color = material.color;
            color.a = alpha;
            material.color = color;
        }

        foreach (GameObject element in elements)
        {
            SubMesh elementComp = element.GetComponent<SubMesh>();

            elementComp.SetAlpha(alpha);
        }
    }

    public override void SetGlow(bool glow)
    {
        Material material = gameObject.GetComponent<Renderer>().material;

        material.SetFloat("_commentIfZero_EnableOutlinePass", glow ? 1 : 0);

        foreach(GameObject element in elements)
        {
            SubMesh elementComp = element.GetComponent<SubMesh>();

            elementComp.SetGlow(glow);
        }
    }

    public static explicit operator BurstSliderSerial(BurstSlider bs)
    {
        return new BurstSliderSerial(bs.beat, bs.x, bs.y, bs.color, bs.cutDirection, bs.tailBeat, bs.tailX, bs.tailY, bs.sliceCount, bs.squash);
    }

    public void SetHead(Note head)
    {
        this.head = head;

        if (head != null) {
            head.SetHead(true);
        }

        CheckHead();
    }

    public void CheckHead()
    {
        Renderer renderer = gameObject.GetComponent<Renderer>();

        renderer.enabled = head != null;
    }

    public override void Selected(bool selected)
    {
        base.Selected(selected);

        foreach(GameObject element in elements)
        {
            element.GetComponent<SubMesh>().SliderSelect(selected);
        }
    }
}
