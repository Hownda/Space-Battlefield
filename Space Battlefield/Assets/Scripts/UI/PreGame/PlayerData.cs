using UnityEngine;
using UnityEngine.SceneManagement;
using Unity.Netcode;
using Unity.Netcode.Transports.UTP;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;
using System.Linq;
using Unity.Services.Lobbies.Models;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;

    // Networking
    public bool isDemo = false;
    public bool isHost = false;
    public string username;
    private bool inGame = false;
    public Allocation allocation;
    public string key;
    public string currentLobbyId;
    public int playerCount;

    // Settings
    public GameObject settingsCanvas;
    public GameObject options;
    public bool disableCameraMovement = false;
    public float mouseSensitivity = 200;

    public GameObject crashPanel;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);

        if (PlayerPrefs.HasKey("Username"))
        {
            username = PlayerPrefs.GetString("Username");
        }
        else
        {
            username = null;
        }
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
        if (isDemo)
        {
            if (SceneManager.GetActiveScene().name == "Game" && inGame == false)
            {
                NetworkManager.Singleton.GetComponent<UnityTransport>().SetConnectionData("127.0.0.1", 7777);
                NetworkManager.Singleton.StartHost();
                inGame = true;
            }
        }
        else
        {
            if (SceneManager.GetActiveScene().name == "Game" && inGame == false)
            {
                if (isHost && allocation != null)
                {
                    CreateRelay();
                }
                if (isHost == false)
                {
                    JoinRelay();
                }
                inGame = true;
            }
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
        if (settingsCanvas.activeInHierarchy || options.activeInHierarchy)
        {
            if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "Lobby")
            {
                settingsCanvas.SetActive(false);
            }
            else
            {
                settingsCanvas.SetActive(false);
                options.SetActive(false);
            }
            disableCameraMovement = false;
        }
        // Activate
        else
        {
            if (SceneManager.GetActiveScene().name == "MainMenu" || SceneManager.GetActiveScene().name == "Lobby")
            {
                settingsCanvas.SetActive(true);
            }
            else
            {
                options.SetActive(true);
            }
            disableCameraMovement = true;
        }
    }

    public void ShowCrash()
    {
        crashPanel.SetActive(true);
    }

    public void OnClickClose()
    {
        crashPanel.SetActive(false);
    }
}
