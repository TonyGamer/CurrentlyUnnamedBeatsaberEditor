using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FileSelectSceneManager : MonoBehaviour
{
    public GameObject fileSelect;
    public GameObject actionSelect;

    public FileLoader fileLoader;

    void Start()
    {
        fileLoader = fileLoader.GetComponent<FileLoader>();
    }

    public void SwitchToLoadSong()
    {
        fileSelect.SetActive(true);
        actionSelect.SetActive(false);

        fileLoader.createNewSong = false;
    }

    public void SwitchToNewSong()
    {
        fileSelect.SetActive(true);
        actionSelect.SetActive(false);

        fileLoader.createNewSong = true;
    }

    public void SwitchToActionSelect()
    {
        fileSelect.SetActive(false);
        actionSelect.SetActive(true);
    }
}
