using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IObjectPoolElement
{
    public abstract void PoolShutdown();
}

public static class ObjectPool<T> where T : Component, IObjectPoolElement
{
    static private List<T> objectPool = new List<T>();
    static private List<T> objectPoolActive = new List<T>();

    static Dictionary<T, ObjectPoolOnShutdown> objectPoolShutdownCallbackDictionary = new Dictionary<T, ObjectPoolOnShutdown>();
    public delegate void ObjectPoolOnShutdown();

    static private GameObject _parentObject;
    static private GameObject parentObject 
    { 
        get {
            if (_parentObject == null)
            {
                _parentObject = new GameObject(nameof(T) + " Pool Object");
                _parentObject.isStatic = true;
            }
            return _parentObject;
        } 
    }

    public static void AddToPool(T poolObject, ObjectPoolOnShutdown shutdownCallback)
    {
        if (objectPool.Contains(poolObject) == false)
        {
            objectPool.Add(poolObject);
            poolObject.transform.SetParent(parentObject.transform);
            objectPoolShutdownCallbackDictionary.Add(poolObject, shutdownCallback);
        }
    }

    public static void RemoveFromPool(T poolObject)
    {
        if (objectPoolShutdownCallbackDictionary.ContainsKey(poolObject))
        {
            objectPoolShutdownCallbackDictionary.Remove(poolObject);

            if (objectPool.Contains(poolObject)) objectPool.Remove(poolObject);
            if (objectPoolActive.Contains(poolObject)) objectPoolActive.Remove(poolObject);
        }
        
    }

    public static T GetPoolObject(Transform sceneParent,Transform prefab)
    {
        if (objectPool.Count > 0)
        {
            return PopFromObjectPool(sceneParent);
        }
        
        T inkTextObject = GameObject.Instantiate(prefab,sceneParent).GetComponent<T>();

        return PopFromObjectPool(sceneParent);
    }

    public static void ReturnToObjectPool(T poolObj)
    {
        if (objectPoolActive.Contains(poolObj))
        {
            objectPoolShutdownCallbackDictionary[poolObj]();
            
            poolObj.transform.SetParent(parentObject.transform);
            
            objectPoolActive.Remove(poolObj);
            objectPool.Add(poolObj);
        }
    }

    private static T PopFromObjectPool(Transform sceneParent)
    {
        int popPos = objectPool.Count - 1;
        
        T poolObject = objectPool[popPos];
        objectPool.RemoveAt(popPos);
            
        objectPoolActive.Add(poolObject);

        poolObject.transform.SetParent(sceneParent);

        return poolObject;
    }

    public static void ReturnAllObjectsToPool()
    {
        int poolObjectTotal = objectPoolActive.Count;
        if(poolObjectTotal > 0)
        {
            for (var q = poolObjectTotal - 1; q >= 0; q--) 
            {
                ReturnToObjectPool(objectPoolActive[q]);
            }
        }
    }
    
}


