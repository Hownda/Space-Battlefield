using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Linq;
using DG.Tweening;

/// <summary>
/// The class <c>Game</c> manages game events on the server.
/// </summary>
public class Game : NetworkBehaviour
{
    public static Game instance;
    private List<PlayerInformation> playerInformationList = new();
    public Dictionary<ulong, PlayerInformation> playerInformationDict = new();
    private Dictionary<ulong, GameObject> playerCards = new();

    public GameObject spaceshipPrefab;
    public GameObject playerPrefab;

    public float enteringDistance = 20f;
    [SerializeField] private float spaceshipSpawnOffset;

    public GameObject playerCardPrefab;
    public Transform playerCardHolder;
    public GameObject scoreAdditionPrefab;

    [Header("Spawning")]
    public Vector3[] spawnLocations;

    // Spaceship Abilites
    public Dictionary<Type, Ability> abilityDict = new Dictionary<Type, Ability>()
    {
        { Type.Boost, new Ability(Type.Boost, false, 10, 30, 0, 0, 10, 3) },
        { Type.Missile, new Ability(Type.Missile, false, 30, 10, 1, 0, 15, 1000) },
        { Type.Shield, new Ability(Type.Shield, false, 20, 20, 2, 0, 25, 10) }
    };

    public Image damageIndicator;

    public GameObject explosionEffect;
    public GameObject respawnPanel;
    public Text respawnDisplay;

    private void Awake()
    {
        instance = this;
    }

    public void StartGame()
    {
        Debug.Log("Starting Game...");
        int i = 0;
        List<Vector3> randomSpawnPositions = spawnLocations.ToList();
        foreach (GameObject playerRoot in GameObject.FindGameObjectsWithTag("Root"))
        {
            int spawnIndex = Random.Range(0, spawnLocations.Length - 1);
            GameObject spawnedPlayer = Instantiate(playerPrefab, randomSpawnPositions[spawnIndex], Quaternion.identity);
            randomSpawnPositions.RemoveAt(spawnIndex);
            spawnedPlayer.GetComponent<NetworkObject>().Spawn();
            spawnedPlayer.GetComponent<NetworkObject>().ChangeOwnership(playerRoot.GetComponent<NetworkObject>().OwnerClientId);
            playerInformationList.Add(new PlayerInformation(playerRoot.GetComponent<NetworkObject>().OwnerClientId, playerRoot, spawnedPlayer, null));
            playerInformationDict.Add(playerRoot.GetComponent<NetworkObject>().OwnerClientId, playerInformationList[i]);
            i++;
        }
        // Wait for players to reach their spawn points before spawning spaceships
        StartCoroutine(spawnDelay());
    }

    private IEnumerator spawnDelay()
    {
        yield return new WaitForSeconds(2f);

        SpawnPlayerCardsClientRpc();
        SpawnSpaceships();
        GetComponent<GameEvents>().StartCountdownClientRpc();
    }

    [ClientRpc] private void SpawnPlayerCardsClientRpc()
    {
        foreach (GameObject playerRoot in GameObject.FindGameObjectsWithTag("Root"))
        {
            GameObject playerCard = Instantiate(playerCardPrefab, playerCardHolder);
            playerCard.GetComponent<PlayerCard>().clientId = playerRoot.GetComponent<NetworkObject>().OwnerClientId;
            Text[] texts = playerCard.GetComponentsInChildren<Text>();
            texts[0].text = playerRoot.GetComponent<PlayerNetwork>().username.Value.ToString();
            texts[1].text = playerRoot.GetComponent<PlayerNetwork>().score.Value.ToString();
            playerCards.Add(playerRoot.GetComponent<NetworkObject>().OwnerClientId, playerCard);
        }
    }

    private void SpawnSpaceships()
    {
        for (int i = 0; i < playerInformationList.Count; i++)
        {
            GameObject spaceship = Instantiate(spaceshipPrefab, new Vector3(playerInformationList[i].player.transform.position.x + spaceshipSpawnOffset, playerInformationList[i].player.transform.position.y, playerInformationList[i].player.transform.position.z + spaceshipSpawnOffset), Quaternion.Euler(Vector3.zero));
            spaceship.GetComponent<NetworkObject>().Spawn();
            spaceship.GetComponent<NetworkObject>().ChangeOwnership(playerInformationList[i].clientId);
            playerInformationList[i].spaceship = spaceship;
        }
    }

    public void AddPlayerToDict(GameObject player)
    {
        playerInformationDict[player.GetComponent<NetworkObject>().OwnerClientId].player = player;
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToPlayerServerRpc(ulong clientId, int damage, ulong attackingClient)
    {
        GameObject player = playerInformationDict[clientId].player;
        player.GetComponent<Healthbar>().TakeDamage(damage);
        DisplayDamageClientRpc(clientId);

        ulong attackingClientId = 0;

        if (attackingClient != 999)
        {
            attackingClientId = NetworkManager.Singleton.SpawnManager.SpawnedObjects[attackingClient].OwnerClientId;
        }
        else
        {
            attackingClientId = attackingClient;
        }

        if (player.GetComponent<Healthbar>().health.Value <= 0)
        {
            GameObject explosion = Instantiate(explosionEffect, playerInformationDict[clientId].spaceship.transform.position, Quaternion.Euler(Vector3.zero));
            explosion.GetComponentInChildren<ParticleSystem>().Play();
            Destroy(explosion, 2f);

            playerInformationDict[clientId].spaceship.GetComponent<NetworkObject>().Despawn();
            player.GetComponent<NetworkObject>().Despawn();
            PreparePlayerRespawnClientRpc(clientId);

            UpdateScoreClientRpc(clientId, -25);

            if (attackingClientId != 999)
            {
                UpdateScoreClientRpc(attackingClientId, 50);
            }
        }
    }

    [ClientRpc] private void DisplayDamageClientRpc(ulong clientId)
    {
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            StartCoroutine(DamageDisplay());
        }
    }

    private IEnumerator DamageDisplay()
    {
        damageIndicator.gameObject.SetActive(true);
        damageIndicator.DOFade(0.3f, 0.2f);
        yield return new WaitForSeconds(0.2f);
        damageIndicator.DOFade(0, 0.2f);
        damageIndicator.gameObject.SetActive(false);
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToSpaceshipServerRpc(ulong clientId, float damage, ulong attackingClient)
    {
        GameObject spaceship = playerInformationDict[clientId].spaceship;
        spaceship.GetComponent<Hull>().TakeDamage(damage);

        if (spaceship.GetComponent<Hull>().integrity.Value <= 0)
        {
            GameObject explosion = Instantiate(explosionEffect, spaceship.transform.position, Quaternion.Euler(Vector3.zero));
            explosion.GetComponentInChildren<ParticleSystem>().Play();
            Destroy(explosion, 2f);

            spaceship.GetComponent<NetworkObject>().Despawn();

            ulong attackingClientId = NetworkManager.Singleton.SpawnManager.SpawnedObjects[attackingClient].OwnerClientId;

            if (playerInformationDict[clientId].player == null)
            {                
                PreparePlayerRespawnClientRpc(clientId);
                UpdateScoreClientRpc(clientId, -25);

                
                if (attackingClientId != 999)
                { 
                    UpdateScoreClientRpc(attackingClientId, 50);
                }
            }
            else
            {
                PrepareSpaceshipRespawnClientRpc(clientId);
                UpdateScoreClientRpc(clientId, -25);
                if (attackingClientId != 999)

                {
                    UpdateScoreClientRpc(attackingClientId, 25);
                }
            }
        }
    }

    [ServerRpc(RequireOwnership = false)] public void HealDamageOnPlayerServerRpc(ulong clientId, int amount)
    {
        GameObject player = playerInformationDict[clientId].player;
        player.GetComponent<Healthbar>().Heal(amount);
    }

    [ServerRpc(RequireOwnership = false)] public void RepairDamageOnSpaceshipServerRpc(ulong clientId, int amount)
    {
        GameObject spaceship = playerInformationDict[clientId].spaceship;
        spaceship.GetComponent<Hull>().Repair(amount);
    }
    
    [ServerRpc(RequireOwnership = false)] public void RemoveSpaceshipServerRpc(ulong clientId)
    {
        GameObject spaceship = playerInformationDict[clientId].spaceship;
        spaceship.GetComponent<NetworkObject>().Despawn();                
    }

    [ClientRpc] private void UpdateScoreClientRpc(ulong clientId, int score)
    {        
        StartCoroutine(ScoreDespawnDelay(clientId, score));       
    }

    private IEnumerator ScoreDespawnDelay(ulong clientId, int score)
    {
        Debug.Log(score);
        GameObject scoreAdditionUI = Instantiate(scoreAdditionPrefab, playerCards[clientId].GetComponentInChildren<HorizontalLayoutGroup>().transform);
        if (score >= 0) 
        {
            scoreAdditionUI.GetComponent<Text>().text = "+" + score.ToString();
        }
        else
        {
            scoreAdditionUI.GetComponent<Text>().text = score.ToString();
        }
        scoreAdditionUI.GetComponent<Text>().DOFade(0, 1);
        yield return new WaitForSeconds(1);
        if (IsServer)
        {
            playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().score.Value += score;
        }
        Destroy(scoreAdditionUI);
    }

    [ServerRpc(RequireOwnership = false)] public void TriggerVictoryServerRpc(ulong loserClientId)
    {
        TriggerVictoryClientRpc(loserClientId);
    }

    [ClientRpc] private void TriggerVictoryClientRpc(ulong loserClientId)
    {
        // To do: Add losing;
    }

    [ServerRpc(RequireOwnership = false)] public void SetTempHealthServerRpc(ulong clientId, int health)
    {
        playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().tempHealth.Value = health;
    }

    [ServerRpc(RequireOwnership = false)] public void GiveObjectToPlayerServerRpc(ulong clientId, Item item, int amount)
    {
        switch (item)
        {
            case Item.Rock:
                playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().rockCount.Value += amount;
                break;

            case Item.Flower:
                playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().flowerCount.Value += amount;
                break;

            default:
                break;
        }
        playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().AddObjectToInventoryClientRpc(item, amount, clientId);
    }

    [ServerRpc(RequireOwnership = false)] public void RemoveObjectFromInventoryServerRpc(ulong clientId, Item item, int amount)
    {
        switch (item)
        {
            case Item.Rock:
                playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().rockCount.Value -= amount;
                break;

            case Item.Flower:
                playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().flowerCount.Value -= amount;
                break;

            default:
                break;
        }
        playerInformationDict[clientId].root.GetComponent<PlayerNetwork>().RemoveObjectFromInventoryClientRpc(item, amount, clientId);
    }

    [ClientRpc] private void PreparePlayerRespawnClientRpc(ulong clientId)
    {
        if (IsServer)
        {
            StartCoroutine(ServerRespawnBreak(clientId));           
        }
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>().enabled = true;
            StartCoroutine(RespawnBreak());
        }
    }

    private IEnumerator ServerRespawnBreak(ulong clientId)
    {
        int respawnTime = 10;
        while (respawnTime > 0)
        {
            yield return new WaitForSeconds(1);
            respawnTime--;
        }
        yield return new WaitForSeconds(.5f);
        RespawnPlayer(clientId);
    }

    private IEnumerator RespawnBreak()
    {
        respawnPanel.SetActive(true);
        int respawnTime = 10;
        while (respawnTime > 0)
        {
            respawnDisplay.text = "Respawning in " + respawnTime.ToString();
            yield return new WaitForSeconds(1);
            respawnTime--;
        }
        respawnDisplay.text = "Respawning in 0";
        yield return new WaitForSeconds(.5f);
        respawnPanel.SetActive(false);
    }

    private void RespawnPlayer(ulong clientId)
    {
        // Spawn Player
        List<Vector3> randomSpawnPositions = spawnLocations.ToList();
        int spawnIndex = Random.Range(0, spawnLocations.Length - 1);

        GameObject spawnedPlayer = Instantiate(playerPrefab, randomSpawnPositions[spawnIndex], Quaternion.identity);
        spawnedPlayer.GetComponent<NetworkObject>().Spawn();
        spawnedPlayer.GetComponent<NetworkObject>().ChangeOwnership(clientId);

        playerInformationDict[clientId].player = spawnedPlayer;

        // Spawn Spaceship
        GameObject spaceship = Instantiate(spaceshipPrefab, new Vector3(playerInformationDict[clientId].player.transform.position.x + spaceshipSpawnOffset, playerInformationDict[clientId].player.transform.position.y, playerInformationDict[clientId].player.transform.position.z + spaceshipSpawnOffset), Quaternion.Euler(Vector3.zero));
        spaceship.GetComponent<NetworkObject>().Spawn();
        spaceship.GetComponent<NetworkObject>().ChangeOwnership(clientId);
        playerInformationDict[clientId].spaceship = spaceship;
    }

    [ClientRpc] private void PrepareSpaceshipRespawnClientRpc(ulong clientId)
    {
        if (IsServer)
        {
            StartCoroutine(ServerSpaceshipRespawnBreak(clientId));            
        }
        if (NetworkManager.Singleton.LocalClientId == clientId)
        {
            StartCoroutine(SpaceshipRespawnBreak());
        }
    }

    [ServerRpc(RequireOwnership = false)] public void DealDamageToEnemyServerRpc(ulong objectId, int damage, ulong attackingClient)
    {
        GameObject enemy = NetworkManager.Singleton.SpawnManager.SpawnedObjects[objectId].gameObject;
        ulong attackingClientId = NetworkManager.Singleton.SpawnManager.SpawnedObjects[attackingClient].OwnerClientId;

        if (enemy.GetComponent<AlienDog>()) 
        {
            enemy.GetComponent<AlienDog>().health.Value -= damage;
            enemy.GetComponent<AlienDog>().SetPlayer(NetworkManager.Singleton.SpawnManager.SpawnedObjects[attackingClient].gameObject);
            if (enemy.GetComponent<AlienDog>().health.Value <= 0)
            {
                enemy.GetComponent<NetworkObject>().Despawn();
                UpdateScoreClientRpc(attackingClientId, 50);
            }
        }
        
    }

    private IEnumerator ServerSpaceshipRespawnBreak(ulong clientId)
    {
        int respawnTime = 10;
        while (respawnTime > 0 && playerInformationDict[clientId].player != null)
        {
            yield return new WaitForSeconds(1);
            respawnTime--;
        }
        yield return new WaitForSeconds(.5f);
        if (playerInformationDict[clientId].player != null)
        {
            RespawnSpaceship(clientId);
        }
    }

    private IEnumerator SpaceshipRespawnBreak()
    {
        respawnPanel.SetActive(true);
        int respawnTime = 10;
        while (respawnTime > 0)
        {
            respawnDisplay.text = "Spaceship respawning in " + respawnTime.ToString();
            yield return new WaitForSeconds(1);
            respawnTime--;
        }
        respawnDisplay.text = "Spaceship respawning in 0";
        yield return new WaitForSeconds(.5f);
        respawnPanel.SetActive(false);
    }

    private void RespawnSpaceship(ulong clientId)
    {
        GameObject spaceship = Instantiate(spaceshipPrefab, playerInformationDict[clientId].player.transform.up * spaceshipSpawnOffset + playerInformationDict[clientId].player.transform.right * spaceshipSpawnOffset, Quaternion.identity);
        spaceship.GetComponent<NetworkObject>().Spawn();
        spaceship.GetComponent<NetworkObject>().ChangeOwnership(clientId);
        playerInformationDict[clientId].spaceship = spaceship;
    }

    public void SetHealth(GameObject player)
    {
        player.GetComponent<Healthbar>().health.Value = playerInformationDict[player.GetComponent<NetworkObject>().OwnerClientId].root.GetComponent<PlayerNetwork>().tempHealth.Value;
    }
}
