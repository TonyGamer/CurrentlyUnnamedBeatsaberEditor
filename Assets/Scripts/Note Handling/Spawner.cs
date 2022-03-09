using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Spawner : MonoBehaviour
{
    public GameObject noteObject;
    public GameObject dotObject;
    public GameObject bombObject;

    private List<GameObject> notes = new List<GameObject>();
    private List<GameObject> bombs = new List<GameObject>();

    public void SpawnNote(NoteSerial noteToSpawn)
    {
        SpawnNote(noteToSpawn.x, noteToSpawn.y, noteToSpawn.c, noteToSpawn.d, noteToSpawn.b, noteToSpawn.a);
    }

    public void SpawnNote(int x, int y, int color, int cutDirection, float beat, int angleOffset)
    {
        GameObject noteMesh = noteObject;
        Vector3 position = calculatePosition(x, y, beat);
        Quaternion rotation = calculateRotation(cutDirection, angleOffset);

        if (cutDirection == 8)
        {
            noteMesh = dotObject;
        }

        GameObject spawnedNote = Instantiate(noteMesh, position, rotation);
        Note noteComp = spawnedNote.GetComponent<Note>();

        spawnedNote.GetComponent<Renderer>().material.color = getColor(color);

        noteComp.beat = beat;
        noteComp.x = x;
        noteComp.y = y;
        noteComp.cutDirection = cutDirection;

        noteComp.color = color;

        notes.Add(spawnedNote);
    }

    public void SpawnBomb(BombSerial bombToSpawn)
    {
        SpawnBomb(bombToSpawn.b, bombToSpawn.x, bombToSpawn.y);
    }

    public void SpawnBomb(float beat, int x, int y)
    {
        Vector3 position = calculatePosition(x, y, beat);
        Quaternion rotation = Quaternion.identity;

        GameObject spawnedBomb = Instantiate(bombObject, position, rotation);
        Bomb bombComp = spawnedBomb.GetComponent<Bomb>();

        bombComp.beat = beat;
        bombComp.x = x;
        bombComp.y = y;

        bombs.Add(spawnedBomb);
    }

    public void ClearNotes()
    {
        foreach (GameObject note in notes)
        {
            Object.Destroy(note);
        }
    }

    public static Quaternion calculateRotation(int cutDirection, int angleOffset)
    {
        Quaternion rotation = Quaternion.identity;

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

        return rotation * Quaternion.Euler(Vector3.forward * angleOffset);
    }

    public static float calculateZOffset(float beat)
    {
        return 0.5f * GlobalData.jumpSpeed * (beat - GlobalData.currentBeat);
    }

    public static Vector3 calculatePosition(int x, int y, float beat)
    {
        return new Vector3(x - 1.5f, y + 0.5f, calculateZOffset(beat));
    }

    public static Color getColor(int color)
    {
        switch (color)
        {
            case 0:
                return Color.red;
            case 1:
                return Color.blue;
            default:
                return Color.magenta; // Fallback to pink if color is unexpected
        }
    }
}
