using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[RequireComponent(typeof(Camera))]
public class CameraScaler : MonoBehaviour
{
    public float multiplier = 4f;

    void Update()
    {
        var camera = GetComponent<Camera>();

        camera.orthographicSize = multiplier * ((float) Screen.height / Screen.width);
    }
}
