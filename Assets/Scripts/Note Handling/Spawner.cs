using System.Collections.Generic;
using UnityEngine;

public class Spawner : MonoBehaviour
{
    public GameObject noteObject;
    public GameObject dotObject;
    public GameObject bombObject;
    public GameObject wallObject;
    public GameObject railObject;
    public GameObject bsObject;

    private static List<GameObject> spawnables = new List<GameObject>();

    public Spawnable SpawnSpawnable(SpawnableSerial spawnable, int index)
    {
        foreach (GameObject testSpawn in spawnables)
        {
            Spawnable spawnComp = testSpawn.GetComponent<Spawnable>();
            if (spawnComp.index == index)
            {
                return null;
            }
        }

        switch(spawnable.GetType().Name)
        {
            case "NoteSerial":
                return SpawnNote(spawnable as NoteSerial, index);
            case "BombSerial":
                return SpawnBomb(spawnable as BombSerial, index);
            case "ObstacleSerial":
                return SpawnObstacle(spawnable as ObstacleSerial, index);
            case "SliderSerial":
                return SpawnRail(spawnable as SliderSerial, index);
            case "BurstSliderSerial":
                return SpawnBurstSlider(spawnable as BurstSliderSerial, index);
            default:
                Debug.LogWarning("Unknown type '" + spawnable.GetType().Name + "'");
                return null;
        }
    }

    public Note SpawnNote(NoteSerial noteToSpawn, int index)
    {
        return SpawnNote(index, noteToSpawn.x, noteToSpawn.y, noteToSpawn.c, noteToSpawn.d, noteToSpawn.b, noteToSpawn.a);
    }

    public Note SpawnNote(int index, int x, int y, int color, int cutDirection, float beat, int angleOffset)
    {
        GameObject noteMesh = noteObject;

        if (cutDirection == 8)
        {
            noteMesh = dotObject;
        }

        GameObject spawnedNote = Instantiate(noteMesh, Vector3.zero, Quaternion.identity);
        Note noteComp = spawnedNote.GetComponent<Note>();

        spawnedNote.GetComponent<Renderer>().material.color = GetColor(color);

        noteComp.index = index;
        noteComp.beat = beat;
        noteComp.x = x;
        noteComp.y = y;
        noteComp.cutDirection = cutDirection;
        noteComp.color = color;

        noteComp.Start();
        noteComp.Update();

        spawnables.Add(spawnedNote);

        return noteComp;
    }

    public Bomb SpawnBomb(BombSerial bombToSpawn, int index)
    {
        return SpawnBomb(index, bombToSpawn.b, bombToSpawn.x, bombToSpawn.y);
    }

    public Bomb SpawnBomb(int index, float beat, int x, int y)
    {
        GameObject spawnedBomb = Instantiate(bombObject, Vector3.zero, Quaternion.identity);
        Bomb bombComp = spawnedBomb.GetComponent<Bomb>();

        bombComp.index = index;
        bombComp.beat = beat;
        bombComp.x = x;
        bombComp.y = y;

        bombComp.Update();

        spawnables.Add(spawnedBomb);

        return bombComp;
    }

    public Obstacle SpawnObstacle(ObstacleSerial obstacleToSpawn, int index)
    {
        return SpawnObstacle(index, obstacleToSpawn.b, obstacleToSpawn.x, obstacleToSpawn.y, obstacleToSpawn.w, obstacleToSpawn.h, obstacleToSpawn.d);
    }

    public Obstacle SpawnObstacle(int index, float beat, int x, int y, int width, int height, float duration)
    {
        GameObject spawnedObstacle = Instantiate(wallObject, Vector3.zero, Quaternion.identity);

        Obstacle obstacleComp = spawnedObstacle.GetComponent<Obstacle>();

        obstacleComp.index = index;
        obstacleComp.beat = beat;
        obstacleComp.x = x;
        obstacleComp.y = y;
        obstacleComp.width = width;
        obstacleComp.height = height;
        obstacleComp.duration = duration;

        obstacleComp.Update();

        spawnables.Add(spawnedObstacle);

        return obstacleComp;
    }

    public Rail SpawnRail(SliderSerial sliderToSpawn, int index)
    {
        return SpawnRail(index, sliderToSpawn.b, sliderToSpawn.x, sliderToSpawn.y, sliderToSpawn.c, sliderToSpawn.d, sliderToSpawn.tb, sliderToSpawn.tx, sliderToSpawn.ty, sliderToSpawn.tc, sliderToSpawn.mu, sliderToSpawn.tmu, sliderToSpawn.m); // Lotta variables to pass in
    }

    public Rail SpawnRail(int index, float beat, int x, int y, int color, int cutDirection, float tailBeat, int tailX, int tailY, int tailDirection, int lengthMultiplier, int tailLengthMultiplier, int anchor)
    {
        GameObject spawnedRail = Instantiate(railObject, Vector3.zero, Quaternion.identity);
        Rail railComp = spawnedRail.GetComponent<Rail>();

        spawnedRail.GetComponent<LineRenderer>().material.color = GetColor(color);

        railComp.index = index;
        railComp.beat = beat;
        railComp.x = x;
        railComp.y = y;
        railComp.color = color;
        railComp.cutDirection = cutDirection;
        railComp.tailBeat = tailBeat;
        railComp.tailX = tailX;
        railComp.tailY = tailY;
        railComp.tailDirection = tailDirection;
        railComp.lengthMultiplier = lengthMultiplier;
        railComp.tailLengthMultiplier = tailLengthMultiplier;
        railComp.anchor = anchor;

        railComp.Start();
        railComp.Update();

        spawnables.Add(spawnedRail);

        return railComp;
    }

    public BurstSlider SpawnBurstSlider(BurstSliderSerial bsToSpawn, int index)
    {
        return SpawnBurstSlider(index, bsToSpawn.b, bsToSpawn.x, bsToSpawn.y, bsToSpawn.c, bsToSpawn.d, bsToSpawn.tb, bsToSpawn.tx, bsToSpawn.ty, bsToSpawn.sc, bsToSpawn.s);
    }

    public BurstSlider SpawnBurstSlider(int index, float beat, int x, int y, int color, int cutDirection, float tailBeat, int tailX, int tailY, int sliceCount, float squash)
    {
        GameObject spawnedBS = Instantiate(bsObject, Vector3.zero, Quaternion.identity);
        BurstSlider bsComp = spawnedBS.GetComponent<BurstSlider>();

        spawnedBS.GetComponent<Renderer>().material.color = GetColor(color);

        bsComp.index = index;
        bsComp.beat = beat;
        bsComp.x = x;
        bsComp.y = y;
        bsComp.color = color;
        bsComp.cutDirection = cutDirection;
        bsComp.tailBeat = tailBeat;
        bsComp.tailX = tailX;
        bsComp.tailY = tailY;
        bsComp.sliceCount = sliceCount;
        bsComp.squash = squash;

        bsComp.Start();
        bsComp.Update();

        spawnables.Add(spawnedBS);

        return bsComp;
    }

    public void ClearNotes()
    {
        foreach (GameObject spawnable in spawnables)
        {
            Destroy(spawnable);
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
        return GlobalData.jumpSpeed * (beat - GlobalData.currentBeat);
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

    public static void RemoveSpawnable(GameObject spawnable)
    {
        spawnables.Remove(spawnable);
        Destroy(spawnable.gameObject);
    }

    public void AdjustSpawnableIndicies(int index)
    {
        foreach(GameObject spawnableObject in spawnables)
        {
            Spawnable spawnable = spawnableObject.GetComponent<Spawnable>();

            if(spawnable.index >= index)
            {
                spawnable.index++;
            }
        }
    }
}
