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

    [Header("Camera Controls")]
    public KeyCode Forwards = KeyCode.W;
    public KeyCode Backwards = KeyCode.S;
    public KeyCode Left = KeyCode.A;
    public KeyCode Right = KeyCode.D;
    public KeyCode Sprint = KeyCode.LeftShift;

    [Header("Editor Controls")]
    public KeyCode Menu = KeyCode.Escape;
    public KeyCode Save = KeyCode.S;

    public KeyCode Pause = KeyCode.Space;
    public KeyCode SeekBack = KeyCode.LeftArrow;
    public KeyCode SeekForward = KeyCode.RightArrow;
    public KeyCode PrecisionUp = KeyCode.UpArrow;
    public KeyCode PrecisionDown = KeyCode.DownArrow;

    [Header("NoteControls")]
    public KeyCode RotLeft = KeyCode.Q;
    public KeyCode RotRight = KeyCode.E;

    [Header("Key Repeat")]
    public float repeatStart = 0.5f;
    public float repeatDelay = 0.05f;

    [Header("References")]
    public MenuManager menuManager;
    public MapManager mapManager;
    public EditorController editorController;

    private Vector3 _moveSpeed;

    private Key MenuKey;

    private Key PauseKey;
    private Key SeekBackKey;
    private Key SeekForeKey;
    private Key PrecUpKey;
    private Key PrecDownKey;

    private Key RotLeftKey;
    private Key RotRightKey;

    void Start()
    {
        _moveSpeed = Vector3.zero;

        MenuKey = new Key(Menu);

        PauseKey = new Key(Pause);
        SeekBackKey = new Key(SeekBack);
        SeekForeKey = new Key(SeekForward);
        PrecUpKey = new Key(PrecisionUp);
        PrecDownKey = new Key(PrecisionDown);

        RotLeftKey = new Key(RotLeft);
        RotRightKey = new Key(RotRight);
    }

    // Update is called once per frame
    void Update()
    {
        HandleEditorControls();

        var acceleration = Vector3.zero;

        if (!menuManager.gameObject.activeSelf && Input.GetMouseButton(1))
        {
            Cursor.lockState = CursorLockMode.Locked;
            Cursor.visible = false;

            HandleMouseRotation();
            acceleration = HandleCameraAcceleration();
        }
        else
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
        // Mouse input
        var rotationHorizontal = Time.deltaTime * XAxisSensitivity * Input.GetAxis("Mouse X");
        var rotationVertical = Time.deltaTime * YAxisSensitivity * Input.GetAxis("Mouse Y");

        // Applying mouse rotation
        // Always rotate Y in global world space to avoid gimbal lock
        transform.Rotate(Vector3.up * rotationHorizontal, Space.World);

        var rotationY = transform.localEulerAngles.y;

        _rotationX += rotationVertical;
        _rotationX = Mathf.Clamp(_rotationX, -MaxXAngle, MaxXAngle);

        transform.localEulerAngles = new Vector3(-_rotationX, rotationY, 0);
    }

    private void HandleDeceleration(Vector3 acceleration)
    {
        // Deceleration functionality
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
        KeyRepeat(ref MenuKey, ToggleMenu, 0);

        // Seeking Controls
        KeyRepeat(ref PauseKey, TogglePaused, 0);

        if (Input.GetKey(KeyCode.LeftControl))
        {
            ChangePrecision(-(int)Input.mouseScrollDelta.y);

            if (Input.GetKeyDown(Save)) mapManager.SaveDiff();
        }
        else if (Input.GetKey(KeyCode.LeftAlt))
        {
            editorController.ChangeBeat((int)Input.mouseScrollDelta.y);
        }
        else
        {
            KeyRepeat(ref PrecUpKey, ChangePrecision, -1);
            KeyRepeat(ref PrecDownKey, ChangePrecision, 1);
            KeyRepeat(ref SeekBackKey, Seek, (float)-GlobalData.beatPrecision);
            KeyRepeat(ref SeekForeKey, Seek, (float)GlobalData.beatPrecision);

            KeyRepeat(ref RotLeftKey, RotateCurrent, -1);
            KeyRepeat(ref RotRightKey, RotateCurrent, 1);
        }
    }

    public void ToggleMenu(float _)
    {
        menuManager.gameObject.SetActive(!menuManager.gameObject.activeSelf);
    }

    public void TogglePaused(float _)
    {
        mapManager.ResyncAudio();
        GlobalData.paused = !GlobalData.paused;
    }

    public void ChangePrecision(float amount)
    {
        int amt = (int)amount;

        GlobalData.beatPrecision *= Mathf.Pow(2, amount);
    }

    public void Seek(float amount)
    {
        GlobalData.currentBeat += amount;
        mapManager.ResyncAudio();
    }

    public void RotateCurrent(float direction)
    {
        editorController.RotateCurrent(direction);
    }

    public void KeyRepeat(ref Key key, Action<float> func, float argument)
    {
        if (Input.GetKey(key.code))
        {
            key.timer += Time.deltaTime;

            if (key.timer >= repeatStart)
            {
                key.prev = false;
                key.timer = repeatStart - repeatDelay;
            }

            if (!key.prev)
            {
                func(argument);
                key.prev = true;
            }
        }
        else
        {
            key.timer = 0;
            key.prev = false;
        }
    }
}

public struct Key
{
    public Key(KeyCode keyCode)
    {
        this.code = keyCode;
        this.timer = 0f;
        this.prev = false;
    }

    public KeyCode code { get; set; }
    public float timer { get; set; }
    public bool prev { get; set; }
}