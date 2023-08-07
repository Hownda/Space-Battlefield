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
        if (PlayerData.instance.username != null)
        {
            usernameText.text = PlayerData.instance.username;
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
        if (usernameInput.text.Length > 3 && usernameInput.text.Length < 18)
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
            PlayerData.instance.username = usernameInput.text;
        }
        SceneManager.LoadScene("Lobby");
    }

    public void OnClickBack()
    {
        SceneManager.LoadScene("MainMenu");
        Destroy(PlayerData.instance.gameObject);
    }
}
