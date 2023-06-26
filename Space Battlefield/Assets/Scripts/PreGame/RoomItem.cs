using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public Text roomName;
    LobbyManager lobbyManager;
    public string roomId;

    private void Start()
    {
        lobbyManager = FindObjectOfType<LobbyManager>();
    }

    public void SetRoomName(string _roomName)
    {
        roomName.text = _roomName;
    }

    public void SetRoomId(string newRoomId)
    {
        roomId = newRoomId;
    }

    public void OnClickItem()
    {
        lobbyManager.JoinLobby(roomId);
    }

}
