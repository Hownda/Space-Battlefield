using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Photon.Pun;
using Photon.Realtime;

public class PlayerItem : MonoBehaviourPunCallbacks
{
    Player player;
    public Text playerName;
    public static PlayerItem instance;
    public int currentParentIndex;
    private PhotonView PV;

    public string ipAddress;
    public string portAddress;

    public ExitGames.Client.Photon.Hashtable playerProperties = new ExitGames.Client.Photon.Hashtable();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        PV = GetComponent<PhotonView>();
    }

    public void SetPlayerInfo(Player _player)
    {
        playerName.text = _player.NickName;
        player = _player;
    }

}
