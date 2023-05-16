using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Unity.Netcode;

public class CameraScript : NetworkBehaviour
{
    public float mouseSensitivity;

    public Transform playerBody;
    public Camera spaceshipCamera;
    private Camera playerCamera;
    public GameObject weapon;
    public GameObject crosshairOverlay;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera = GetComponent<Camera>();
        if (!IsOwner)
        {
            playerCamera.enabled = false;
            playerCamera.GetComponent<AudioListener>().enabled = false;
            crosshairOverlay.SetActive(false);
        }
        if (PlayerPrefs.HasKey("Sensitivity"))
        {
            mouseSensitivity = PlayerPrefs.GetFloat("Sensitivity");
        }
        else
        {
            mouseSensitivity = 200;
        }
    }

    void Update()
    {
        if (IsOwner && Options.instance.disableCameraMovement == false)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
        DisableBodyParts();
    }

    private void DisableBodyParts()
    {
        if (!IsOwner && IsClient && GameObject.FindGameObjectsWithTag("Player").Length == 2)
        {
            playerCamera.transform.parent.gameObject.layer = 0;
            Renderer[] renderers = playerCamera.transform.parent.gameObject.GetComponentsInChildren<Renderer>();
            foreach (Renderer renderer in renderers)
            {
                renderer.gameObject.layer = 0;
            }
            weapon.layer = 7;
        }
    }
}
