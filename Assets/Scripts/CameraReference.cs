using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CameraReference : MonoBehaviour
{
    public static Transform cameraTransform;
    public static Transform cameraCenterStage;

    private void Awake()
    {
        cameraTransform = transform;
        
        foreach(Transform child in transform)
        {
            if (child.gameObject.name == "CenterStage") cameraCenterStage = child;
        }
    }

    
}
