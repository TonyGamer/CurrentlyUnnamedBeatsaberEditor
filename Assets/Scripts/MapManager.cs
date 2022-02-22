using System.Collections;
using System.Collections.Generic;
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
    public NoteSpawner noteSpawner;
    public Dropdown modeSelect;
    public Dropdown diffSelect;

    [HideInInspector]
    public MapData mapData;
    [HideInInspector]
    public DifData difData;
    [HideInInspector]
    public NoteSerial[] notes;

    private int currentNoteIndex = -1;
    private int oldestNoteIndex = 0;
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
        foreach(DifficultyBeatmapSet mode in mapData._difficultyBeatmapSets)
        {
            modes.Add(mode._beatmapCharacteristicName);
            List<string> diffs = new List<string>();
            foreach(DifficultyBeatmap diff in mode._difficultyBeatmaps)
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
        int end = notes.Length - 1;

        if (GlobalData.paused)
        {
            audioSource.Pause();
        } else if (!audioSource.isPlaying)
        {
            audioSource.Play();
        }

        if (GlobalData.currentBeat - prevBeat > 0) { // Going forwards
            while(GlobalData.currentBeat > notes[currentNoteIndex+1]._time - GlobalData.HJD)
            {
                currentNoteIndex++;

                if (currentNoteIndex > end)
                {
                    break;
                }

                noteSpawner.SpawnNote(notes[currentNoteIndex]);
            }

            while(GlobalData.currentBeat > notes[oldestNoteIndex]._time)
            {
                oldestNoteIndex++;

                if (oldestNoteIndex > end)
                {
                    break;
                }
            }
        } else if(GlobalData.currentBeat - prevBeat < 0) // Going backwards
        {
            if (oldestNoteIndex > 0)
            {
                while (GlobalData.currentBeat < notes[oldestNoteIndex - 1]._time)
                {
                    oldestNoteIndex--;

                    noteSpawner.SpawnNote(notes[oldestNoteIndex]);
                }
            }

            while (GlobalData.currentBeat < notes[currentNoteIndex]._time - GlobalData.HJD)
            {
                if (currentNoteIndex <= 0)
                {
                    break;
                }

                currentNoteIndex--;
            }
        }

        prevBeat = GlobalData.currentBeat;
    }

    private IEnumerator loadSong()
    {
        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + GlobalData.selectedFolder + "/" + mapData._songFilename, AudioType.OGGVORBIS))
        {
            www.SendWebRequest();

            yield return www;

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
        volumeDisplay.text = ((int) volume.value).ToString();
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

        notes = difData._notes;
        GlobalData.jumpSpeed = difficultyBeatMap._noteJumpMovementSpeed;
        GlobalData.spawnOffset = difficultyBeatMap._noteJumpStartBeatOffset;

        GlobalData.HJD = Mathf.Max(0.25f, (4 / Mathf.Pow(2, Mathf.Floor((GlobalData.jumpSpeed / GlobalData.bpm) / .075f))) + GlobalData.spawnOffset);
    }

    public void ReloadDifficulty()
    {
        noteSpawner.ClearNotes();
        currentNoteIndex = -1;
        oldestNoteIndex = 0;
        prevBeat = 0;
        LoadDifficulty(modeSelect.value, diffSelect.value);
    }

    public void ReloadDiffOptions()
    {
        diffSelect.ClearOptions();
        diffSelect.AddOptions(diffsList[modeSelect.value]);
    }
}