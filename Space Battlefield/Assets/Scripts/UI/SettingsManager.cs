using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public Slider volumeSlider;    
    public Dropdown resolutionDropdown;
    public Toggle isFullscreen;
    public Slider sensitivitySlider;

    public AudioMixer audioMixer;

    Resolution[] resolutions;

    private void Start()
    {
        LoadResolutions();
        LoadGraphicsSettings();
        LoadControlsSettings();
        LoadSoundSettings();
    } 

    private void LoadResolutions()
    {
        resolutions = Screen.resolutions;

        resolutionDropdown.ClearOptions();

        List<string> options = new();

        int currentResolutionIndex = 0;
        for (int i = 0; i < resolutions.Length; i++)
        {
            string option = resolutions[i].width + " x " + resolutions[i].height;
            options.Add(option);

            if (resolutions[i].width == Screen.currentResolution.width
                && resolutions[i].height == Screen.currentResolution.height)
                currentResolutionIndex = i;
        }
        resolutionDropdown.AddOptions(options);
        resolutionDropdown.value = currentResolutionIndex;
        resolutionDropdown.RefreshShownValue();
    }

    public void SetResolution()
    {
        Resolution resolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

public void SetScreenMode()
    {
        if (Screen.fullScreen == true)
        {
            PlayerPrefs.SetInt("Fullscreen", 0);
            Screen.fullScreen = false;
        }
        else
        {
            PlayerPrefs.SetInt("Fullscreen", 1);
            Screen.fullScreen = true;
        }
    }

    public void SetSensitivity()
    {
        PlayerData.instance.mouseSensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat("Sensitivity", sensitivitySlider.value);
    }

    public void SetVolume()
    {
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volumeSlider.value) * 20);
        PlayerPrefs.SetFloat("Volume", Mathf.Log10(volumeSlider.value) * 20);
    }

    public void OnClickGraphics()
    {
        LoadGraphicsSettings();
    }

    public void OnClickControls()
    {
        LoadControlsSettings();
    }

    public void OnClickSound()
    {
        LoadSoundSettings();
    }

    private void LoadGraphicsSettings()
    {
        if (PlayerPrefs.HasKey("ResolutionIndex"))
        {
            resolutionDropdown.value = PlayerPrefs.GetInt("ResolutionIndex");
            SetResolution();
        }
        else
        {
            SetResolution();
        }

        if (PlayerPrefs.HasKey("Fullscreen"))
        {
            if (PlayerPrefs.GetInt("Fullscreen") == 1)
            {
                Screen.fullScreen = true;
            }    
            else
            {
                Screen.fullScreen = false;
            }
        }
    }

    private void LoadControlsSettings()
    {
        if (PlayerPrefs.HasKey("Sensitivity"))
        {
            sensitivitySlider.value = PlayerPrefs.GetFloat("Sensitivity");
            SetSensitivity();
        }
        else
        {
            SetSensitivity();
        }
    }

    private void LoadSoundSettings()
    {
        if (PlayerPrefs.HasKey("Volume"))
        {
            volumeSlider.value = Mathf.Pow(10, PlayerPrefs.GetFloat("Volume")/20);
            SetVolume();
        }
        else
        {
            SetVolume();
        }
    }
}
