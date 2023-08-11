using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using SysEnum = System.Enum;
using DG.Tweening;
using System.Collections;

public enum Type { Boost, Missile, Shield}

public class Ability
{
    public Type type;
    public bool unlocked;
    public int rockCost;
    public int flowerCost;
    public int index;

    public Ability(Type type, bool unlocked, int rockCost, int flowerCost, int index)
    {
        this.type = type;
        this.unlocked = unlocked;
        this.rockCost = rockCost;
        this.flowerCost = flowerCost;
        this.index = index;
    }
}

public class SpaceshipActions : NetworkBehaviour
{
    private MovementControls gameActions;
    public GameObject playerPrefab;
   
    public Image thrustIcon;
    public Image thrustSliderFill;

    public Image[] abilityLocks;
    private Dictionary<Type, Ability> abilityDict = new Dictionary<Type, Ability>() 
    { 
        { Type.Boost, new Ability(Type.Boost, false, 10, 30, 0) }, 
        { Type.Missile, new Ability(Type.Missile, false, 30, 10, 1) }, 
        { Type.Shield, new Ability(Type.Shield, false, 20, 20, 2) } 
    };
    public InputActionReference[] inputActions;
    public Text[] keybindTexts;
    public Text errorText;

    // Boost
    [ColorUsage(true, true)]
    public Color boostColor;
    [ColorUsage(true, true)]
    public Color normalColor;
    private bool warpActive = false;
    public float boostSpeed = 300;
    private float boostDuration = 5;
    private float boostTime;
    public float rate = 0.02f;

    private void OnEnable()
    {
        if (IsOwner)
        {
            KeybindManager.rebindComplete += OnRebind;
            gameActions = KeybindManager.inputActions;
            gameActions.Spaceship.Exit.started += ExitInput;
            gameActions.Spaceship.Boost.started += UseAbility;
            gameActions.Spaceship.Missile.started += UseAbility;
            gameActions.Spaceship.Shield.started += UseAbility;
            gameActions.Spaceship.Enable();
        }
    }

    private void OnDisable()
    {
        KeybindManager.rebindComplete -= OnRebind;
    }

    private void Start()
    {
        boostTime = Time.time;
        OnRebind();
    }

    private void Update()
    {
        if (warpActive)
        {
            if (boostTime + boostDuration <= Time.time)
            {
                GetComponent<SpaceshipMovement>().thrust = 200;
                GetComponent<SpaceshipMovement>().thrustEffect.SetVector4("Color", normalColor);
                warpActive = false;
            }
        }
    }

    private void OnRebind()
    {
        foreach (KeyValuePair<Type, Ability> ability in abilityDict)
        {
            if (ability.Value.unlocked)
            {
                keybindTexts[ability.Value.index].text = inputActions[ability.Value.index].action.GetBindingDisplayString();
            }
            else
            {
                keybindTexts[ability.Value.index].text = inputActions[ability.Value.index].action.GetBindingDisplayString() + " to unlock\n" + ability.Value.rockCost + " Rock\n" + ability.Value.flowerCost + " Flower";
            }
        }
    }

    public void ExitInput(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            Exit();
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting...");
        gameActions.Spaceship.Exit.started -= ExitInput;
        gameActions.Spaceship.Boost.started -= UseAbility;
        gameActions.Spaceship.Disable();
        GetComponent<Hull>().integrityBillboard.SetActive(true);
        Camera.main.GetComponent<AudioListener>().enabled = false;
        Camera.main.enabled = false;       
        GetComponentInChildren<SpaceshipMovement>().enabled = false;        
        GetComponent<Cannons>().enabled = false;
        SpawnPlayerServerRpc();
        this.enabled = false;
    }

    
    [ServerRpc] private void SpawnPlayerServerRpc()
    {
        Vector3 spawnPosition;
        if (GetComponent<SpaceshipGravity>().gravityOrbit != null)
        {
            spawnPosition = transform.position + 3 * ((transform.position - GetComponent<SpaceshipGravity>().gravityOrbit.transform.position).normalized);
        }
        else
        {
            spawnPosition = transform.position + 3 * transform.up;
        }

        GameObject player = Instantiate(playerPrefab, new Vector3(spawnPosition.x, spawnPosition.y, spawnPosition.z), Quaternion.Euler(Vector3.zero));
        player.GetComponent<NetworkObject>().Spawn();
        player.GetComponent<NetworkObject>().ChangeOwnership(OwnerClientId);

        // Make UI look at player
        GetComponent<Hull>().cam = player.GetComponentInChildren<Camera>();
        Game.instance.AddPlayerToDict(player);
        Game.instance.SetHealth(player);
        Game.instance.DisableBodyPartsClientRpc();
    }

    private void UseAbility(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            if (obj.action.name == SysEnum.GetName(typeof(Type), Type.Boost))
            {
                if (abilityDict[Type.Boost].unlocked != false)
                {
                    Boost();
                }
                else
                {
                    TryUnlockAbility(Type.Boost);
                }
            }
            if (obj.action.name == SysEnum.GetName(typeof(Type), Type.Missile))
            {
                if (abilityDict[Type.Missile].unlocked != false)
                {
                    ActivateMissileMode();
                }
                else
                {
                    TryUnlockAbility(Type.Missile);
                }
            }
            if (obj.action.name == SysEnum.GetName(typeof(Type), Type.Shield))
            {
                if (abilityDict[Type.Shield].unlocked != false)
                {
                    ActivateShield();
                }
                else
                {
                    TryUnlockAbility(Type.Shield);
                }
            }
        }
    }

    private void Boost()
    {
        Debug.Log("Boost");
        GetComponent<SpaceshipMovement>().thrust = boostSpeed;
        GetComponent<SpaceshipMovement>().thrustEffect.SetVector4("Color", boostColor);
        boostTime = Time.time;
        warpActive = true;
    }

    private void ActivateMissileMode()
    {

    }

    private void ActivateShield()
    {

    }

    private void TryUnlockAbility(Type type)
    {
        CheckInventoryServerRpc(OwnerClientId, abilityDict[type].rockCost, abilityDict[type].flowerCost, type);
    }

    [ServerRpc] private void CheckInventoryServerRpc(ulong clientId, int rockCost, int flowerCost, Type type)
    {
        PlayerNetwork playerNetwork = Game.instance.playerInformationDict[clientId].root.GetComponent<PlayerNetwork>();
        if (playerNetwork.rockCount.Value >= rockCost)
        {
            if (playerNetwork.flowerCount.Value >= flowerCost)
            {
                Game.instance.RemoveObjectFromInventoryServerRpc(clientId, Item.Rock, rockCost);
                Game.instance.RemoveObjectFromInventoryServerRpc(clientId, Item.Flower, flowerCost);
                UnlockAbilityClientRpc(type);
            }
        }
        ReturnErrorClientRpc();
    }

    [ClientRpc] private void ReturnErrorClientRpc()
    {
        if (IsOwner)
        {
            StartCoroutine(DisplayError());           
        }
    }

    private IEnumerator DisplayError()
    {
        errorText.DOFade(1, .5f);
        yield return new WaitForSeconds(2);
        errorText.DOFade(0, 1);
    }

    [ClientRpc] private void UnlockAbilityClientRpc(Type type)
    {
        if (IsOwner)
        {
            abilityDict[type].unlocked = true;
            abilityLocks[abilityDict[type].index].gameObject.SetActive(false);

        }
    }
}
