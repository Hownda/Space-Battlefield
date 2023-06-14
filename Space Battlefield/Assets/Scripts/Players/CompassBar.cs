using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;

public class CompassBar : NetworkBehaviour
{
    public RawImage compassImage;
    public Image enemyIcon;

    private Transform enemy;

    private void Update()
    {
        if (IsOwner)
        {
            if (enemy == null)
            {
                FindEnemy();
            }
            else
            {
                compassImage.uvRect = new Rect(transform.localEulerAngles.y / 360f, 0f, 1f, 1f);
                float angle = Vector2.SignedAngle(new Vector2(enemy.position.x, enemy.position.z) - new Vector2(transform.position.x, transform.position.z), new Vector2(transform.forward.x, transform.forward.z));
                enemyIcon.rectTransform.anchoredPosition = new Vector2((compassImage.rectTransform.rect.width / 360f) * angle, 0f);
            }
        }
    }

    private void FindEnemy()
    {
        GameObject[] players = GameObject.FindGameObjectsWithTag("Player");
        foreach (GameObject player in players)
        {
            if (player.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
            {
                enemy = player.transform;
            }
        }
        if (enemy == null)
        {
            GameObject[] spaceships = GameObject.FindGameObjectsWithTag("Spaceship");
            foreach (GameObject spaceship in spaceships)
            {
                if (spaceship.GetComponent<NetworkObject>().OwnerClientId != OwnerClientId)
                {
                    enemy = spaceship.transform;
                }
            }
        }
    }
}
