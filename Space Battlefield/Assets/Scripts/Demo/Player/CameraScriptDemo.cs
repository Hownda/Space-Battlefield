using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraScriptDemo : MonoBehaviour
{
    public float mouseSensitivity;

    public Transform playerBody;
    public Camera spaceshipCamera;
    private Camera playerCamera;
    public GameObject weapon;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera = GetComponent<Camera>();
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
        if (Options.instance.disableCameraMovement == false)
        {
            float mouseX = Input.GetAxis("Mouse X") * mouseSensitivity * Time.deltaTime;
            float mouseY = Input.GetAxis("Mouse Y") * mouseSensitivity * Time.deltaTime;

            xRotation -= mouseY;
            xRotation = Mathf.Clamp(xRotation, -90f, 90f);

            transform.localRotation = Quaternion.Euler(xRotation, 0f, 0f);
            playerBody.Rotate(Vector3.up * mouseX);
        }
    }
}
