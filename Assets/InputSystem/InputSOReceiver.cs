using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class InputSOReceiver : MonoBehaviour
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

    public float defaultCameraDistance = 20f;

    [SerializeField] protected InputSO input;

    public void RegisterInput(InputSO _input)
    {
        if(input != _input && input != null)
        {
            UnregisterInput();
        }

        input = _input;

        _RegisterInput(_input);
    }

    protected virtual void _UnregisterInput(InputSO _input)
    {
        if (!_input) return;

        _input.OnInputStart -= OnInputStart;
        _input.OnInputUpdate -= OnInputUpdate;
        _input.OnInputEnd -= OnInputEnd;
    }

    protected virtual void _RegisterInput(InputSO _input)
    {
        if (!_input) return;
        
        _input.OnInputStart += OnInputStart;
        _input.OnInputUpdate += OnInputUpdate;
        _input.OnInputEnd += OnInputEnd;
    }

    public void UnregisterInput()
    {
        if(input != null)
        {
            _UnregisterInput(input);
        }
    }


    public abstract void OnInputStart(object sendingSO, InputSOData _input);

    public abstract void OnInputEnd(object sendingSO, InputSOData _input);

    public abstract void  OnInputUpdate(object sendingSO, InputSOData _input);

    public virtual void OnEnable()
    {
        if (input != null) RegisterInput(input);
        else Debug.Log("Didn't assign an InputSO object for " + gameObject.name + "." + this + " (enabled)");
    }

    public virtual void OnDisable()
    {
        if(input != null) _UnregisterInput(input);
        else Debug.Log("Didn't assign an InputSO object for " + gameObject.name + "." + this + " (disabled)");
    }

}
