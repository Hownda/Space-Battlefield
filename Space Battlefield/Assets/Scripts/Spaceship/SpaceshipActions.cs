using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using SysEnum = System.Enum;
using DG.Tweening;
using System.Collections;
using Unity.Collections.LowLevel.Unsafe;

public enum Type { Boost, Missile, Shield}

public class Ability
{
    public Type type;
    public bool unlocked;
    public int rockCost;
    public int flowerCost;
    public int index;
    public float timeStamp;
    public float cooldown;
    public float duration;

    public Ability(Type type, bool unlocked, int rockCost, int flowerCost, int index, float timeStamp, float cooldown, float duration)
    {
        this.type = type;
        this.unlocked = unlocked;
        this.rockCost = rockCost;
        this.flowerCost = flowerCost;
        this.index = index;
        this.timeStamp = timeStamp;
        this.cooldown = cooldown;
        this.duration = duration;
    }
}

public class SpaceshipActions : NetworkBehaviour
{
    public MovementControls gameActions;
    public GameObject playerPrefab;
   
    public Image thrustIcon;
    public Image thrustSliderFill;

    public Image[] abilityLocks;
    public InputActionReference[] inputActions;
    public Text[] keybindTexts;
    public Text[] cooldownTexts;
    public Text errorText;

    // Boost
    [ColorUsage(true, true)]
    public Color boostColor;
    [ColorUsage(true, true)]
    public Color normalColor;
    public float boostSpeed = 300;
    public float rate = 0.02f;
    public bool boostActive;

    // Shield
    public GameObject shield;
    public NetworkVariable<bool> shieldActive = new(false, writePerm: NetworkVariableWritePermission.Owner);


    private void OnEnable()
    {
        if (IsOwner)
        {
            KeybindManager.rebindComplete += OnRebind;
            gameActions = KeybindManager.newInputActions;
            KeybindManager.LoadAllBindings(gameActions);
            KeybindManager.newInputActions = gameActions;
            gameActions.Spaceship.Exit.started += GetComponent<SpaceshipActions>().ExitInput;
            gameActions.Spaceship.Boost.started += GetComponent<SpaceshipActions>().UseAbility;
            gameActions.Spaceship.Missile.started += GetComponent<SpaceshipActions>().UseAbility;
            gameActions.Spaceship.Shield.started += GetComponent<SpaceshipActions>().UseAbility;
            gameActions.Spaceship.Enable();
            OnRebind();
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsOwner)
        {
            Camera.main.GetComponent<AudioListener>().enabled = false;
            KeybindManager.rebindComplete -= OnRebind;
        }
    }

    private void OnDisable()
    {
        if (IsOwner)
        {
            KeybindManager.rebindComplete -= OnRebind;
        }
    }

    private void Start()
    {
        if (IsOwner)
        {
            Game.instance.abilityDict[Type.Boost].timeStamp = Time.time;
            Game.instance.abilityDict[Type.Shield].timeStamp = Time.time;

            foreach (KeyValuePair<Type, Ability> ability in Game.instance.abilityDict)
            {
                if (ability.Value.unlocked == true)
                {
                    abilityLocks[ability.Value.index].gameObject.SetActive(false);
                }
            }
        }
    }

    private void Update()
    {
        if (IsOwner)
        {
            foreach (KeyValuePair<Type, Ability> ability in Game.instance.abilityDict)
            {
                if (ability.Value.unlocked == true)
                {
                    Text cooldown = cooldownTexts[ability.Value.index];
                    if (ability.Value.timeStamp + ability.Value.cooldown > Time.time)
                    {
                        cooldown.gameObject.SetActive(true);
                        cooldown.text = (Mathf.Round(100*(ability.Value.timeStamp + ability.Value.cooldown - Time.time)) / 100).ToString();
                    }
                    else
                    {
                        cooldown.gameObject.SetActive(false);
                    }
                }
            }
            if (boostActive)
            {
                if (Game.instance.abilityDict[Type.Boost].timeStamp + Game.instance.abilityDict[Type.Boost].duration <= Time.time)
                {
                    GetComponent<SpaceshipMovement>().thrust = 200;
                    GetComponent<SpaceshipMovement>().thrustEffect.SetVector4("Color", normalColor);
                    boostActive = false;
                }
            }
            if (shieldActive.Value == true)
            {
                if (Game.instance.abilityDict[Type.Shield].timeStamp + Game.instance.abilityDict[Type.Shield].duration <= Time.time)
                {
                    shield.SetActive(false);
                    shieldActive.Value = false;
                }
            }
        }
        else
        {
            if (shieldActive.Value == true)
            {
                shield.SetActive(true);
            }
            else
            {
                shield.SetActive(false);
            }
        }
    }    

    private void OnRebind()
    {
        if (IsOwner)
        {
            foreach (KeyValuePair<Type, Ability> ability in Game.instance.abilityDict)
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
    }

    public void ExitInput(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            if (GetComponent<SpaceshipGravity>().gravityOrbit != null)
            {
                Exit();
            }
        }
    }

    public void Exit()
    {
        Debug.Log("Exiting...");
        gameActions.Spaceship.Exit.started -= GetComponent<SpaceshipActions>().ExitInput;
        gameActions.Spaceship.Boost.started -= GetComponent<SpaceshipActions>().UseAbility;
        gameActions.Spaceship.Missile.started -= GetComponent<SpaceshipActions>().UseAbility;
        gameActions.Spaceship.Shield.started -= GetComponent<SpaceshipActions>().UseAbility;
        gameActions.Spaceship.Disable();
        Camera.main.GetComponent<AudioListener>().enabled = false;
        Camera.main.enabled = false;       
        GetComponent<SpaceshipMovement>().enabled = false;
        GetComponent<CompassObject>().enabled = false;
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
    }

    public void UseAbility(InputAction.CallbackContext obj)
    {
        if (IsOwner)
        {
            if (obj.action.name == SysEnum.GetName(typeof(Type), Type.Boost))
            {
                if (Game.instance.abilityDict[Type.Boost].unlocked != false)
                {
                    var ability = Game.instance.abilityDict[Type.Boost];
                    if (ability.timeStamp + ability.cooldown < Time.time)
                    {
                        Boost();
                    }
                }
                else
                {
                    TryUnlockAbility(Type.Boost);
                }
            }
            if (obj.action.name == SysEnum.GetName(typeof(Type), Type.Missile))
            {
                if (Game.instance.abilityDict[Type.Missile].unlocked != false)
                {
                    var ability = Game.instance.abilityDict[Type.Missile];
                    if (ability.timeStamp + ability.cooldown < Time.time)
                    {
                        ActivateMissileMode();
                    }
                }
                else
                {
                    TryUnlockAbility(Type.Missile);
                }
            }
            if (obj.action.name == SysEnum.GetName(typeof(Type), Type.Shield))
            {
                if (Game.instance.abilityDict[Type.Shield].unlocked != false)
                {
                    var ability = Game.instance.abilityDict[Type.Shield];
                    if (ability.timeStamp + ability.cooldown < Time.time)
                    {
                        ActivateShield();
                    }
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
        Game.instance.abilityDict[Type.Boost].timeStamp = Time.time;
        boostActive = true;
    }

    private void ActivateMissileMode()
    {
        GetComponent<Cannons>().ammo = Ammo.Missile;
        GetComponent<Cannons>().trackingRectangle.SetActive(true);
        GetComponent<Cannons>().crosshair.GetComponent<Image>().enabled = false;
        Debug.Log("Missile mode active");
    }

    private void ActivateShield()
    {
        Debug.Log("Shield active");
        shield.SetActive(true);
        Game.instance.abilityDict[Type.Shield].timeStamp = Time.time;
        shieldActive.Value = true;
        ActivateShieldServerRpc();
    }

    private void TryUnlockAbility(Type type)
    {
        CheckInventoryServerRpc(OwnerClientId, Game.instance.abilityDict[type].rockCost, Game.instance.abilityDict[type].flowerCost, type);
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
            Game.instance.abilityDict[type].unlocked = true;
            abilityLocks[Game.instance.abilityDict[type].index].GetComponent<Animator>().SetTrigger("Unlock");
            OnRebind();
        }
    }

    [ServerRpc] private void ActivateShieldServerRpc()
    {
        ActivateShieldClientRpc();
    }

    [ClientRpc] private void ActivateShieldClientRpc()
    {
        if (!IsOwner)
        {
            shield.SetActive(true);
        }
    }
}
