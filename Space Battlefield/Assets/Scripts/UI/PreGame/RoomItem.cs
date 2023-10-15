using UnityEngine;
using UnityEngine.UI;

public class RoomItem : MonoBehaviour
{
    public Text roomName;
    public Text playerCountText;
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

    public void SetPlayerCount(int count, int maxCount)
    {
        playerCountText.text = count.ToString() + "/" + maxCount.ToString();
    }

    public void OnClickItem()
    {
        lobbyManager.JoinLobby(roomId);
    }

}
