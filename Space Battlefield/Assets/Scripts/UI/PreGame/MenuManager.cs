using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class MenuManager : MonoBehaviour
{
    public GameObject settingsPanel;
    public Text usernameText;
    public Button changeUsernameButton;
    public InputField usernameInput;
    public Button submitButton;
    public Text errorText;

    private void Start()
    {
        if (PlayerData.instance.username != null)
        {
            usernameText.text = PlayerData.instance.username;
        }
        else
        {
            changeUsernameButton.gameObject.SetActive(false);
            usernameText.text = "";
            usernameInput.gameObject.SetActive(true);
            submitButton.gameObject.SetActive(true);
        }
    }

    public void OnClickChangeUsername()
    {
        changeUsernameButton.gameObject.SetActive(false);
        usernameText.text = "";
        usernameInput.gameObject.SetActive(true);
        submitButton.gameObject.SetActive(true);
    }

    public void OnClickSubmit()
    {
        if (usernameInput.text.Length > 3 && usernameInput.text.Length < 12)
        {
            PlayerData.instance.username = usernameInput.text;
            PlayerPrefs.SetString("Username", usernameInput.text);
            changeUsernameButton.gameObject.SetActive(true);
            usernameText.text = usernameInput.text;
            usernameInput.gameObject.SetActive(false);
            submitButton.gameObject.SetActive(false);
            errorText.gameObject.SetActive(false);

        }
        else
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "Username invalid. Please use a name with more than 3 and less than 12 characters.";
        }
    }

    public void OnClickPlay()
    {
        if (PlayerData.instance.username != null)
        {
            SceneManager.LoadScene("Lobby");
        }
        else
        {
            errorText.gameObject.SetActive(true);
            errorText.text = "You haven't configured a username yet!";
        }
    }

    public void OnClickSettings()
    {
        settingsPanel.SetActive(true);
    }

    public void OnClickQuit()
    {
        Application.Quit();
    }

}