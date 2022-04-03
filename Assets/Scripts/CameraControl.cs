// Much of this code is borrowed from https://stackoverflow.com/questions/58328209/how-to-make-a-free-fly-camera-script-in-unity-with-acceleration-and-decceleratio

using static EditorController;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;

public class CameraControl : MonoBehaviour
{
    // Cursor Control
    [DllImport("user32.dll")]
    public static extern bool SetCursorPos(int X, int Y);
    [DllImport("user32.dll")]
    public static extern bool GetCursorPos(out POINT lpPoint);

    [Header("Constants")]
    // Unity controls and constants input
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

    public KeyCode Note = KeyCode.Alpha1;
    public KeyCode Bomb = KeyCode.Alpha2;
    public KeyCode Wall = KeyCode.Alpha3;
    public KeyCode Stack = KeyCode.Alpha4;
    public KeyCode Rail = KeyCode.Alpha5;

    public KeyCode Delete = KeyCode.Delete;

    [Header("NoteControls")]
    public KeyCode RotLeft = KeyCode.Q;
    public KeyCode RotRight = KeyCode.E;

    public KeyCode Move = KeyCode.G;

    [Header("Key Repeat")]
    public float repeatStart = 0.5f;
    public float repeatDelay = 0.05f;

    [Header("Spawnable Placement")]
    public SpawnableType spawnableToPlace = SpawnableType.Note;
    public int spawnableColor = 0;

    private Spawnable hitSpawnable;
    private Selectable lastHitSelectable;

    private Plane hitPlane;
    private Vector3 offset;

    [Header("Images")]
    public Sprite NoteImage;
    public Sprite BombImage;
    public Sprite WallImage;
    public Sprite StackImage;
    public Sprite RailImage;

    [Header("References")]
    public MenuManager menuManager;
    public MapManager mapManager;
    public EditorController editorController;
    public Image indicator;

    private Vector3 _moveSpeed;

    private Key MenuKey;

    private Key PauseKey;
    private Key SeekBackKey;
    private Key SeekForeKey;
    private Key PrecUpKey;
    private Key PrecDownKey;

    private Key RotLeftKey;
    private Key RotRightKey;

    private bool movingObject;
    private bool movingObjectPrev;

    private IEnumerator ghostRoutine;

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

        // Clamp the move speed
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
        KeyRepeat(ref MenuKey, ToggleMenu);

        if (!menuManager.gameObject.activeSelf)
        {
            // Seeking Controls
            KeyRepeat(ref PauseKey, TogglePaused);

            if (Input.GetKey(KeyCode.LeftControl))
            {
                ChangePrecision(-(int)Input.mouseScrollDelta.y);

                if (Input.GetKeyDown(Save)) mapManager.SaveDiff();
            }
            else if (Input.GetKey(KeyCode.LeftAlt))
            {
                if(hitSpawnable != null)
                {
                    ChangeBeat((int)Input.mouseScrollDelta.y);
                }
            }
            else if (!movingObject)
            {
                KeyRepeat(ref PrecUpKey, () => ChangePrecision(-1));
                KeyRepeat(ref PrecDownKey, () => ChangePrecision(1));
                KeyRepeat(ref SeekBackKey, () => Seek(-1f));
                KeyRepeat(ref SeekForeKey, () => Seek(1f));

                KeyRepeat(ref RotLeftKey, () => RotateCurrent(-1));
                KeyRepeat(ref RotRightKey, () => RotateCurrent(1));

                if (Input.GetKeyDown(Note))
                {
                    spawnableToPlace = SpawnableType.Note;
                    indicator.sprite = NoteImage;
                }
                else if (Input.GetKeyDown(Bomb))
                {
                    spawnableToPlace = SpawnableType.Bomb;
                    indicator.sprite = BombImage;
                }
                else if (Input.GetKeyDown(Wall))
                {
                    spawnableToPlace = SpawnableType.Wall;
                    indicator.sprite = WallImage;
                }
                else if (Input.GetKeyDown(Stack))
                {
                    spawnableToPlace = SpawnableType.Stack;
                    indicator.sprite = StackImage;
                }
                else if (Input.GetKeyDown(Rail))
                {
                    spawnableToPlace = SpawnableType.Rail;
                    indicator.sprite = RailImage;
                }

                if (Input.GetKeyDown(Delete))
                {
                    Spawner.RemoveSpawnable(hitSpawnable.gameObject);
                }

                if (Input.GetMouseButtonDown(2))
                {
                    if (spawnableColor == 1)
                    {
                        indicator.color = Color.red;
                        spawnableColor = 0;
                    }
                    else
                    {
                        indicator.color = Color.blue;
                        spawnableColor = 1;
                    }
                }

                Seek(Input.mouseScrollDelta.y);
            }

            if (Input.anyKeyDown || Input.GetMouseButtonUp(0))
            {
                movingObject = false;
            }

            if ((Input.GetKeyDown(Move) || Input.GetMouseButtonDown(0)) && !movingObjectPrev)
            {
                movingObject = true;
            }

            if (Input.GetMouseButtonDown(0) && !movingObjectPrev)
            {
                Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
                RaycastHit hit;
                if (Physics.Raycast(ray, out hit, Mathf.Infinity, ~(1 << 8)))
                {
                    Selectable hitSelectable = hit.transform.GetComponent<Selectable>();

                    if (hitSelectable != null)
                    {
                        hitSpawnable = hitSelectable.GetRoot();
                        hitSpawnable.Selected(!hitSpawnable.GetSelected());
                    } else
                    {
                        hitSpawnable = null;
                    }
                }
                else if (hitSpawnable == null)
                {
                    hitPlane = new Plane(Vector3.forward, Vector3.zero);
                    Vector3 spawnPosition = GetPlaneIntersect();

                    hitSpawnable = mapManager.AddSpawnable(spawnableToPlace, (int)Mathf.Floor(spawnPosition.x + 2), (int)Mathf.Floor(spawnPosition.y), spawnableColor);

                    if (hitSpawnable != null)
                    {
                        hitSpawnable.Selected(true);
                    }
                }
                else
                {
                    hitSpawnable.Selected(false);
                    hitSpawnable = null;
                }

                if (lastHitSelectable != null && lastHitSelectable != hitSpawnable)
                {
                    lastHitSelectable.Selected(false);
                }

                lastHitSelectable = hitSpawnable;
            }

            if (movingObject && !movingObjectPrev && hitSpawnable != null)
            {
                hitPlane = new Plane(Vector3.forward, hitSpawnable.transform.position);
                offset = GetPlaneIntersect() - new Vector3(hitSpawnable.x, hitSpawnable.y, 0);
            }

            if (movingObject && hitSpawnable != null)
            {
                if (hitSpawnable.GetSelected())
                {
                    Vector3 curPosition = GetPlaneIntersect() - offset;

                    MoveCurrent((int)Mathf.Floor(curPosition.x + 0.5f), (int)Mathf.Floor(curPosition.y + 0.5f));

                    hitSpawnable.Moved();
                }
            }

            movingObjectPrev = movingObject;
        }
    }

    // Functions for key presses
    private void ToggleMenu()
    {
        menuManager.gameObject.SetActive(!menuManager.gameObject.activeSelf);
    }

    public void TogglePaused()
    {
        GlobalData.paused = !GlobalData.paused;
        mapManager.ResyncAudio();
    }

    public void ChangePrecision(int amount)
    {

        GlobalData.beatPrecision *= Mathf.Pow(2, amount);
    }

    public void Seek(float amount)
    {
        if (amount == 0)
        {
            return;
        }
        GlobalData.currentBeat += amount * GlobalData.beatPrecision;
        mapManager.ResyncAudio();
       
        if(ghostRoutine != null)
        {
            StopCoroutine(ghostRoutine);
        }

        ghostRoutine = mapManager.GhostAudio();
        StartCoroutine(ghostRoutine);
    }

    public void RotateCurrent(int direction)
    {
        if (hitSpawnable.GetType().IsSubclassOf(typeof(Colored)) && hitSpawnable.GetSelected())
        {
            Colored hitColored = hitSpawnable as Colored;
            hitColored.cutDirection = (((hitColored.cutDirection + (int)direction) % 8) + 8) % 8;
            hitColored.UpdateRotation();
        }
    }

    private void MoveCurrent(int x, int y)
    {
        hitSpawnable.x = x;
        hitSpawnable.y = y;
    }

    private void ChangeBeat(float beat)
    {
        hitSpawnable.beat += beat * GlobalData.beatPrecision;
        hitSpawnable.Moved();
    }

    // Execute a function repetitively when a key is held
    public void KeyRepeat(ref Key key, Action func)
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
                func();
                key.prev = true;
            }
        }
        else
        {
            key.timer = 0;
            key.prev = false;
        }
    }

    private Vector3 GetPlaneIntersect()
    {
        float depth = 0;
        Ray ray = Camera.main.ScreenPointToRay(Input.mousePosition);
        hitPlane.Raycast(ray, out depth);

        Vector3 curScreenPoint = new Vector3(Input.mousePosition.x, Input.mousePosition.y, depth);
        Vector3 curPosition = Camera.main.ScreenToWorldPoint(curScreenPoint);

        return curPosition;
    }

    public struct POINT
    {
        public int X;
        public int Y;

        public POINT(int x, int y)
        {
            this.X = x;
            this.Y = y;
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
}

public enum SpawnableType
{
    Note,
    Bomb,
    Wall,
    Stack,
    Rail
}