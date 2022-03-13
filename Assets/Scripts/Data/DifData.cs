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
public class NoteSerial : SpawnableSerial
{
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
public class BombSerial : SpawnableSerial
{
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
public class ObstacleSerial : SpawnableSerial
{
    public int w;
    public int h;
    public float d;

    public ObstacleSerial(float beat, int x, int y, int width, int height, float duration)
    {
        this.b = beat;
        this.x = x;
        this.y = y;
        this.w = width;
        this.h = height;
        this.d = duration;
    }

    public static explicit operator Obstacle(ObstacleSerial obstacle)
    {
        return new Obstacle(obstacle.b, obstacle.x, obstacle.y, obstacle.w, obstacle.h, obstacle.d);
    }
}

[Serializable]
public class SliderSerial : SpawnableSerial
{
    public int c;
    public int d;
    public float tb; // tail beat
    public int tx;   // tail x
    public int ty;   // tail y
    public int tc;   // tail cut direction
    public int mu;   // length multiplier
    public int tmu;  // tail length multiplier
    public int m;    // mid anchor mode

    public SliderSerial(float beat, int x, int y, int color, int cutDirection, float tailBeat, int tailX, int tailY, int tailDirection, int lengthMultiplier, int tailLengthMultiplier, int anchor)
    {
        this.b = beat;
        this.x = x;
        this.y = y;
        this.c = color;
        this.d = cutDirection;
        this.tb = tailBeat;
        this.tx = tailX;
        this.ty = tailY;
        this.tc = tailDirection;
        this.mu = lengthMultiplier;
        this.tmu = tailLengthMultiplier;
        this.m = anchor;
    }

    public static explicit operator Rail(SliderSerial sliderSerial)
    {
        return new Rail(sliderSerial.b, sliderSerial.x, sliderSerial.y, sliderSerial.c, sliderSerial.d, sliderSerial.tb, sliderSerial.tx, sliderSerial.ty, sliderSerial.tc, sliderSerial.mu, sliderSerial.tmu, sliderSerial.m);
    }
}

[Serializable]
public class BurstSliderSerial : SpawnableSerial
{
    public int c;
    public int d;
    public float tb;
    public int tx;
    public int ty;
    public int sc;  // slice count
    public float s; // squash

    public BurstSliderSerial(float beat, int x, int y, int color, int cutDirection, float tailBeat, int tailX, int tailY, int sliceCount, float squash)
    {
        this.b = beat;
        this.x = x;
        this.y = y;
        this.c = color;
        this.d = cutDirection;
        this.tb = tailBeat;
        this.tx = tailX;
        this.ty = tailY;
        this.sc = sliceCount;
        this.s = squash;
    }

    public static explicit operator BurstSlider(BurstSliderSerial bs)
    {
        return new BurstSlider(bs.b, bs.x, bs.y, bs.c, bs.d, bs.tb, bs.tx, bs.ty, bs.sc, bs.s);
    }
}