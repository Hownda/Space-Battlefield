using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class PlayerCard : MonoBehaviour
{
    public ulong clientId;
    public Text scoreDisplay;
    private PlayerNetwork playerNetwork;

    private void Start()
    {
        foreach (GameObject playerRoot in GameObject.FindGameObjectsWithTag("Root"))
        {
            if (playerRoot.GetComponent<NetworkObject>().OwnerClientId == clientId)
            {
                playerNetwork = playerRoot.GetComponent<PlayerNetwork>();
            }
        }
    }

    private void Update()
    {
        scoreDisplay.text = playerNetwork.score.Value.ToString();
    }
}
