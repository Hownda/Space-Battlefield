using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class Settings : MonoBehaviour
{
    public float volume;

    private void Start()
    {
        Load();
    }

    private void OnDestroy()
    {
        Save();
    }

    public void Save()
    {
        PlayerPrefs.SetFloat("Volume", volume);
        PlayerPrefs.SetInt("Fullscreen", Screen.fullScreen ? 1 : 0);
    }

    public void Load()
    {
        // Sound Settings
        volume = PlayerPrefs.GetFloat("Volume");

        // Window Settings
        if (PlayerPrefs.GetInt("Fullscreen") == 1)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreen = false;
        }     
    }

    public void OnClickWindowSettings()
    {

    }

    public void OnClickKeyboardSettings()
    {

    }

    public void OnClickAudioSettings()
    {

    }
}
