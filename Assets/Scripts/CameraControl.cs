// Much of this code is borrowed from https://stackoverflow.com/questions/58328209/how-to-make-a-free-fly-camera-script-in-unity-with-acceleration-and-decceleratio

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraControl : MonoBehaviour
{
    [Header("Constants")]
    //unity controls and constants input
    public float AccelerationMod;
    public float XAxisSensitivity;
    public float YAxisSensitivity;
    public float DecelerationMod;

    [Space]
    [Range(0, 89)] public float MaxXAngle = 60f;
    [Space]

    public float SprintSpeed = 0.1f;
    public float NormalSpeed = 0.03f;
    private float MaximumMovementSpeed = 0;

    [Header("Controls")]
    public KeyCode Forwards = KeyCode.W;
    public KeyCode Backwards = KeyCode.S;
    public KeyCode Left = KeyCode.A;
    public KeyCode Right = KeyCode.D;
    public KeyCode Up = KeyCode.Q;
    public KeyCode Down = KeyCode.E;
    public KeyCode Sprint = KeyCode.LeftShift;

    [Header("Key Repeat")]
    public float repeatStart = 0.5f;
    public float repeatDelay = 0.05f;

    [Header("References")]
    public MenuManager menuManager;
    public MapManager mapManager;

    private Vector3 _moveSpeed;

    // Single click buttons
    private bool menuPrev = false;
    private bool pausePrev = false;
    private bool upPrev = false;
    private bool downPrev = false;
    private bool leftPrev = false;
    private bool rightPrev = false;

    // Button Repeat Timers
    private float upTimer = 0;
    private float downTimer = 0;
    private float leftTimer = 0;
    private float rightTimer = 0;

    private void Start()
    {
        _moveSpeed = Vector3.zero;
    }

    // Update is called once per frame
    private void Update()
    {
        HandleEditorControls();

        var acceleration = Vector3.zero;

        if (!menuManager.gameObject.activeSelf)
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            HandleMouseRotation();
            acceleration = HandleCameraAcceleration();
        } else
        {
            Cursor.lockState = CursorLockMode.None;
            Cursor.visible = true;
        }

        _moveSpeed += Time.deltaTime * acceleration;

        HandleDeceleration(Time.deltaTime * acceleration);

        // clamp the move speed
        if (_moveSpeed.magnitude > MaximumMovementSpeed)
        {
            _moveSpeed = _moveSpeed.normalized * MaximumMovementSpeed;
        }

        transform.Translate(_moveSpeed);
    }

    private Vector3 HandleCameraAcceleration()
    {
        var acceleration = Vector3.zero;

        if (Input.GetKey(Forwards))
        {
            acceleration.z += 1;
        }

        if (Input.GetKey(Backwards))
        {
            acceleration.z -= 1;
        }

        if (Input.GetKey(Left))
        {
            acceleration.x -= 1;
        }

        if (Input.GetKey(Right))
        {
            acceleration.x += 1;
        }

        if (Input.GetKey(Up))
        {
            acceleration.y += 1;
        }

        if (Input.GetKey(Down))
        {
            acceleration.y -= 1;
        }

        if (Input.GetKey(Sprint))
        {
            MaximumMovementSpeed = SprintSpeed;
        }
        else
        {
            MaximumMovementSpeed = NormalSpeed;
        }

        return acceleration.normalized * AccelerationMod;
    }

    private float _rotationX;

    private void HandleMouseRotation()
    {
        //mouse input
        var rotationHorizontal = Time.deltaTime * XAxisSensitivity * Input.GetAxis("Mouse X");
        var rotationVertical = Time.deltaTime * YAxisSensitivity * Input.GetAxis("Mouse Y");

        //applying mouse rotation
        // always rotate Y in global world space to avoid gimbal lock
        transform.Rotate(Vector3.up * rotationHorizontal, Space.World);

        var rotationY = transform.localEulerAngles.y;

        _rotationX += rotationVertical;
        _rotationX = Mathf.Clamp(_rotationX, -MaxXAngle, MaxXAngle);

        transform.localEulerAngles = new Vector3(-_rotationX, rotationY, 0);
    }

    private void HandleDeceleration(Vector3 acceleration)
    {
        //deceleration functionality
        if (Mathf.Approximately(Mathf.Abs(acceleration.x), 0))
        {
            if (Mathf.Abs(_moveSpeed.x) < DecelerationMod)
            {
                _moveSpeed.x = 0;
            }
            else
            {
                _moveSpeed.x -= DecelerationMod * Mathf.Sign(_moveSpeed.x);
            }
        }

        if (Mathf.Approximately(Mathf.Abs(acceleration.y), 0))
        {
            if (Mathf.Abs(_moveSpeed.y) < DecelerationMod)
            {
                _moveSpeed.y = 0;
            }
            else
            {
                _moveSpeed.y -= DecelerationMod * Mathf.Sign(_moveSpeed.y);
            }
        }

        if (Mathf.Approximately(Mathf.Abs(acceleration.z), 0))
        {
            if (Mathf.Abs(_moveSpeed.z) < DecelerationMod)
            {
                _moveSpeed.z = 0;
            }
            else
            {
                _moveSpeed.z -= DecelerationMod * Mathf.Sign(_moveSpeed.z);
            }
        }
    }

    private void HandleEditorControls()
    {
        // Menu
        if (Input.GetKey(KeyCode.Escape))
        {
            if (!menuPrev)
            {
                menuManager.gameObject.SetActive(!menuManager.gameObject.activeSelf);
                GlobalData.paused = true;
                menuPrev = true;
            }
        }
        else
        {
            menuPrev = false;
        }

        // Seeking Controls
        if (Input.GetKey(KeyCode.Space))
        {
            if (!pausePrev)
            {
                mapManager.ResyncAudio();
                GlobalData.paused = !GlobalData.paused;
                pausePrev = true;
            }
        }
        else
        {
            pausePrev = false;
        }

        if (Input.GetKey(KeyCode.UpArrow))
        {
            upTimer += Time.deltaTime;

            if (upTimer >= repeatStart)
            {
                upPrev = false;
                upTimer = repeatStart - repeatDelay;
            }

            if (!upPrev)
            {
                GlobalData.beatPrecision /= 2;
                upPrev = true;
            }
        }
        else
        {
            upPrev = false;
            upTimer = 0;
        }

        if (Input.GetKey(KeyCode.DownArrow))
        {
            downTimer += Time.deltaTime;

            if (downTimer >= repeatStart)
            {
                downPrev = false;
                downTimer = repeatStart - repeatDelay;
            }

            if (!downPrev)
            {
                GlobalData.beatPrecision *= 2;
                downPrev = true;
            }
        }
        else
        {
            downPrev = false;
            downTimer = 0;
        }

        if (Input.GetKey(KeyCode.LeftArrow))
        {
            leftTimer += Time.deltaTime;

            if (leftTimer >= repeatStart)
            {
                leftPrev = false;
                leftTimer = repeatStart - repeatDelay;
            }

            if (!leftPrev)
            {
                GlobalData.currentBeat -= GlobalData.beatPrecision;
                mapManager.ResyncAudio();
                leftPrev = true;
            }
        }
        else
        {
            leftPrev = false;
            leftTimer = 0;
        }

        if (Input.GetKey(KeyCode.RightArrow))
        {
            rightTimer += Time.deltaTime;

            if (rightTimer >= repeatStart)
            {
                rightPrev = false;
                rightTimer = repeatStart - repeatDelay;
            }

            if (!rightPrev)
            {
                GlobalData.currentBeat += GlobalData.beatPrecision;
                mapManager.ResyncAudio();
                rightPrev = true;
            }
        }
        else
        {
            rightPrev = false;
            rightTimer = 0;
        }
    }
}