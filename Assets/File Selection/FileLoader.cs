using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

using System;
using System.IO;

[RequireComponent(typeof(InputField))]
public class FileLoader : MonoBehaviour
{
    public bool createNewSong = false;

    public void LoadScene()
    {
        var input = GetComponent<InputField>();
        if (Directory.Exists(input.text))
        {
            FileAttributes attr = File.GetAttributes(input.text);
            if (attr.HasFlag(FileAttributes.Directory))
            {
                Debug.Log("Folder exists, switching scenes");
                GlobalData.selectedFolder = input.text;
                SceneManager.LoadScene("Editor");
            } else
            {
                Debug.Log("Not a directory");
                input.text = "Not a Directory";
            }
        } else
        {
            Debug.Log("Folder does not exist");
            input.text = "File Not Found";
        }
    }
}
