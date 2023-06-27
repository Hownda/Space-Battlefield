using UnityEngine;
using UnityEngine.SceneManagement;

public class Options : MonoBehaviour
{
    public GameObject optionsPanel;
    public static Options instance;

    public GameObject settingsPanel;

    private void Awake()
    {
        instance = this;
    }

    private void OnEnable()
    {
        Cursor.lockState = CursorLockMode.None;        
    }

    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void OnClickLeave()
    {
        SceneManager.LoadScene("MainMenu");
        Cursor.lockState = CursorLockMode.None;
        Destroy(PlayerData.instance.gameObject);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }
}
