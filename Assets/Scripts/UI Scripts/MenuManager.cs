using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class MenuManager : MonoBehaviour
{
    [Header("Menus")]
    public Button[] buttons;
    public Text[] texts;

    public void ChangeMenu(int index)
    {
        foreach(Text text in texts)
        {
            text.gameObject.SetActive(false);
        }

        texts[index].gameObject.SetActive(true);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
