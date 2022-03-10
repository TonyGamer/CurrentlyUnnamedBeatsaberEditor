using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.IO;

public class Spawner : MonoBehaviour
{
    public GameObject noteObject;
    public GameObject dotObject;
    public GameObject bombObject;
    public GameObject wallObject;
    public GameObject railObject;
    public GameObject bsObject;

    private List<GameObject> spawnables = new List<GameObject>();

    public void SpawnSpawnable(SpawnableSerial spawnable)
    {
        switch(spawnable.GetType().Name)
        {
            case "NoteSerial":
                SpawnNote(spawnable as NoteSerial);
                break;
            case "BombSerial":
                SpawnBomb(spawnable as BombSerial);
                break;
            case "ObstacleSerial":
                SpawnObstacle(spawnable as ObstacleSerial);
                break;
            case "SliderSerial":
                SpawnRail(spawnable as SliderSerial);
                break;
            case "BurstSliderSerial":
                SpawnBurstSlider(spawnable as BurstSliderSerial);
                break;
            default:
                Debug.LogWarning("Unknown type \"" + spawnable.GetType().Name + "\"");
                break;
        }
    }

    public void SpawnNote(NoteSerial noteToSpawn)
    {
        SpawnNote(noteToSpawn.x, noteToSpawn.y, noteToSpawn.c, noteToSpawn.d, noteToSpawn.b, noteToSpawn.a);
    }

    public void SpawnNote(int x, int y, int color, int cutDirection, float beat, int angleOffset)
    {
        GameObject noteMesh = noteObject;
        Vector3 position = CalculatePosition(x, y, beat);
        Quaternion rotation = CalculateRotation(cutDirection, angleOffset);

        if (cutDirection == 8)
        {
            noteMesh = dotObject;
        }

        GameObject spawnedNote = Instantiate(noteMesh, position, rotation);
        Note noteComp = spawnedNote.GetComponent<Note>();

        spawnedNote.GetComponent<Renderer>().material.color = GetColor(color);

        noteComp.beat = beat;
        noteComp.x = x;
        noteComp.y = y;
        noteComp.cutDirection = cutDirection;

        noteComp.color = color;

        spawnables.Add(spawnedNote);
    }

    public void SpawnBomb(BombSerial bombToSpawn)
    {
        SpawnBomb(bombToSpawn.b, bombToSpawn.x, bombToSpawn.y);
    }

    public void SpawnBomb(float beat, int x, int y)
    {
        Vector3 position = CalculatePosition(x, y, beat);
        Quaternion rotation = Quaternion.identity;

        GameObject spawnedBomb = Instantiate(bombObject, position, rotation);
        Bomb bombComp = spawnedBomb.GetComponent<Bomb>();

        bombComp.beat = beat;
        bombComp.x = x;
        bombComp.y = y;

        spawnables.Add(spawnedBomb);
    }

    public void SpawnObstacle(ObstacleSerial obstacleToSpawn)
    {
        SpawnObstacle(obstacleToSpawn.b, obstacleToSpawn.x, obstacleToSpawn.y, obstacleToSpawn.w, obstacleToSpawn.h, obstacleToSpawn.d);
    }

    public void SpawnObstacle(float beat, int x, int y, int width, int height, float duration)
    {
        Vector3 position = CalculatePosition(x + width - 1, y + height - 1, beat);
        float depth = 0.5f * duration * GlobalData.jumpSpeed;

        position.z += depth / 2;

        GameObject spawnedObstacle = Instantiate(wallObject, position, Quaternion.identity);
        spawnedObstacle.transform.localScale = new Vector3(width * 100, height * 100, depth * 100);

        Obstacle obstacleComp = spawnedObstacle.GetComponent<Obstacle>();

        obstacleComp.beat = beat;
        obstacleComp.x = x;
        obstacleComp.y = y;
        obstacleComp.width = width;
        obstacleComp.height = height;
        obstacleComp.duration = duration;

        spawnables.Add(spawnedObstacle);
    }

    public void SpawnRail(SliderSerial sliderToSpawn)
    {
        SpawnRail(sliderToSpawn.b, sliderToSpawn.x, sliderToSpawn.y, sliderToSpawn.c, sliderToSpawn.d, sliderToSpawn.tb, sliderToSpawn.tx, sliderToSpawn.ty, sliderToSpawn.tc, sliderToSpawn.mu, sliderToSpawn.tmu, sliderToSpawn.m); // Lotta variables to pass in
    }

    public void SpawnRail(float beat, int x, int y, int color, int direction, float tailBeat, int tailX, int tailY, int tailDirection, int lengthMultiplier, int tailLengthMultiplier, int anchor)
    {
        Vector3 position = CalculatePosition(x, y, beat);

        GameObject spawnedRail = Instantiate(railObject, position, Quaternion.identity);
        Rail railComp = spawnedRail.GetComponent<Rail>();

        spawnedRail.GetComponent<LineRenderer>().material.color = GetColor(color);

        railComp.beat = beat;
        railComp.x = x;
        railComp.y = y;
        railComp.color = color;
        railComp.direction = direction;
        railComp.tailBeat = tailBeat;
        railComp.tailX = tailX;
        railComp.tailY = tailY;
        railComp.tailDirection = tailDirection;
        railComp.lengthMultiplier = lengthMultiplier;
        railComp.tailLengthMultiplier = tailLengthMultiplier;
        railComp.anchor = anchor;

        spawnables.Add(spawnedRail);
    }

    public void SpawnBurstSlider(BurstSliderSerial bsToSpawn)
    {
        SpawnBurstSlider(bsToSpawn.b, bsToSpawn.x, bsToSpawn.y, bsToSpawn.c, bsToSpawn.d, bsToSpawn.tb, bsToSpawn.tx, bsToSpawn.ty, bsToSpawn.sc, bsToSpawn.s);
    }

    public void SpawnBurstSlider(float beat, int x, int y, int color, int direction, float tailBeat, int tailX, int tailY, int sliceCount, float squash)
    {
        Vector3 position = CalculatePosition(x, y, beat);
        Quaternion rotation = CalculateRotation(direction, 0);

        GameObject spawnedBS = Instantiate(bsObject, position, rotation);
        BurstSlider bsComp = spawnedBS.GetComponent<BurstSlider>();

        spawnedBS.GetComponent<Renderer>().material.color = GetColor(color);

        bsComp.beat = beat;
        bsComp.x = x;
        bsComp.y = y;
        bsComp.color = color;
        bsComp.direction = direction;
        bsComp.tailBeat = tailBeat;
        bsComp.tailX = tailX;
        bsComp.tailY = tailY;
        bsComp.sliceCount = sliceCount;
        bsComp.squash = squash;

        spawnables.Add(spawnedBS);
    }

    public void ClearNotes()
    {
        foreach (GameObject spawnable in spawnables)
        {
            Object.Destroy(spawnable);
        }
    }

    public static Quaternion CalculateRotation(int cutDirection, int angleOffset)
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

    public static float CalculateZOffset(float beat)
    {
        return 0.5f * GlobalData.jumpSpeed * (beat - GlobalData.currentBeat);
    }

    public static Vector3 CalculatePosition(float x, float y, float beat)
    {
        return new Vector3(x - 1.5f, y + 0.5f, CalculateZOffset(beat));
    }

    public static Vector3 CalculatePosition(int x, int y, float beat)
    {
        return CalculatePosition((float)x, (float)y, beat);
    }

    public static Color GetColor(int color)
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
