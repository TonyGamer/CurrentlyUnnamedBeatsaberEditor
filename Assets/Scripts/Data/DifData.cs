using System;

[Serializable]
public class DifData
{
    public string _version;
    public Event[] _events;
    public NoteSerial[] _notes;
}

[Serializable]
public class Event
{
    public float _time;
    public int _type;
    public int _value;
}

[Serializable]
public class NoteSerial
{
    public float _time;
    public int _lineIndex;
    public int _lineLayer;
    public int _type;
    public int _cutDirection;

    public NoteSerial(float time, int lineIndex, int lineLayer, int type, int cutDirection)
    {
        this._time = time;
        this._lineIndex = lineIndex;
        this._lineLayer = lineLayer;
        this._type = type;
        this._cutDirection = cutDirection;
    }

    public static explicit operator Note(NoteSerial note)
    {
        return new Note(note._time, note._lineIndex, note._lineLayer, note._type, note._cutDirection);
    }
}