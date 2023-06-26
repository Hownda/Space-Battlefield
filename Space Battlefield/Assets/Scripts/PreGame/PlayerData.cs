using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerData : MonoBehaviour
{
    public static PlayerData instance;
    private string username;

    private void Awake()
    {
        instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public string GetUsername()
    {
        return username;
    }

    public void SetUsername(string newUsername)
    {
        username = newUsername;
    }    
}
