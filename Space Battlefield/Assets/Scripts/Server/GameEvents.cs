using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class GameEvents : NetworkBehaviour
{
    public NetworkVariable<bool> started = new(false);

    public Text countdownDisplay;
    public Text eventText;
    public int countdownTime = 3;

    // Events
    public GameObject alienDogPrefab;
    public Vector3[] spawnPoints;
    public Vector3[] spawnRotations;

    [ClientRpc] public void StartCountdownClientRpc()
    {
        StartCoroutine(Countdown());
    }

    private IEnumerator Countdown()
    {
        while (countdownTime > 0)
        {
            countdownDisplay.text = countdownTime.ToString();
            yield return new WaitForSeconds(1);
            countdownTime--;
        }
        countdownDisplay.text = "Go!";
        yield return new WaitForSeconds(.5f);

        countdownDisplay.text = "";
        if (IsOwner)
        {
            started.Value = true;
        }

        countdownTime = 600;
        while (countdownTime > 0)
        {
            if (IsOwner)
            {
                if (countdownTime != 600 && countdownTime != 0)
                {
                    if (countdownTime % 120 == 0)
                    {
                        SummonEvent();
                    }
                }
            }

            countdownDisplay.text = countdownTime.ToString();
            yield return new WaitForSeconds(1);
            countdownTime--;
        }
    }

    private void SummonEvent()
    {
        int index = Random.Range(0, spawnPoints.Length - 1);
        GameObject alienDog = Instantiate(alienDogPrefab, spawnPoints[index], Quaternion.Euler(spawnRotations[index]));
        alienDog.GetComponent<NetworkObject>().Spawn();

        DisplayEventMessageClientRpc();
    }

    [ClientRpc] private void DisplayEventMessageClientRpc()
    {
        StartCoroutine(EventMessage());
    }

    private IEnumerator EventMessage()
    {
        eventText.DOFade(1, 1);
        eventText.text = "An Alien Walker Boss has just spawned!";
        yield return new WaitForSeconds(1);
        eventText.DOFade(0, 1);
    }
}
