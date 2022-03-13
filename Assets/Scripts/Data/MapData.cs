using System;

[Serializable]
public class MapData
{
    public string _version;
    public float _beatsPerMinute;
    public float _songTimeOffset;
    public string _songFilename;
    public DifficultyBeatmapSet[] _difficultyBeatmapSets;
}

[Serializable]
public class DifficultyBeatmapSet
{
    public string _beatmapCharacteristicName;
    public DifficultyBeatmap[] _difficultyBeatmaps;
}

[Serializable]
public class DifficultyBeatmap
{
    public string _difficulty;
    public string _beatmapFilename;
    public float _noteJumpMovementSpeed;
    public float _noteJumpStartBeatOffset;
}