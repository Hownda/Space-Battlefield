using System.Collections;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.SceneManagement;

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

    public GameObject endPanel;
    public GameObject scorePrefab;
    public Transform scoreContainer;

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
        countdownDisplay.text = "Time's up!";
        FinishGame();
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

    private void FinishGame()
    {

        foreach (KeyValuePair<ulong, PlayerInformation> player in Game.instance.playerInformationDict)
        {
            if (player.Value.spaceship != null)
            {
                player.Value.spaceship.GetComponent<NetworkObject>().Despawn();
            }
            if (player.Value.player != null)
            {
                player.Value.player.GetComponent<NetworkObject>().Despawn();
            }
        }
        FinishGameClientRpc();
    }

    [ClientRpc] private void FinishGameClientRpc()
    {
        endPanel.SetActive(true);
        List<ScoreData> scores = new List<ScoreData>();
        PlayerNetwork[] playerNetworks = FindObjectsOfType<PlayerNetwork>();
        foreach (PlayerNetwork item in playerNetworks)
        {
            scores.Add(new ScoreData(item.username.Value.ToString(), item.score.Value));
        }
        scores.OrderByDescending(s => s.score);

        for (int i = 0; i < scores.Count; i++)
        {
            GameObject scoreObject = Instantiate(scorePrefab, scoreContainer);
            Text[] texts = scoreObject.GetComponentsInChildren<Text>();
            texts[0].text = scores[i].username;
            texts[1].text = scores[i].score.ToString();
        }

        StartCoroutine(MainMenu());
    }

    private IEnumerator MainMenu()
    {
        yield return new WaitForSeconds(4);
        SceneManager.LoadScene("MainMenu");
    }
}
