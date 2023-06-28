using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public struct InputSOData
{
    public InputSOData(Vector3 _pos, float _travelDistance, float _absoluteDistance, bool _clickSafe,bool _clicked = true)
    {
        position = _pos;
        travelDistance = _travelDistance;
        absoluteDistance = _absoluteDistance;
        clickSafe = _clickSafe;
        clicked = _clicked;
    }

    public Vector3 position;
    public float travelDistance;
    public float absoluteDistance;
    public bool clickSafe;
    public bool clicked;
}

[CreateAssetMenu(fileName = "New InputSO Source Object",menuName = "InputSO")]
public class InputSO : ScriptableObject
{
    public EventHandler<InputSOData> OnInputStart;
    public EventHandler<InputSOData> OnInputEnd;
    public EventHandler<InputSOData> OnInputUpdate;
    /// <summary>
    /// Sent when the input device moves without being clicked.
    /// </summary>
    public EventHandler<InputSOData> OnInputMouseOver;

    [SerializeField] public bool enableDebugMessages = false;
    [SerializeField] float clickProtectionDelay = 0.5f;

    public float clickProtectionTimeStamp = 0f;

    public static Vector3 input2DPosition = new Vector3(0f,0f,0f);
    public static Vector2 input2DStartPosition = new Vector2(0f, 0f);
    public static Vector2 input2DEndPosition = new Vector2(0f, 0f);
    
    public static float inputTravelDistance = 0f;
    public static float inputAbsoluteTravelDistance = 0f;
    
    public static float pressTime = -1f;
    public static float releaseTime = -1f;
    
    public static bool isPressed
    {
        get { return pressTime != -1f && releaseTime == -1f; }
    }

    public static float pressDuration { get { return (pressTime == -1f ? -1f : Time.time - pressTime); } }
    public static float releaseDuration { get { return (releaseTime == -1f ? -1f : Time.time - releaseTime); } }

    private void OnEnable()
    {
        if(enableDebugMessages) Debug.Log("InputSO: OnEnable - Click Timestamp" + clickProtectionTimeStamp);
        clickProtectionTimeStamp = 0f;
    }

    public static void Update2DInput(Vector2 inputVector)
    {
        inputTravelDistance += Vector2.Distance(input2DPosition, inputVector);
        input2DPosition = inputVector;
        inputAbsoluteTravelDistance = Vector2.Distance(inputVector, input2DStartPosition);
        input2DEndPosition = inputVector;
    }

    public static void Reset2DInput(Vector2 _input)
    {
        inputTravelDistance = 0f;
        inputAbsoluteTravelDistance = 0f;
        input2DPosition = _input;
        input2DStartPosition = _input;
    }

    public void StartInput(Vector2 inputVector)
    {
        Reset2DInput(inputVector);

        pressTime = Time.time;
        releaseTime = -1f;

        OnInputStart?.Invoke(this,new InputSOData(input2DPosition,0f,0f,CheckClickProtection(pressTime),true));
    }

    public void EndInput(Vector2 inputVector)
    {
        Update2DInput(inputVector);
        
        releaseTime = Time.time;
        pressTime = -1f;

        //Vector3 input3DPosition = FormatMousePositionToWorldPosition(inputVector);


        OnInputEnd?.Invoke(this, new InputSOData(input2DPosition,inputTravelDistance,inputAbsoluteTravelDistance, CheckClickProtection(releaseTime),false));
    }

    public void UpdateInputPosition(Vector2 inputVector)
    {
        Update2DInput(inputVector);

        OnInputUpdate?.Invoke(this, new InputSOData(input2DPosition, inputTravelDistance,inputAbsoluteTravelDistance, CheckClickProtection(Time.time),true));
    }

    public void UpdateMouseOver(Vector2 inputVector)
    {
        OnInputMouseOver?.Invoke(this, new InputSOData(inputVector, 0f, 0f, false, false));
    }

    public void ActivateClickProtection()
    {
        if (enableDebugMessages) Debug.Log("InputSO: ActivateClickProtection");
        clickProtectionTimeStamp = Time.time;
    }

    public bool CheckClickProtection(float currentTime)
    {
        return currentTime - clickProtectionTimeStamp >= clickProtectionDelay;
    }

}
