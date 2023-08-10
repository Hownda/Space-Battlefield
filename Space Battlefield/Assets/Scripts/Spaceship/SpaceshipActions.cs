using UnityEngine;
using Unity.Netcode;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using System.Collections.Generic;
using SysEnum = System.Enum;
using UnityEngine.VFX;
using System.Collections;

public enum Ability
{
    Boost, Missile, Shield
}

public class SpaceshipActions : NetworkBehaviour
{
    private MovementControls gameActions;
    public GameObject playerPrefab;
   
    public Image thrustIcon;
    public Image thrustSliderFill;

    public Image[] abilityLocks;
    private Dictionary<Ability, bool> abilityDict = new Dictionary<Ability, bool>() { { Ability.Boost, true }, { Ability.Missile, false }, { Ability.Shield, false } };
    public Text[] keybindTexts;

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
        if (IsOwner)
        {            
            if (boostTime + boostDuration <= Time.time && warpActive)
            {               
                GetComponent<SpaceshipMovement>().thrust = 200;
                GetComponent<SpaceshipMovement>().thrustEffect.SetVector4("Color", normalColor);
                warpActive = false;
            }
        }
    }

    private void OnRebind()
    {
        keybindTexts[0].text = gameActions.Spaceship.Boost.GetBindingDisplayString();
        keybindTexts[1].text = gameActions.Spaceship.Missile.GetBindingDisplayString();
        keybindTexts[2].text = gameActions.Spaceship.Shield.GetBindingDisplayString();
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
            if (obj.action.name == SysEnum.GetName(typeof(Ability), Ability.Boost))
            {
                if (abilityDict[Ability.Boost] != false)
                {
                    Boost();
                }
            }
            if (obj.action.name == SysEnum.GetName(typeof(Ability), Ability.Missile))
            {
                if (abilityDict[Ability.Boost] != false)
                {
                    ActivateMissileMode();
                }
            }
            if (obj.action.name == SysEnum.GetName(typeof(Ability), Ability.Shield))
            {
                if (abilityDict[Ability.Boost] != false)
                {
                    ActivateShield();
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
}
