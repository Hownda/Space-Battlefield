using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Linq;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    // Networking
    public bool isHost = false;
    public string username;
    private bool inGame = false;
    public Allocation allocation;
    public string key;

    // Settings
    public GameObject settingsCanvas;
    public GameObject settingsPanel;
    public GameObject options;
    public float mouseSensitivity = 200;
    public bool disableCameraMovement;

    //Temporary until Netcode 1.4.1
    [SerializeField] private NetworkPrefabsList _networkPrefabsList;

    private void Awake()
    {
        instance = this;
        username = null;
        DontDestroyOnLoad(gameObject);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            ToggleSettings();
        }

        ManageRelay();        
    }

    private void ManageRelay()
    {
        if (SceneManager.GetActiveScene().name == "Game" && inGame == false)
        {
            if (isHost && allocation != null)
            {
                TemporaryNetworkFix();
                CreateRelay();
            }
            if (isHost == false)
            {
                TemporaryNetworkFix();
                JoinRelay();
            }
            inGame = true;
        }
    }

    private void CreateRelay()
    {
        try
        {
            NetworkManager.Singleton.GetComponent<UnityTransport>().SetHostRelayData(
                allocation.RelayServer.IpV4,
                (ushort)allocation.RelayServer.Port,
                allocation.AllocationIdBytes,
                allocation.Key,
                allocation.ConnectionData
                );

            NetworkManager.Singleton.StartHost();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    private async void JoinRelay()
    {
        try
        {
            JoinAllocation joinAllocation = await RelayService.Instance.JoinAllocationAsync(key);

            NetworkManager.Singleton.GetComponent<UnityTransport>().SetClientRelayData(
                joinAllocation.RelayServer.IpV4,
                (ushort)joinAllocation.RelayServer.Port,
                joinAllocation.AllocationIdBytes,
                joinAllocation.Key,
                joinAllocation.ConnectionData,
                joinAllocation.HostConnectionData
                );

            NetworkManager.Singleton.StartClient();
        }
        catch (RelayServiceException e)
        {
            Debug.Log(e);
        }
    }

    public void ToggleSettings()
    {
        // Deactivate
        if (settingsCanvas.activeInHierarchy)
        {            
            if (SceneManager.GetActiveScene().name == "Game")
            {
                settingsPanel.SetActive(false);
                disableCameraMovement = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {                
                Cursor.lockState = CursorLockMode.None;
            }
            options.SetActive(false);
            settingsCanvas.SetActive(false);
        }
        // Activate
        else
        {
            if (SceneManager.GetActiveScene().name == "Game")
            {
                options.SetActive(true);
                settingsPanel.SetActive(false);
                disableCameraMovement = true;
            }
            else
            {
                options.SetActive(false);
                Cursor.lockState = CursorLockMode.None;
            }
            settingsCanvas.SetActive(true);
        }
    } 

    private void TemporaryNetworkFix()
    {
        var prefabs = _networkPrefabsList.PrefabList.Select(x => x.Prefab);
        foreach (var prefab in prefabs)
        {
            NetworkManager.Singleton.AddNetworkPrefab(prefab);
        }
    }
}
