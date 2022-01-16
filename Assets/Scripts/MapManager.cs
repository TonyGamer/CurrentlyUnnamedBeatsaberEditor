using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.Networking;

[RequireComponent(typeof(AudioSource))]
public class MapManager : MonoBehaviour
{
    public string selectedFolder;
    public NoteSpawner noteSpawner;

    public MapData mapData;
    public DifData difData;
    public NoteSerial[] notes;

    private int currentNoteIndex = -1;
    private int oldestNoteIndex = 0;
    private float prevBeat;

    private AudioSource audioSource;

    void Start()
    {
        mapData = ImportJson<MapData>(selectedFolder + "Info.dat");

        DifficultyBeatmap difficultyBeatMap = mapData._difficultyBeatmapSets[0]._difficultyBeatmaps[1];
        difData = ImportJson<DifData>(selectedFolder + difficultyBeatMap._beatmapFilename);

        notes = difData._notes;

        GlobalData.bpm = mapData._beatsPerMinute;
        GlobalData.jumpSpeed = difficultyBeatMap._noteJumpMovementSpeed;
        GlobalData.offset = difficultyBeatMap._noteJumpStartBeatOffset;

        audioSource = GetComponent<AudioSource>();

        using (UnityWebRequest www = UnityWebRequestMultimedia.GetAudioClip("file:///" + selectedFolder + mapData._songFilename, AudioType.OGGVORBIS))
        {
            www.SendWebRequest();

            if(www.error != null)
            {
                Debug.Log(www.error);
            } else
            {
                audioSource.clip = DownloadHandlerAudioClip.GetContent(www);
            }
        }

        audioSource.volume = 0.33f;
    }

    void Update()
    {
        int end = notes.Length - 1;

        if (GlobalData.paused)
        {
            audioSource.Pause();
            audioSource.time = 60 * GlobalData.currentBeat / GlobalData.bpm;
        } else if (!audioSource.isPlaying)
        {
            audioSource.Play();
            audioSource.time = 60 * GlobalData.currentBeat / GlobalData.bpm;
        }

        if (GlobalData.currentBeat - prevBeat > 0) {
            while(GlobalData.currentBeat > GlobalData.offset + notes[currentNoteIndex+1]._time - 1.0)
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
        } else if(GlobalData.currentBeat - prevBeat < 0)
        {
            if (oldestNoteIndex > 0)
            {
                while (GlobalData.currentBeat < notes[oldestNoteIndex - 1]._time)
                {
                    Debug.Log(oldestNoteIndex);

                    oldestNoteIndex--;

                    Debug.Log("Creating Note: " + oldestNoteIndex);
                    noteSpawner.SpawnNote(notes[oldestNoteIndex]);
                }
            }

            while (GlobalData.currentBeat < GlobalData.offset + notes[currentNoteIndex]._time - 1.0)
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

    T ImportJson<T>(string path)
    {
        StreamReader reader = new StreamReader(path);
        return JsonUtility.FromJson<T>(reader.ReadToEnd());
    }
}