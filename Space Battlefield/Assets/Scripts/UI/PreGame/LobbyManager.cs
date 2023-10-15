using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Services.Core;
using Unity.Services.Authentication;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using System.Threading.Tasks;
using UnityEngine.SceneManagement;
using Unity.Services.Relay;
using Unity.Services.Relay.Models;

#if UNITY_EDITOR
using ParrelSync;
#endif

public class LobbyManager : MonoBehaviour
{
    public InputField roomNameInput;
    public GameObject roomItemPrefab;
    public GameObject playerItemPrefab;
    public Transform roomsParent;
    public Transform playerParent;
    public GameObject roomPanel;
    public GameObject lobbyPanel;
    public Text roomNameText;

    private List<GameObject> roomsList = new List<GameObject>();
    private List<GameObject> playerList = new List<GameObject>();
    public Lobby currentLobby;
    private float refreshTimer;
    private float heartbeatTimer = 0;

    public GameObject startButton;

    // Lobby Updates
    private LobbyEventCallbacks callbacks;
    private ILobbyEvents events;


    private void Start()
    {
        Authentication();
    }

    // Authentication
    public async void Authentication()
    {
        await Authenticate();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as player id: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
    }

    private async Task Authenticate()
    {
        var options = new InitializationOptions();

#if UNITY_EDITOR
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

        await UnityServices.InitializeAsync(options);
    }

    // List Lobbies and Players
    #region
    private void Update()
    {
        Refresh();
        RefreshPlayers();
        Heartbeat();

        if (currentLobby != null)
        {
            if (currentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                startButton.SetActive(true);
            }
            else
            {
                startButton.SetActive(false);
            }
        }
    }

    private void Refresh()
    {
        if (currentLobby == null && AuthenticationService.Instance.IsSignedIn)
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0)
            {
                refreshTimer = 1.1f;
                ListLobbies();
            }
        }
    }

    private async void ListLobbies()
    {
        try
        {
            QueryResponse queryResponse = await Lobbies.Instance.QueryLobbiesAsync();

            foreach (GameObject room in roomsList)
            {
                Destroy(room);
            }
            roomsList.Clear();

            foreach (Lobby lobby in queryResponse.Results)
            {
                GameObject roomItem = Instantiate(roomItemPrefab, roomsParent);
                roomItem.GetComponentInChildren<RoomItem>().SetRoomName(lobby.Name);
                roomItem.GetComponentInChildren<RoomItem>().SetRoomId(lobby.Id);
                roomItem.GetComponentInChildren<RoomItem>().SetPlayerCount(lobby.Players.Count, lobby.MaxPlayers);
                roomsList.Add(roomItem);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void RefreshPlayers()
    {
        if (currentLobby != null)
        {
            refreshTimer -= Time.deltaTime;
            if (refreshTimer <= 0)
            {
                refreshTimer = 1.1f;
                ListPlayers();
            }
        }
    }

    private async void ListPlayers()
    {
        try
        {
            currentLobby = await Lobbies.Instance.GetLobbyAsync(currentLobby.Id);
            ClearList(playerList);

            foreach (Player player in currentLobby.Players)
            {
                GameObject playerItem = Instantiate(playerItemPrefab, playerParent);
                playerItem.GetComponent<PlayerItem>().SetPlayerData(player.Data["PlayerName"].Value, player.Id);
                playerList.Add(playerItem);

                if (currentLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    if (player.Id != AuthenticationService.Instance.PlayerId)
                    {
                        playerItem.GetComponent<PlayerItem>().EnableKick();
                    }
                }
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void ClearList(List<GameObject> list)
    {
        foreach (GameObject go in list)
        {
            Destroy(go);
        }
        playerList.Clear();
    }

    private async void Heartbeat()
    {
        if (currentLobby != null)
        {
            if (currentLobby.HostId == AuthenticationService.Instance.PlayerId)
            {
                heartbeatTimer -= Time.deltaTime;
                if (heartbeatTimer <= 0)
                {
                    heartbeatTimer = 15;
                    await LobbyService.Instance.SendHeartbeatPingAsync(currentLobby.Id);
                }
            }
        }
    }
    #endregion

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = roomNameInput.text;
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject>()
                {
                    { "Start", new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,
                    value: "0",
                    index: DataObject.IndexOptions.N1) },

                    { "ConnectionKey", new DataObject(
                    visibility: DataObject.VisibilityOptions.Member,
                    value: "null",
                    index: DataObject.IndexOptions.S1) }

                },

                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerData.instance.username) }
                    }
                }
            };           

            Lobby lobby = await LobbyService.Instance.CreateLobbyAsync(lobbyName, maxPlayers, createLobbyOptions);
            currentLobby = lobby;

            OnJoinedLobby(currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void JoinLobby(string roomId)
    {
        try
        {
            JoinLobbyByIdOptions joinLobbyByIdOptions = new JoinLobbyByIdOptions
            {
                Player = new Player
                {
                    Data = new Dictionary<string, PlayerDataObject>
                    {
                        { "PlayerName", new PlayerDataObject(PlayerDataObject.VisibilityOptions.Member, PlayerData.instance.username) }
                    }
                }
            };

            currentLobby = await Lobbies.Instance.JoinLobbyByIdAsync(roomId, joinLobbyByIdOptions);

            // Events
            callbacks = new LobbyEventCallbacks();
            callbacks.LobbyChanged += OnLobbyChanged;
            callbacks.KickedFromLobby += KickedFromLobby;

            events = await Lobbies.Instance.SubscribeToLobbyEventsAsync(roomId, callbacks);

            OnJoinedLobby(currentLobby.Name);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    // Lobby Utility
    #region
    private void OnJoinedLobby(string name)
    {
        roomPanel.SetActive(true);
        roomNameText.text = name;
        lobbyPanel.SetActive(false);
    }

    public async void KickPlayer(string id)
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, id);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void LeaveLobby()
    {
        try
        {
            await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
            roomPanel.SetActive(false);
            lobbyPanel.SetActive(true);
            currentLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void OnClickMainMenu()
    {
        try
        {
            if (currentLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            AuthenticationService.Instance.SignOut();
            Destroy(PlayerData.instance.gameObject);
            SceneManager.LoadScene("MainMenu");
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }
    #endregion

    private void OnLobbyChanged(ILobbyChanges changes)
    {
        if (changes.LobbyDeleted)
        {
            ClearList(playerList);
            currentLobby = null;
            roomPanel.SetActive(false);
            lobbyPanel.SetActive(false);
        }
        if (changes.Data.Changed)
        {
            if (changes.Data.Value.ContainsKey("Start"))
            {
                if (changes.Data.Value["Start"].Value.Value == "1")
                {
                    string key = changes.Data.Value["ConnectionKey"].Value.Value;
                    PlayerData.instance.key = key;
                    PlayerData.instance.currentLobby = currentLobby;
                    SceneManager.LoadScene("Game");
                }
            }
        }
    }

    private void KickedFromLobby()
    {
        ClearList(playerList);
        currentLobby = null;
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public async void OnClickStart()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(currentLobby.MaxPlayers);
        PlayerData.instance.allocation = allocation;

        UpdateLobbyOptions updateOptions = new UpdateLobbyOptions();
        updateOptions.Data = currentLobby.Data;
        currentLobby.Data.Remove("Start");
        currentLobby.Data.Remove("ConnectionKey");

        updateOptions.Data.Add("Start",

            new DataObject(
                visibility: DataObject.VisibilityOptions.Member,
                value: "1",
                index: DataObject.IndexOptions.N1));

        updateOptions.Data.Add("ConnectionKey",

            new DataObject(
                visibility: DataObject.VisibilityOptions.Member,
                value: await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId),
                index: DataObject.IndexOptions.S1));

        await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateOptions);
        PlayerData.instance.currentLobby = currentLobby;

        SceneManager.LoadScene("Game");
    }
}