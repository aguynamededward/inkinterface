using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InputSOReceiver : MonoBehaviour
{
    protected static Camera _cameraMain;
    public static Camera cameraMain
    {
        get
        {
            if (_cameraMain == null) _cameraMain = Camera.main;
            return _cameraMain;
        }
    }

    public Vector3 FormatMousePositionToWorldPosition(Vector2 inputVector)
    {
        return cameraMain.ScreenToWorldPoint(new Vector3(inputVector.x, inputVector.y, defaultCameraDistance));
    }

    public float defaultCameraDistance = 17f;

    [SerializeField] InputSO input;

    protected void RegisterInput(InputSO _input)
    {
        _input.OnInputStart += OnInputStart;
        _input.OnInputUpdate += OnInputUpdate;
        _input.OnInputEnd += OnInputEnd;
    }

    protected void UnregisterInput(InputSO _input)
    {
        _input.OnInputStart += OnInputStart;
        _input.OnInputUpdate += OnInputUpdate;
        _input.OnInputEnd += OnInputEnd;
    }

    public virtual void OnInputStart(object o, InputSOData _input)
    {
        Debug.Log("Unimplemented OnInputStart on " + gameObject.name + "." + this + " (InputSOReceiver)");
    }

    public virtual void OnInputEnd(object o, InputSOData _input)
    {
        Debug.Log("Unimplemented OnInputEnd on " + gameObject.name + "." + this + " (InputSOReceiver)");
    }

    public virtual void OnInputUpdate(object o, InputSOData _input)
    {
        Debug.Log("Unimplemented OnInputContinue on " + gameObject.name + "." + this + " (InputSOReceiver)");
    }

    public virtual void OnEnable()
    {
        if (input != null) RegisterInput(input);
        else Debug.Log("Didn't assign an InputSO object for " + gameObject.name + "." + this + " (enabled)");
    }

    public virtual void OnDisable()
    {
        if(input != null) UnregisterInput(input);
        else Debug.Log("Didn't assign an InputSO object for " + gameObject.name + "." + this + " (disabled)");
    }

}
