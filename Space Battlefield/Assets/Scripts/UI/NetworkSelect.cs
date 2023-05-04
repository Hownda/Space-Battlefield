using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

/// <summary>
/// Developer tool to skip lobby system.
/// </summary>
public class NetworkSelect : MonoBehaviour
{
    public Button hostButton;
    public Button serverButton;
    public Button clientButton;

    public GameObject eventManager;

    private void Awake()
    {
        Application.targetFrameRate = 500;
    }
    public void OnClickHost()
    {
        NetworkManager.Singleton.StartHost();
        DisableButtons();
    }

    public void OnClickServer()
    {
        NetworkManager.Singleton.StartServer();
        DisableButtons();
    }
    public void OnClickClient()
    {
        NetworkManager.Singleton.StartClient();
        DisableButtons();
    }

    private void DisableButtons()
    {
        hostButton.gameObject.SetActive(false);
        serverButton.gameObject.SetActive(false);
        clientButton.gameObject.SetActive(false);
        //Camera.main.GetComponent<TerrainMovement>().enabled = false;
    }
}
