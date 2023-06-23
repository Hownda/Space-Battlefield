using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectDictionary : MonoBehaviour
{
    public static ObjectDictionary instance;

    public Dictionary<GameObject, int> objectDictionary = new Dictionary<GameObject, int>();

    private void Awake()
    {
        instance = this;
    }

    void Start()
    {
        GameObject[] destructibleObjects = GameObject.FindGameObjectsWithTag("DestructibleObjects");
        for (int i = 0; i < destructibleObjects.Length; i++)
        {
            objectDictionary.Add(destructibleObjects[i], i);
        }
    }   
    
    public GameObject GetObjectById(int id)
    {
        foreach (KeyValuePair<GameObject, int> objectInstance in objectDictionary)
        {
            if (objectInstance.Value == id)
            {
                return objectInstance.Key;
            }
        }
        return null;
        
    }

    public int GetIdOfObject(GameObject go)
    {
        return objectDictionary[go];
    }
}
