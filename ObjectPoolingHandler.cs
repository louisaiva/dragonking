using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class ObjectPoolingHandler : MonoBehaviour {

    public static ObjectPoolingHandler current;
    public GameObject pooledObject;
    public int pooledAmount = 20;
    public bool willGrow = true;

    // list of pooled objects
    List<GameObject> pooledObjects;

    // unity functions

    void Awake(){
        current = this;
    }

    void Start(){
        pooledObjects = new List<GameObject>();
        for (int i = 0; i < pooledAmount; i++){
            GameObject obj = (GameObject)Instantiate(pooledObject);
            obj.SetActive(false);
            pooledObjects.Add(obj);
        }
    }

    // important functions

    public GameObject GetPooledObject(){
        for (int i = 0; i < pooledObjects.Count; i++){
            if (!pooledObjects[i].activeInHierarchy){
                return pooledObjects[i];
            }
        }
        if (willGrow){
            GameObject obj = (GameObject)Instantiate(pooledObject);
            pooledObjects.Add(obj);
            return obj;
        }
        return null;
    }



}