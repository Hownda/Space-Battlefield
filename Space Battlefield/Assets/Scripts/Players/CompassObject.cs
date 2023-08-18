using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Linq;

public class CompassObject : NetworkBehaviour
{
    public RectTransform compass;
    public List<GameObject> players = new();
    public Dictionary<ulong, GameObject> arrows = new();
    public GameObject arrowPrefab;
    public float compassRadius = 5;

    private int previousLength = 0;

    private void Update()
    {
        if (IsOwner)
        {
            players = GameObject.FindGameObjectsWithTag("Spaceship").ToList();
            players.Remove(gameObject);
            if (players.Count != previousLength)
            {
                UpdateArrows();
            }
            for (int i = 0; i < players.Count; i++)
            {
                Vector2 direction = new Vector2(players[i].transform.position.x, players[i].transform.position.z) - new Vector2(transform.position.x, transform.position.z);
                float angle = Vector2.SignedAngle(direction, new Vector2(transform.forward.x, transform.forward.z));

                GameObject arrow = arrows[players[i].GetComponent<NetworkObject>().OwnerClientId];
                arrow.transform.rotation = Quaternion.Euler(new Vector3(0, 0, -angle));
                arrow.GetComponent<RectTransform>().anchoredPosition = arrow.GetComponent<RectTransform>().up * compassRadius;

                if (players[i].transform.position.y <= transform.position.y)
                {
                    float globalAngle = Vector2.SignedAngle(arrow.GetComponent<RectTransform>().up, Vector2.down);
                    arrow.GetComponentsInChildren<RectTransform>()[1].localRotation = Quaternion.Euler(0, 0, globalAngle);
                }
                else
                {
                    float globalAngle = Vector2.SignedAngle(arrow.GetComponent<RectTransform>().up, Vector2.up);
                    arrow.GetComponentsInChildren<RectTransform>()[1].localRotation = Quaternion.Euler(0, 0, globalAngle);
                }
            }
            previousLength = players.Count;
        }
    }

    private void UpdateArrows()
    {
        foreach (KeyValuePair<ulong, GameObject> arrow in arrows)
        {
            Destroy(arrow.Value);
        }
        players.Clear();
        arrows.Clear();
        GameObject localPlayer = null;
        players = GameObject.FindGameObjectsWithTag("Spaceship").ToList();
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
            {
                arrows.Add(player.GetComponent<NetworkObject>().OwnerClientId, Instantiate(arrowPrefab, compass));
                Debug.Log("Adding player " + player.GetComponent<NetworkObject>().OwnerClientId);
            }
        }
        players.Remove(gameObject);        
    }
}
