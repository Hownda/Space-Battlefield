using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using UnityEngine.SceneManagement;

public class Options : MonoBehaviour
{
    public GameObject optionsPanel;
    public static Options instance;

    public GameObject settingsPanel;

    public bool disableCameraMovement = false;

    private void Awake()
    {
        instance = this;
    }
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleOptions();
        }
    }

    public void OnClickResume()
    {
        ToggleOptions();
    }
    
    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
        optionsPanel.SetActive(false);
    }

    public void OnClickLeave()
    {
        PhotonNetwork.LeaveRoom();
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

    public void ToggleOptions()
    {
        if (disableCameraMovement)
        {
            optionsPanel.SetActive(false);
            settingsPanel.SetActive(false);
            disableCameraMovement = false;
            Cursor.lockState = CursorLockMode.Locked;
        }
        else
        {
            optionsPanel.SetActive(true);
            disableCameraMovement = true;
            Cursor.lockState = CursorLockMode.None;
        }
    }

}
