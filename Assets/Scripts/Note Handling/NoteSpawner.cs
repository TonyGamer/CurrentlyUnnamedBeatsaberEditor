using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class NoteSpawner : MonoBehaviour
{
    public GameObject note;
    public GameObject dot;
    private List<GameObject> notes = new List<GameObject>();

    public void SpawnNote(NoteSerial noteToSpawn)
    {
        SpawnNote(noteToSpawn._lineIndex, noteToSpawn._lineLayer, noteToSpawn._type, noteToSpawn._cutDirection, noteToSpawn._time);
    }

    public void SpawnNote(int lineIndex, int lineLayer, int type, int cutDirection, float time)
    {
        GameObject spawnedNote;
        GameObject noteMesh;
        Vector3 position = new Vector3(lineIndex - 1.5f, lineLayer + 0.5f, 30);
        Quaternion rotation = Quaternion.identity;

        if (cutDirection != 8 && type != 3)
        {
            switch (cutDirection)
            {
                case 1:
                    rotation *= Quaternion.Euler(Vector3.forward * 180);
                    break;
                case 2:
                    rotation *= Quaternion.Euler(Vector3.forward * 90);
                    break;
                case 3:
                    rotation *= Quaternion.Euler(Vector3.forward * -90);
                    break;
                case 4:
                    rotation *= Quaternion.Euler(Vector3.forward * 45);
                    break;
                case 5:
                    rotation *= Quaternion.Euler(Vector3.forward * -45);
                    break;
                case 6:
                    rotation *= Quaternion.Euler(Vector3.forward * 135);
                    break;
                case 7:
                    rotation *= Quaternion.Euler(Vector3.forward * -135);
                    break;
                default:
                    rotation = Quaternion.identity;
                    break;
            }

            noteMesh = note;
        }
        else
        {
            noteMesh = dot;
        }

        position = new Vector3(lineIndex - 1.5f, lineLayer + 0.5f, 0.5f * GlobalData.jumpSpeed * (time - GlobalData.currentBeat));
        spawnedNote = Instantiate(noteMesh, position, rotation);
        Note noteComp = spawnedNote.GetComponent<Note>();

        switch (type)
        {
            case 0:
                spawnedNote.GetComponent<Renderer>().material.color = Color.red;
                break;
            case 1:
                spawnedNote.GetComponent<Renderer>().material.color = Color.blue;
                break;
            default:
                spawnedNote.GetComponent<Renderer>().material.color = Color.black;
                break;
        }

        noteComp.time = time;
        noteComp.lineIndex = lineIndex;
        noteComp.lineLayer = lineLayer;
        noteComp.cutDirection = cutDirection;

        noteComp.type = type;

        notes.Add(spawnedNote);
    }

    public void ClearNotes()
    {
        foreach(GameObject note in notes)
        {
            Object.Destroy(note);
        }
    }
}
