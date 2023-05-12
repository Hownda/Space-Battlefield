using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SettingsManager : MonoBehaviour
{
    public static SettingsManager instance;

    public Toggle isFullscreen;
    public Slider volumeSlider;
    public AudioMixer audioMixer;

    #region RESOLUTION

    public Dropdown resolutionDropdown;

    public Slider sensitivitySlider;

    Resolution[] resolutions;

    private void Awake()
    {
        instance = this;
    }
    private void Start()
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

        // Volume Settings
        if (PlayerPrefs.HasKey("MasterVolume"))
        {
            LoadVolume();
        }
        else
        {
            SetMusicVolume();
        }

        if (PlayerPrefs.HasKey("Sensitivity"))
        {
            return;
        }
        else
        {
            SetSensitivity();
        }
    }

    public void SetResolution()
    {
        Resolution resolution = resolutions[resolutionDropdown.value];
        Screen.SetResolution(resolution.width, resolution.height, Screen.fullScreen);
    }

    #endregion

    public void SetScreenMode(int screenModeIndex)
    {
        if (screenModeIndex == 0)
        {
            Screen.fullScreen = true;
        }
        else
        {
            Screen.fullScreen = false;
        }
    }

    public void SetFullscreen()
    {
        Screen.fullScreen = isFullscreen.isOn;
        Debug.Log(isFullscreen.isOn);
    }

    public void SetMusicVolume()
    {
        float volume = volumeSlider.value;
        audioMixer.SetFloat("MasterVolume", Mathf.Log10(volume)*20);
        PlayerPrefs.SetFloat("MasterVolume", volume);
    }

    private void LoadVolume()
    {
        volumeSlider.value = PlayerPrefs.GetFloat("MasterVolume");
    }

    public void SetSensitivity()
    {
        float sensitivity = sensitivitySlider.value;
        PlayerPrefs.SetFloat("Sensitivity", sensitivity);
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponentInChildren<CameraScript>() != null) {
                player.GetComponentInChildren<CameraScript>().mouseSensitivity = sensitivity;
            }
            if (player.GetComponentInChildren<CameraScriptDemo>() != null)
            {
                player.GetComponentInChildren<CameraScriptDemo>().mouseSensitivity = sensitivity;
            }
        }
    }
}
