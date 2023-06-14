using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Unity.Netcode;

public class SpaceshipCamera : NetworkBehaviour
{
    private Vector2 previousCursorPosition;
    private Quaternion cannonPreviousRotation;
    public Vector2 screenCenter;
    public GameObject crosshair;
    public GameObject overlay;

    private float cannonHorizontalRotation;
    private float cannonVerticalRotation;

    public int shootingAreaRadius = 200;
    private int sensitivity = 2000;
    public int cannonRotationStrength = 100;

    private void OnEnable()
    {
        overlay.SetActive(true);
    }

    private void OnDisable()
    {
        overlay.SetActive(false);
    }

    // Start is called before the first frame update
    void Start()
    {
        screenCenter = new Vector2(Screen.width / 2, Screen.height / 1.75f); 
        if (IsOwner)
        {
            crosshair.SetActive(true);
            crosshair.transform.position = new Vector2(Screen.width / 2, Screen.height / 1.75f);
            previousCursorPosition = new Vector2(Screen.width / 2, Screen.height / 1.75f);
        }
    }

    // Update is called once per frame
    void Update()
    {
        if (IsOwner)
        {
            float mouseX = Input.GetAxis("Mouse X");
            float mouseY = Input.GetAxis("Mouse Y");

            crosshair.transform.position += new Vector3(mouseX * sensitivity * Time.deltaTime, mouseY * sensitivity * Time.deltaTime, 0);

            if (Vector2.Distance(screenCenter, crosshair.transform.position) > shootingAreaRadius)
            {
                crosshair.transform.position = previousCursorPosition;
            }
            previousCursorPosition = crosshair.transform.position;
        }
    }
}
