using System;

[Serializable]
public class DifData
{
    public string version;
    //public Event[] _events;
    public NoteSerial[] colorNotes;
    public BombSerial[] bombNotes;
    public ObstacleSerial[] obstacles;
    public SliderSerial[] sliders;
    public BurstSliderSerial[] burstSliders;
}

// Removed until I can figure out the new format. DM me if you wish to provide the information for the event format system.
/*[Serializable]
public class Event
{
    public float _time;
    public int _type;
    public int _value;
}*/

[Serializable]
public class NoteSerial
{
    public float b;             // time
    public int x;               // line index
    public int y;               // line layer
    public int c;               // type
    public int d;               // cut direction
    public int a;               // NEW: angle offset

    public NoteSerial(float beat, int x, int y, int type, int cutDirection, int angleOffset)
    {
        this.b = beat;
        this.x = x;
        this.y = y;
        this.c = type;
        this.d = cutDirection;
        this.a = angleOffset;
    }

    public static explicit operator Note(NoteSerial note)
    {
        return new Note(note.b, note.x, note.y, note.c, note.d, note.a);
    }
}

[Serializable]
public class BombSerial
{
    public float b;
    public int x;
    public int y;

    public BombSerial(float beat, int x, int y)
    {
        this.b = beat;
        this.x = x;
        this.y = y;
    }

    public static explicit operator Bomb(BombSerial bomb)
    {
        return new Bomb(bomb.b, bomb.x, bomb.y);
    }
    // There might be some parts missing here. DM me at TonyGamer#7947 if there is something missing here.
}

[Serializable]
public class ObstacleSerial
{
    public float b;
    public int x;
    public int y;
}

[Serializable]
public class SliderSerial
{
    public float b;
    public int x;
    public int y;
    public int c;
    public int d;
    public float tb;
    public int tx;
    public int ty;
    public int tc;
    public int mu;
    public int m;
}

[Serializable]
public class BurstSliderSerial
{
    public float b;
    public int x;
    public int y;
    public int c;
    public int d;
    public float tb;
    public int tx;
    public int ty;
    public int sc;
    public float s;
}