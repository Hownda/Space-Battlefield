using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public class Username : MonoBehaviour
{
    public Text usernameText;
    public InputField usernameInput;
    public Button connectButton;
    public Button changeNameButton;

    private void Start()
    {
        if (PlayerData.instance.GetUsername() != null)
        {
            usernameText.text = PlayerData.instance.GetUsername();
            connectButton.gameObject.SetActive(true);
            changeNameButton.gameObject.SetActive(true);
            usernameInput.gameObject.SetActive(false);
        }
        else 
        {
            usernameText.text = "You have not yet configured a username.";
            connectButton.gameObject.SetActive(false);
            usernameInput.gameObject.SetActive(true);
        }
    }

    private void Update()
    {
        if (usernameInput.text.Length > 3)
        {
            connectButton.gameObject.SetActive(true);
        }
        else
        {
            connectButton.gameObject.SetActive(false);
        }
    }

    public void OnClickChangeUsername()
    {
        changeNameButton.gameObject.SetActive(false);
        usernameInput.gameObject.SetActive(true);
    }

    public void OnClickConnect()
    {        
        if (usernameInput.text.Length > 3)
        {
            PlayerData.instance.SetUsername(usernameInput.text);
        }
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
