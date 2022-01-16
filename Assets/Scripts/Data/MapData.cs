using System;

[Serializable]
public class MapData
{
    public float _beatsPerMinute;
    public string _songFilename;
    public DifficultyBeatmapSet[] _difficultyBeatmapSets;
}

[Serializable]
public class DifficultyBeatmapSet
{
    public DifficultyBeatmap[] _difficultyBeatmaps;
}

[Serializable]
public class DifficultyBeatmap
{
    public string _beatmapFilename;
    public float _noteJumpMovementSpeed;
    public float _noteJumpStartBeatOffset;
}