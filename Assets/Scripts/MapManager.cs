using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class MapManager : MonoBehaviour
{
    [HideInInspector]
    public bool isMapVaild = true;

    [Header("Defaults")]
    [Range(0, 100)]
    public float defaultVolume;

    [Header("UI")]
    public Slider volume;
    public Text volumeDisplay;

    [Header("References")]
    public Spawner spawner;
    public Dropdown modeSelect;
    public Dropdown diffSelect;
    public GameObject warning;
    public Slider progressBar;
    public Text timeRemaining;

    private MapData mapData;
    private DifData difData;
    private int currentType;
    private int currentDiff;

    private static List<SpawnableSerial> spawnables;

    private static bool hasSpawnablesChanged = false;

    private bool hasAudioLoaded;

    private static int spawnIndex = -1;
    private int oldestSpawnIndex = 0;
    private float prevBeat;

    private AudioSource audioSource;

    private float lastSetValue;

    private string maxTimeString;

    [Header("Debugging")]
    public string selectedFolder;

    private List<string> modes = new List<string>();
    private List<List<string>> diffsList = new List<List<string>>();

    void Start()
    {
#if UNITY_EDITOR
        GlobalData.selectedFolder = selectedFolder;
#endif

        mapData = ImportJson<MapData>(GlobalData.selectedFolder + "/Info.dat");

        if(mapData._version != "3.0.0")
        {
            timeRemaining.text = "Unsupported map version";
            isMapVaild = false;
            return;
        }

        GlobalData.bpm = mapData._beatsPerMinute;
        GlobalData.audioOffset = mapData._songTimeOffset / 1000f;
        foreach (DifficultyBeatmapSet mode in mapData._difficultyBeatmapSets)
        {
            modes.Add(mode._beatmapCharacteristicName);
            List<string> diffs = new List<string>();
            foreach (DifficultyBeatmap diff in mode._difficultyBeatmaps)
            {
                diffs.Add(diff._difficulty);
            }
            diffsList.Add(diffs);
        }

        modeSelect.AddOptions(modes);
        ReloadDiffOptions();

        LoadDifficulty(0, 0);

        audioSource = GetComponent<AudioSource>();

        StartCoroutine(LoadSong());

        volume.value = defaultVolume;
        UpdateVolume();
    }

    void Update()
    {
        if (isMapVaild) return;

        if (!hasAudioLoaded)
        {
            return;
        }

        int end = spawnables.Count - 1;

        UpdateTimeRemaining();

        float speed = GlobalData.currentBeat - prevBeat;

        if (speed != 0)
        {
            bool direction = speed > 0;

            List<Spawnable> spawnedThisFrame = new List<Spawnable>();

            if (direction)
            { // forwards
                if (spawnIndex + 1 <= end)
                {
                    while (CheckForSpawn(spawnables[spawnIndex + 1].b, true))
                    {
                        spawnIndex++;
                        spawnedThisFrame.Add(spawner.SpawnSpawnable(spawnables[spawnIndex], spawnIndex));

                        if (spawnIndex + 1 > end)
                        {
                            break;
                        }
                    }
                }

                if (oldestSpawnIndex <= end)
                {
                    while (CheckForDespawn(spawnables[oldestSpawnIndex].b, true))
                    {
                        oldestSpawnIndex++;

                        if (oldestSpawnIndex > end)
                        {
                            break;
                        }
                    }
                }
            }
            else // backwards
            {
                if (oldestSpawnIndex > 0)
                {
                    while (CheckForSpawn(spawnables[oldestSpawnIndex - 1].b, false))
                    {
                        oldestSpawnIndex--;
                        spawnedThisFrame.Add(spawner.SpawnSpawnable(spawnables[oldestSpawnIndex], oldestSpawnIndex));

                        if (oldestSpawnIndex <= 0)
                        {
                            break;
                        }
                    }
                }

                if (spawnIndex >= 0)
                {
                    while (CheckForDespawn(spawnables[spawnIndex].b, false))
                    {
                        spawnIndex--;

                        if (spawnIndex < 0)
                        {
                            break;
                        }
                    }
                }
            }

            List<BurstSlider> sliders = new List<BurstSlider>();
            List<Note> notes = new List<Note>();
            foreach(Spawnable spawnable in spawnedThisFrame)
            {
                if(spawnable == null)
                {
                    continue;
                }

                switch (spawnable.GetType().Name)
                {
                    case "BurstSlider":
                        BurstSlider slider = spawnable as BurstSlider;
                        sliders.Add(slider);

                        foreach(Note noteCheck in notes)
                        {
                            if(noteCheck.x == slider.x && noteCheck.y == slider.y && noteCheck.beat == slider.beat)
                            {
                                slider.SetHead(noteCheck);
                                notes.Remove(noteCheck);
                                sliders.Remove(slider);
                                break;
                            }
                        }
                        break;
                    case "Note":
                        Note note = spawnable as Note;
                        notes.Add(note);

                        foreach (BurstSlider sliderCheck in sliders)
                        {
                            if (note.x == sliderCheck.x && note.y == sliderCheck.y && note.beat == sliderCheck.beat)
                            {
                                sliderCheck.SetHead(note);
                                notes.Remove(note);
                                sliders.Remove(sliderCheck);
                                break;
                            }
                        }
                        break;
                }
            }

            foreach(BurstSlider slider in sliders)
            {
                slider.SetHead(null);
            }
        }


        prevBeat = GlobalData.currentBeat;
    }

    private IEnumerator LoadSong()
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + GlobalData.selectedFolder + "/" + mapData._songFilename, AudioType.OGGVORBIS))
        {
            yield return www.SendWebRequest();

            if (www.error != null)
            {
                Debug.Log(www.error);
            }
            else
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
            }
        }

        audioSource.Play(); // Required due to a bug with Unity where audioSource.time is always zero until played
        audioSource.Pause();

        int seconds = (int)audioSource.clip.length;
        int minutes = seconds / 60;
        maxTimeString = " / " + string.Format("{0:D1}:{1:D2}", minutes, seconds % 60);
        hasAudioLoaded = true;
    }

    T ImportJson<T>(string path)
    {
        StreamReader reader = new StreamReader(path);
        T returnVal = JsonUtility.FromJson<T>(reader.ReadToEnd());
        reader.Close();
        return returnVal;
    }

    void ExportJson<T>(string path, T data)
    {
        StreamWriter writer = new StreamWriter(path);

        string jsonData = JsonUtility.ToJson(data);

        jsonData = jsonData.Replace(",", ",\n");
        jsonData = jsonData.Replace("[", "[\n");
        jsonData = jsonData.Replace("{", "{\n");
        jsonData = jsonData.Replace("]", "\n]");
        jsonData = jsonData.Replace("}", "\n}");
        jsonData = jsonData.Replace(":", ": ");

        int indentAmt = 0;
        string prettyJson = "";

        using (StringReader reader = new StringReader(jsonData))
        {
            string line;
            while ((line = reader.ReadLine()) != null)
            {
                if (line.Contains("}") || line.Contains("]"))
                {
                    indentAmt--;
                }

                prettyJson += new String(' ', indentAmt * 4) + line + "\n";

                if (line.Contains("{") || line.Contains("["))
                {
                    indentAmt++;
                }
            }
        }

        writer.WriteLine(prettyJson);
        writer.Close();
    }

    public void UpdateVolume()
    {
        volumeDisplay.text = ((int)volume.value).ToString();
        audioSource.volume = volume.value / 100f;
    }

    public void LoadDifficulty(int type, int difficulty)
    {
        currentType = type;
        currentDiff = difficulty;

        DifficultyBeatmap difficultyBeatMap = mapData._difficultyBeatmapSets[type]._difficultyBeatmaps[difficulty];
        difData = ImportJson<DifData>(GlobalData.selectedFolder + "/" + difficultyBeatMap._beatmapFilename);

        spawnables = CreateSpawnablesList(difData);
        GlobalData.jumpSpeed = difficultyBeatMap._noteJumpMovementSpeed;
        GlobalData.spawnOffset = difficultyBeatMap._noteJumpStartBeatOffset;

        GlobalData.HJD = Mathf.Max(0.25f, (4 / Mathf.Pow(2, Mathf.Floor((GlobalData.jumpSpeed / GlobalData.bpm) / .075f))) + GlobalData.spawnOffset);
    }

    public void ReloadDifficulty()
    {
        spawner.ClearNotes();
        spawnIndex = -1;
        oldestSpawnIndex = 0;
        prevBeat = 0;
        LoadDifficulty(modeSelect.value, diffSelect.value);
    }

    public void ReloadDiffOptions()
    {
        diffSelect.ClearOptions();
        diffSelect.AddOptions(diffsList[modeSelect.value]);
    }

    public void ResyncAudio()
    {
        if (lastSetValue != progressBar.value)
        {
            GlobalData.currentBeat = (GlobalData.bpm * progressBar.value * audioSource.clip.length) / 60;
        }

        audioSource.time = 60 * ((GlobalData.currentBeat) / GlobalData.bpm) + GlobalData.audioOffset;

        progressBar.value = audioSource.time / audioSource.clip.length;
        lastSetValue = progressBar.value;

        if (GlobalData.paused)
        {
            audioSource.Pause();
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }
    }

    public bool CheckForSpawn(float beat, bool forwards)
    {
        if (forwards)
        {
            if (GlobalData.currentBeat >= beat - GlobalData.HJD)
            {
                return true;
            }
        }
        else if (GlobalData.currentBeat <= beat + 0.5f)
        {
            return true;
        }

        return false;
    }

    public bool CheckForDespawn(float beat, bool forwards)
    {
        if (forwards)
        {
            if (GlobalData.currentBeat > beat + 0.5f)
            {
                return true;
            }
        }
        else if (GlobalData.currentBeat < beat - GlobalData.HJD)
        {
            return true;
        }

        return false;
    }

    private List<SpawnableSerial> CreateSpawnablesList(DifData difData)
    {
        List<NoteSerial> notes = difData.colorNotes.ToList();
        List<BombSerial> bombs = difData.bombNotes.ToList();
        List<ObstacleSerial> obstacles = difData.obstacles.ToList();
        List<SliderSerial> rails = difData.sliders.ToList();
        List<BurstSliderSerial> burstSliders = difData.burstSliders.ToList();

        List<SpawnableSerial> spawnables = new List<SpawnableSerial>(notes.Count + bombs.Count + obstacles.Count + rails.Count + burstSliders.Count);
        spawnables.AddRange(notes);
        spawnables.AddRange(bombs);
        spawnables.AddRange(obstacles);
        spawnables.AddRange(rails);
        spawnables.AddRange(burstSliders);

        SpawnableSerial[] spawnableArray = spawnables.ToArray();
        Array.Sort(spawnableArray);

        return spawnableArray.ToList();
    }

    public static void UpdateSpawnable(Spawnable spawnable)
    {
        switch (spawnable.GetType().Name)
        {
            case "Note":
                spawnables[spawnable.index] = (NoteSerial)(spawnable as Note);
                break;
            case "Bomb":
                spawnables[spawnable.index] = (BombSerial)(spawnable as Bomb);
                break;
            case "Obstacle":
                spawnables[spawnable.index] = (ObstacleSerial)(spawnable as Obstacle);
                break;
            case "Rail":
                spawnables[spawnable.index] = (SliderSerial)(spawnable as Rail);
                break;
            case "BurstSlider":
                spawnables[spawnable.index] = (BurstSliderSerial)(spawnable as BurstSlider);
                break;
            default:
                Debug.LogWarning("Unknown type '" + spawnable.GetType().Name + "'");
                return;
        }

        hasSpawnablesChanged = true;
    }

    public void TryReloadDifficulty()
    {
        if (hasSpawnablesChanged)
        {
            warning.SetActive(true);
        }
        else
        {
            ReloadDifficulty();
        }
    }

    public void ReloadConfirmed()
    {
        hasSpawnablesChanged = false;
        ReloadDifficulty();
    }

    public void SaveDiff()
    {
        List<NoteSerial> notes = new List<NoteSerial>();
        List<BombSerial> bombs = new List<BombSerial>();
        List<ObstacleSerial> obstacles = new List<ObstacleSerial>();
        List<SliderSerial> sliders = new List<SliderSerial>();
        List<BurstSliderSerial> burstSliders = new List<BurstSliderSerial>();

        foreach (SpawnableSerial spawnable in spawnables)
        {
            switch (spawnable.GetType().Name)
            {
                case "NoteSerial":
                    notes.Add(spawnable as NoteSerial);
                    break;
                case "BombSerial":
                    bombs.Add(spawnable as BombSerial);
                    break;
                case "ObstacleSerial":
                    obstacles.Add(spawnable as ObstacleSerial);
                    break;
                case "SliderSerial":
                    sliders.Add(spawnable as SliderSerial);
                    break;
                case "BurstSliderSerial":
                    burstSliders.Add(spawnable as BurstSliderSerial);
                    break;
                default:
                    Debug.LogWarning("Unknown type '" + spawnable.GetType().Name + "'");
                    return;
            }
        }

        difData.colorNotes = notes.ToArray();
        difData.bombNotes = bombs.ToArray();
        difData.obstacles = obstacles.ToArray();
        difData.sliders = sliders.ToArray();
        difData.burstSliders = burstSliders.ToArray();

        DifficultyBeatmap difficultyBeatMap = mapData._difficultyBeatmapSets[currentType]._difficultyBeatmaps[currentDiff];
        ExportJson<DifData>(GlobalData.selectedFolder + "/" + difficultyBeatMap._beatmapFilename, difData);
    }
    
    private void UpdateTimeRemaining()
    {
        int seconds = (int)audioSource.time;
        int minutes = seconds / 60;

        timeRemaining.text = string.Format("{0:D1}:{1:D2}", minutes, seconds % 60) + maxTimeString;
    }

    public Spawnable AddSpawnable(SpawnableType spawnableType, int x, int y, int color)
    {
        int index = spawnables.BinarySearch(new BombSerial(GlobalData.currentBeat, 0, 0));
        if (index < 0) index = ~index;

        switch (spawnableType)
        {
            case (SpawnableType.Note):
                spawnables.Insert(index, new NoteSerial(GlobalData.currentBeat, x, y, color, 0, 0));
                break;
            case (SpawnableType.Bomb):
                spawnables.Insert(index, new BombSerial(GlobalData.currentBeat, x, y));
                break;
            case (SpawnableType.Wall):
                spawnables.Insert(index, new ObstacleSerial(GlobalData.currentBeat, x, y, 1, 1, 0.5f));
                break;
            case (SpawnableType.Stack):
                spawnables.Insert(index, new BurstSliderSerial(GlobalData.currentBeat, x, y, color, 0, GlobalData.currentBeat + 0.5f, 0, 0, 2, 0.5f));
                break;
            case (SpawnableType.Rail):
                spawnables.Insert(index, new SliderSerial(GlobalData.currentBeat, x, y, color, 0, GlobalData.currentBeat + 0.5f, 0, 0, 0, 1, 1, 0));
                break;
        }

        spawnIndex++;

        spawner.AdjustSpawnableIndicies(index);

        return spawner.SpawnSpawnable(spawnables[index], index);
    }

    public void DeleteSpawnable(Spawnable spawnableToRemove)
    {
        spawnables.RemoveAt(spawnableToRemove.index);
        Spawner.RemoveSpawnable(spawnableToRemove.gameObject);
    }

    public IEnumerator<object> GhostAudio()
    {
        audioSource.Play();
        yield return new WaitForSeconds(60 * GlobalData.beatPrecision / GlobalData.bpm);
        audioSource.Pause();
        ResyncAudio();
    }
}