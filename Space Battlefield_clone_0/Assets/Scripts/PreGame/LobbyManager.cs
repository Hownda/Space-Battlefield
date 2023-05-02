using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Hashtable = ExitGames.Client.Photon.Hashtable;

public class LobbyManager : MonoBehaviourPunCallbacks
{
    public static LobbyManager instance;

    public InputField roomNameField;
    public InputField ipInput;
    public InputField portInput;

    public GameObject lobbyPanel;
    public GameObject roomPanel;
    public GameObject ipPanel;
    public Text roomName;

    public RoomItem roomItemPrefab;
    List<RoomItem> roomItemsList = new List<RoomItem>();
    public Transform contentObject;

    public float timeBetweenUpdates = 1.5f;
    float nextUpdateTime;

    public List<PlayerItem> playerItemsList = new List<PlayerItem>();

    public PlayerItem playerItemPrefab;
    public Transform content;

    [Header("Room Panel UI")]
    public GameObject playButton;

    [Header("IP Menu UI")]
    private string ipAddress;
    private string portAddress;

    private void Start()
    {
        instance = this;
        PhotonNetwork.JoinLobby();
    }

    public void OnClickCreate()
    {
        if (roomNameField.text.Length >= 1)
        {
            ipPanel.SetActive(true);
        }
    }

    public void OnClickConfirmCreate()
    {
        ipAddress = ipInput.text;
        portAddress = portInput.text;
        RoomOptions roomOptions =
        new RoomOptions()
        {
            MaxPlayers = 4,
            BroadcastPropsChangeToAll = true
        };

        Hashtable roomCustomProps = new Hashtable();

        roomCustomProps.Add("ip", ipAddress);
        roomCustomProps.Add("port", portAddress);

        roomOptions.CustomRoomProperties = roomCustomProps;

        PhotonNetwork.CreateRoom(roomNameField.text, roomOptions);        
    }

    public void OnClickLocal()
    {
        ipAddress = "127.0.0.1";
        portAddress = "7777";
        RoomOptions roomOptions =
        new RoomOptions()
        {
            MaxPlayers = 4,
            BroadcastPropsChangeToAll = true
        };

        Hashtable roomCustomProps = new Hashtable();

        roomCustomProps.Add("ip", ipAddress);
        roomCustomProps.Add("port", portAddress);

        roomOptions.CustomRoomProperties = roomCustomProps;

        PhotonNetwork.CreateRoom(roomNameField.text, roomOptions);
    }

    public void OnClickLan()
    {
        ipAddress = "192.168.1.1";
        portAddress = "7777";
        RoomOptions roomOptions =
        new RoomOptions()
        {
            MaxPlayers = 4,
            BroadcastPropsChangeToAll = true
        };

        Hashtable roomCustomProps = new Hashtable();

        roomCustomProps.Add("ip", ipAddress);
        roomCustomProps.Add("port", portAddress);

        roomOptions.CustomRoomProperties = roomCustomProps;

        PhotonNetwork.CreateRoom(roomNameField.text, roomOptions);
    }

    public override void OnJoinedRoom()
    {
        lobbyPanel.SetActive(false);
        ipPanel.SetActive(false);
        roomPanel.SetActive(true);
        roomName.text = PhotonNetwork.CurrentRoom.Name;
        UpdatePlayerList();
    }

    public override void OnRoomListUpdate(List<RoomInfo> roomList)
    {
        if (Time.time >= nextUpdateTime)
        {
            UpdateRoomList(roomList);
            nextUpdateTime = Time.time + timeBetweenUpdates;
        }
    }

    void UpdateRoomList(List<RoomInfo> list)
    {
        foreach (RoomItem item in roomItemsList)
        {
            Destroy(item.gameObject);
        }
        roomItemsList.Clear();

        foreach (RoomInfo room in list)
        {
            RoomItem newRoom = Instantiate(roomItemPrefab, contentObject);
            newRoom.SetRoomName(room.Name);
            roomItemsList.Add(newRoom);
        }
    }

    public void JoinRoom(string roomName)
    {
        PhotonNetwork.JoinRoom(roomName);
    }

    public void OnClickLeaveRoom()
    {
        PhotonNetwork.LeaveRoom();
    }

    public override void OnLeftRoom()
    {
        roomPanel.SetActive(false);
        lobbyPanel.SetActive(true);
    }

    public override void OnConnectedToMaster()
    {
        PhotonNetwork.JoinLobby();
    }

    public void UpdatePlayerList()
    {
        foreach (PlayerItem item in playerItemsList)
        {
            Destroy(item.gameObject);
        }
        playerItemsList.Clear();

        if (PhotonNetwork.CurrentRoom == null)
        {
            return;
        }

        foreach (KeyValuePair<int, Player> player in PhotonNetwork.CurrentRoom.Players)
        {
            PlayerItem newPlayerItem = Instantiate(playerItemPrefab, content);
            newPlayerItem.SetPlayerInfo(player.Value);           
            playerItemsList.Add(newPlayerItem);
        }
    }

    public void OnTeamValueChanged()
    {
        UpdatePlayerList();
    }

    public override void OnPlayerEnteredRoom(Player newPlayer)
    {
        UpdatePlayerList();
    }

    public override void OnPlayerLeftRoom(Player otherPlayer)
    {
        UpdatePlayerList();
    }

    private void Update()
    {
        if (PhotonNetwork.MasterClient != null)
        {
            if (PhotonNetwork.CurrentRoom.PlayerCount == 2 && PhotonNetwork.IsMasterClient)
            {
                playButton.SetActive(true);
            }

            else
            {
                playButton.SetActive(false);
            }
        }
    }

    public void OnClickStartGame()
    {
        PhotonNetwork.LoadLevel("MainScene");
    }

    public void MainMenu()
    {
        SceneManager.LoadScene("MainMenu");
    }
}
