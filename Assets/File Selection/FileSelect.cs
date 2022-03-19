using System.Text;
using System.Collections;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using SFB;

[RequireComponent(typeof(Button))]
public class FileSelect : MonoBehaviour, IPointerDownHandler {
    public InputField input;

    public void OnPointerDown(PointerEventData eventData) { }

    void Start() {
        var button = GetComponent<Button>();
        button.onClick.AddListener(OnClick);
    }

    private void OnClick() {
        var paths = StandaloneFileBrowser.OpenFolderPanel("", "", false);
        if (paths.Length > 0) {
            input.text = System.IO.Directory.GetParent(paths[0]).FullName;
        }
    }
}