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

    [HideInInspector]
    public MapData mapData;
    [HideInInspector]
    public DifData difData;
    [HideInInspector]
    public SpawnableSerial[] spawnables;

    private int spawnIndex = -1;
    private int oldestSpawnIndex = 0;
    private float prevBeat;

    private AudioSource audioSource;

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

        StartCoroutine(loadSong());

        volume.value = defaultVolume;
        UpdateVolume();
    }

    void Update()
    {
        int end = spawnables.Length - 1;

        if (GlobalData.paused)
        {
            audioSource.Pause();
        }
        else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        float speed = GlobalData.currentBeat - prevBeat;

        if(speed == 0) { return; }

        bool direction = speed > 0;

        if (direction) { // forwards
            if(spawnIndex + 1 <= end)
            {
                while (CheckForSpawn(spawnables[spawnIndex + 1].b, true))
                {
                    spawner.SpawnSpawnable(spawnables[++spawnIndex]);

                    if(spawnIndex + 1 > end)
                    {
                        break;
                    }
                }
            }

            if(oldestSpawnIndex <= end)
            {
                while(CheckForDespawn(spawnables[oldestSpawnIndex].b, true)){
                    oldestSpawnIndex++;

                    if(oldestSpawnIndex > end)
                    {
                        break;
                    }
                }
            }
        } else // backwards
        {
            if(oldestSpawnIndex > 0)
            {
                while (CheckForSpawn(spawnables[oldestSpawnIndex - 1].b, false))
                {
                    oldestSpawnIndex--;

                    spawner.SpawnSpawnable(spawnables[oldestSpawnIndex]);

                    if (oldestSpawnIndex <= 0)
                    {
                        break;
                    }
                }
            }

            if(spawnIndex >= 0)
            {
                while(CheckForDespawn(spawnables[spawnIndex].b, false))
                {
                    spawnIndex--;

                    if(spawnIndex < 0)
                    {
                        break;
                    }
                }
            }
        }

        prevBeat = GlobalData.currentBeat;
    }

    private IEnumerator loadSong()
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
    }

    T ImportJson<T>(string path)
    {
        StreamReader reader = new StreamReader(path);
        return JsonUtility.FromJson<T>(reader.ReadToEnd());
    }

    public void UpdateVolume()
    {
        volumeDisplay.text = ((int)volume.value).ToString();
        audioSource.volume = volume.value / 100f;
    }

    public void ResyncAudio()
    {
        audioSource.time = (60 * (GlobalData.currentBeat) / GlobalData.bpm) + GlobalData.audioOffset;
    }

    public void LoadDifficulty(int type, int difficulty)
    {
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

    private SpawnableSerial[] CreateSpawnablesList(DifData difData)
    {
        List<NoteSerial> notes = difData.colorNotes.ToList<NoteSerial>();
        List<BombSerial> bombs = difData.bombNotes.ToList<BombSerial>();
        List<ObstacleSerial> obstacles = difData.obstacles.ToList<ObstacleSerial>();
        List<SliderSerial> rails = difData.sliders.ToList<SliderSerial>();
        List<BurstSliderSerial> burstSliders = difData.burstSliders.ToList<BurstSliderSerial>();

        List<SpawnableSerial> spawnables = new List<SpawnableSerial>(notes.Count + bombs.Count + obstacles.Count + rails.Count + burstSliders.Count);
        spawnables.AddRange(notes);
        spawnables.AddRange(bombs);
        spawnables.AddRange(obstacles);
        spawnables.AddRange(rails);
        spawnables.AddRange(burstSliders);

        SpawnableSerial[] spawnableArray = spawnables.ToArray();
        Array.Sort(spawnableArray);

        return spawnableArray;
    }
}