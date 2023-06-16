using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public enum WorldDepth
{
    Screen = 0,
    Background = 20,
    Text = 19
}
public class CanvasInterface : MonoBehaviour
{
    public static CanvasInterface Instance;

    private Canvas canvas;


    private Dictionary<WorldDepth, float[]> screenSizeDictionary = new Dictionary<WorldDepth, float[]>();

    bool initialized = false;

    private Camera mainCamera;
    private RectTransform rectTransform;
    private void Awake()
    {
        if(Instance == null)
        {
            Instance = this;
            UpdateScreenReferences();
        }
        else
        {
            Debug.Log("Two CanvasInterfaces! Destroying this one.");
        }
    }

    private void Start()
    {
        Initialize();
        
        UpdateScreenReferences();
    }

    public static Vector3 WorldSpaceAtDepth(Vector2 screenSpace,WorldDepth depth)
    {
        return Instance.WorldSpaceDepthLocal(screenSpace, depth);
    }

    private Vector3 WorldSpaceDepthLocal(Vector2 screenSpace, WorldDepth depth)
    {
        if (!screenSizeDictionary.ContainsKey(depth)) screenSizeDictionary[depth] = GetScreenPlaneAtDistance((int)depth);


        float[] _screenArray = screenSizeDictionary[WorldDepth.Screen];
        float[] _tempArray = screenSizeDictionary[depth];

        Vector3 worldSpace = new Vector3(0f, 0f, _tempArray[4]);
        // x
        float _temp = (screenSpace.x / _screenArray[2]);
        _temp *= _tempArray[2] - _tempArray[0];
        _temp += _tempArray[0];

        worldSpace.x = _temp;
        // y
        _temp = (screenSpace.y / _screenArray[3]);
        _temp *= _tempArray[3] - _tempArray[1];
        _temp += _tempArray[1];

        worldSpace.y = _temp;

        return worldSpace;
    }

    private void Initialize()
    {
        canvas = GetComponent<Canvas>();
        mainCamera = Camera.main;
        rectTransform = canvas.GetComponent<RectTransform>();
        initialized = true;
    }

    private void OnRectTransformDimensionsChange()
    {
        UpdateScreenReferences();
    }

    private float screenWidth;
    private float screenHeight;
    private float verticalFOV;
    private float horizontalFOV;

    private void UpdateScreenReferences()
    {
        if (!initialized) return;

        screenWidth = rectTransform.sizeDelta.x;
        screenHeight = rectTransform.sizeDelta.y;

        verticalFOV = mainCamera.fieldOfView;
        horizontalFOV = Camera.VerticalToHorizontalFieldOfView(verticalFOV, screenWidth / screenHeight);

        screenSizeDictionary[WorldDepth.Screen] = new float[4] { 0f, 0f, screenWidth, screenHeight };

        screenSizeDictionary[WorldDepth.Background] = GetScreenPlaneAtDistance((int)WorldDepth.Background);
        screenSizeDictionary[WorldDepth.Text] = GetScreenPlaneAtDistance((int)WorldDepth.Text);
    }

    private float[] GetScreenPlaneAtDistance(int depth)
    {
        //Vector2 origin = new Vector2(0f, 0f);
        //Vector2 screenSize = new Vector2(0f, 0f);

        //float depthForward = (mainCamera.transform.position + (mainCamera.transform.forward * depth)).z;
        //Vector3 centerPoint = mainCamera.ScreenToWorldPoint(new Vector3(screenWidth * 0.5f, screenHeight * 0.5f, depthForward));
        //depthForward = Mathf.Abs(mainCamera.transform.position.z - centerPoint.z);
        //float yChange = depthForward / (Mathf.Cos(verticalFOV * 0.5f)); // the hypotenuse
        //yChange *= yChange;
        //yChange = yChange - (depthForward * depthForward);
        //yChange = (float)Math.Sqrt(yChange);
        //
        ////yChange = (float)Mathf.Sqrt((depth * depth) - (yChange * yChange));




        //origin.y = centerPoint.y - yChange;
        //screenSize.y = centerPoint.y + yChange;
        //
        //float xChange = (float)depth / (Mathf.Cos(horizontalFOV * 0.5f));
        //xChange = Mathf.Sqrt((xChange * xChange) - (depth * depth));
        //
        //origin.x = centerPoint.x - xChange;
        //screenSize.x = centerPoint.x + xChange;

        Vector3 origin = mainCamera.ScreenToWorldPoint(new Vector3(0, 0, depth));
        //origin *= (float)depth / origin.z;
        
        Vector3 screenSize = mainCamera.ScreenToWorldPoint(new Vector3(screenWidth, screenHeight, depth));
        
        //screenSize *= (float)depth / screenSize.z;

        return new float[] { origin[0], origin[1], screenSize[0], screenSize[1],screenSize.z};
    }
}
