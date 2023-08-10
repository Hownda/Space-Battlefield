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

    private float boostDuration = 5;
    private float boostTime;
    private bool boostActive;
    public Image thrustIcon;
    public Image thrustSliderFill;

    public Image[] abilityLocks;
    private Dictionary<Ability, bool> abilityDict = new Dictionary<Ability, bool>() { { Ability.Boost, true }, { Ability.Missile, false }, { Ability.Shield, false } };
    public Text[] keybindTexts;

    // Boost
    private bool warpActive = false;
    public VisualEffect warpSpeedVFX;
    public float desiredVolume = 0.5f;
    public float desiredPitch = 1;
    public float finalPitch = 2;
    public float finalVolume = 1;
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
        warpSpeedVFX.SetFloat("WarpAmount", 0);
        OnRebind();
    }

    private void Update()
    {
        if (IsOwner)
        {            
            if (boostTime + boostDuration <= Time.time && boostActive)
            {               
                GetComponent<SpaceshipMovement>().thrust = 20;
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
        warpActive = true;
        warpSpeedVFX.SetFloat("WarpAmount", 0);
        StartCoroutine(ActivateParticles());
    }

    private IEnumerator ActivateParticles()
    {
        if (warpActive)
        {
            warpSpeedVFX.Play();
            float amount = warpSpeedVFX.GetFloat("WarpAmount");
            while (amount < 1 && warpActive == true)
            {
                amount += rate;
                warpSpeedVFX.SetFloat("WarpAmount", amount);

                if (desiredVolume < finalVolume || desiredPitch < finalPitch)
                    desiredPitch += rate;
                desiredVolume += rate / 3;
                yield return new WaitForSeconds(0.1f);
            }
        }
        else
        {
            float amount = warpSpeedVFX.GetFloat("WarpAmount");
            while (amount > 0 && warpActive == false)
            {
                amount -= rate;
                warpSpeedVFX.SetFloat("WarpAmount", amount);
                yield return new WaitForSeconds(0.1f);

                if (amount <= 0 + rate)
                {
                    amount = 0;
                    warpSpeedVFX.SetFloat("WarpAmount", amount);
                    warpSpeedVFX.Stop();
                }
            }            
        }
    }

    private void ActivateMissileMode()
    {

    }

    private void ActivateShield()
    {

    }

}
