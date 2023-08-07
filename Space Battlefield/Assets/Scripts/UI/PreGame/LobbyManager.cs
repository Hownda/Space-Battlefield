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
    private Lobby currentLobby;
    private Lobby hostedLobby;
    private float refreshTimer;

    public GameObject errorWindow;
    public GameObject startButton;

    private void Start()
    {
        Authentication();
    }

    private void Update()
    {
        Refresh();
        RefreshPlayers();

        if (hostedLobby != null)
        {
            startButton.SetActive(true);
        }
        else
        {
            startButton.SetActive(false);
        }
    }

    public async void Authentication()
    {
        await Authenticate();

        AuthenticationService.Instance.SignedIn += () =>
        {
            Debug.Log("Signed in as player id: " + AuthenticationService.Instance.PlayerId);
        };

        await AuthenticationService.Instance.SignInAnonymouslyAsync();
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

    private async Task Authenticate()
    {
        var options = new InitializationOptions();

#if UNITY_EDITOR
        options.SetProfile(ClonesManager.IsClone() ? ClonesManager.GetArgument() : "Primary");
#endif

        await UnityServices.InitializeAsync(options);
    }

    public async void CreateLobby()
    {
        try
        {
            string lobbyName = roomNameInput.text;
            int maxPlayers = 4;

            CreateLobbyOptions createLobbyOptions = new CreateLobbyOptions()
            {
                Data = new Dictionary<string, DataObject> {
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
            hostedLobby = lobby;
            currentLobby = hostedLobby;

            Debug.Log("Created Lobby with name: " + lobbyName + " and player count: " + maxPlayers.ToString());
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

            await Lobbies.Instance.JoinLobbyByIdAsync(roomId, joinLobbyByIdOptions);
            currentLobby = await Lobbies.Instance.GetLobbyAsync(roomId);

            OnJoinedLobby(currentLobby.Name);
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
            hostedLobby = null;
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
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
                roomsList.Add(roomItem);
            }
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    private void OnJoinedLobby(string name)
    {
        roomPanel.SetActive(true);
        roomNameText.text = name;
        lobbyPanel.SetActive(false);
    }

    private async void ListPlayers()
    {
        try
        {
            Lobby joinedLobby = await Lobbies.Instance.GetLobbyAsync(currentLobby.Id);
            if (joinedLobby.Data != null)
            {
                if (joinedLobby.Data["Start"].Value == "1" && joinedLobby.Data["ConnectionKey"].Value != "null")
                {
                    PlayerData.instance.currentLobbyId = currentLobby.Id;
                    SceneManager.LoadScene("Game");
                    PlayerData.instance.key = joinedLobby.Data["ConnectionKey"].Value;
                }
            }

            ClearList(playerList);

            foreach (Player player in joinedLobby.Players)
            {
                if (player.Data == null)
                {
                    ClearList(playerList);
                    currentLobby = null;
                    roomPanel.SetActive(false);
                    lobbyPanel.SetActive(false);
                    errorWindow.SetActive(true);
                    break;
                }

                GameObject playerItem = Instantiate(playerItemPrefab, playerParent);
                playerItem.GetComponent<PlayerItem>().SetPlayerData(player.Data["PlayerName"].Value, player.Id);
                playerList.Add(playerItem);

                if (joinedLobby.HostId == AuthenticationService.Instance.PlayerId)
                {
                    hostedLobby = currentLobby;
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

    public async void OnClickMainMenu()
    {
        try
        {
            if (currentLobby != null)
            {
                await LobbyService.Instance.RemovePlayerAsync(currentLobby.Id, AuthenticationService.Instance.PlayerId);
            }
            AuthenticationService.Instance.SignOut();
            SceneManager.LoadScene("MainMenu");
            Destroy(PlayerData.instance.gameObject);
        }
        catch (LobbyServiceException e)
        {
            Debug.Log(e);
        }
    }

    public async void OnClickStart()
    {
        Allocation allocation = await RelayService.Instance.CreateAllocationAsync(2);
        PlayerData.instance.allocation = allocation;
        PlayerData.instance.isHost = true;

        UpdateLobbyOptions updateOptions = new UpdateLobbyOptions();
        updateOptions.Data = new Dictionary<string, DataObject>()
        {
            { "Start", new DataObject(
                visibility: DataObject.VisibilityOptions.Member,
                value: "1",
                index: DataObject.IndexOptions.N1) },

            { "ConnectionKey", new DataObject(
                visibility: DataObject.VisibilityOptions.Member,
                value: await RelayService.Instance.GetJoinCodeAsync(allocation.AllocationId),
                index: DataObject.IndexOptions.S1) }
        };
        await LobbyService.Instance.UpdateLobbyAsync(currentLobby.Id, updateOptions);
        PlayerData.instance.currentLobbyId = currentLobby.Id;

        SceneManager.LoadScene("Game");
    }
}