using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MouseLookDemo : MonoBehaviour
{
    private float mouseSensitivity = 100f;

    public Transform playerBody;
    public Camera spaceshipCamera;
    private Camera playerCamera;

    private float xRotation = 0f;

    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        playerCamera = GetComponent<Camera>();
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
