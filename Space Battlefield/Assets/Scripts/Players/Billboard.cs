using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class Billboard : NetworkBehaviour
{
    public Camera ownCamera;
    private Camera otherCamera;
    public Text billBoardText;
    private PlayerNetwork otherPlayer;

    private void Start()
    {
        if (IsOwner)
        {
            billBoardText.gameObject.SetActive(false);
            
            otherPlayer = PlayerDictionary.instance.GetOtherPlayer(OwnerClientId).GetComponent<PlayerNetwork>();
        }
    }

    void Update()
    {
        if (otherPlayer != null)
        {
            if (otherPlayer.playerObject != null)
            {
                otherPlayer.playerObject.GetComponentInChildren<Billboard>().otherCamera = ownCamera;
                otherPlayer.playerObject.GetComponentInChildren<Billboard>().billBoardText.text = otherPlayer.username.Value.ToString();
            }
        }
        if (otherCamera != null)
        {
            transform.LookAt(otherCamera.transform);
        }
    }    
}