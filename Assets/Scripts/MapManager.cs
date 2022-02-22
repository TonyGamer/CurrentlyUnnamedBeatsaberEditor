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

    void Start()
    {
#if UNITY_EDITOR
        GlobalData.selectedFolder = selectedFolder;
#endif

        mapData = ImportJson<MapData>(GlobalData.selectedFolder + "/Info.dat");

        DifficultyBeatmap difficultyBeatMap = mapData._difficultyBeatmapSets[0]._difficultyBeatmaps[0];
        difData = ImportJson<DifData>(GlobalData.selectedFolder + "/" + difficultyBeatMap._beatmapFilename);

        notes = difData._notes;

        GlobalData.bpm = mapData._beatsPerMinute;
        GlobalData.audioOffset = mapData._songTimeOffset / 1000f;
        GlobalData.jumpSpeed = difficultyBeatMap._noteJumpMovementSpeed;
        GlobalData.spawnOffset = difficultyBeatMap._noteJumpStartBeatOffset;

        GlobalData.HJD = Mathf.Max(0.25f, (4 / Mathf.Pow(2, Mathf.Floor((GlobalData.jumpSpeed / GlobalData.bpm) / .075f))) + GlobalData.spawnOffset);

        Debug.Log(GlobalData.HJD);

        audioSource = GetComponent<AudioSource>();

        StartCoroutine(loadSong());

        volume.value = defaultVolume;
        updateVolume();
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

    public void updateVolume()
    {
        volumeDisplay.text = ((int) volume.value).ToString();
        audioSource.volume = volume.value / 100f;
    }

    public void resyncAudio()
    {
        audioSource.time = (60 * (GlobalData.currentBeat) / GlobalData.bpm) + GlobalData.audioOffset;
    }
}