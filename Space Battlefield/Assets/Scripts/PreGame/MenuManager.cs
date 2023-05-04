using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Video;

public class MenuManager : MonoBehaviour
{
    [Header("Main Menu")]
    public GameObject settingsPanel;
    public GameObject mainPanel;

    // Main Menu 
    public void OnClickPlay()
    {
        SceneManager.LoadScene("Username");
    }

    public void OnClickDemo()
    {
        SceneManager.LoadScene("Demo");
    }

    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
        mainPanel.SetActive(false);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

}