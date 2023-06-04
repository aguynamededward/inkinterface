using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;


public class SwipePageInputReceiver : InputSOReceiver
{
    private Action callBack;
    [SerializeField] FloatValueSO ScreenIndicatorPercent;

    public float DebugAngle;
    public float distanceToSwipe;
    public float movementMultiplier = 1f;
    public bool activated = false;
    public bool moving = false;
    public bool readyToSwipe = false;

    private Vector3 homePosition;
    private Quaternion homeRotation;
    private Vector3 offsetPosition;
    private Vector3 baseAngle;
    
    public void Setup(Action _cb)
    {
        activated = true;
        moving = false;
        callBack = _cb;
    }
    
    public override void OnInputStart(object o, InputSOData _input)
    {
        if (!activated || moving) return;
        moving = false;
        homePosition = transform.position;
        homeRotation = transform.rotation;

        Vector3 _pos = FormatMousePositionToWorldPosition(_input.position);
        offsetPosition = _pos;
        baseAngle = (_pos - homePosition).normalized;
    }

    public override void OnInputEnd(object o, InputSOData _input)
    {
        if (!activated) return;

        if (readyToSwipe)
        {
            MoveToOffscreenPosition();
            SetActivated(false);
        }
        else MoveToOriginalPosition();
    }

    private void OnDrawGizmos()
    {
        Gizmos.DrawLine(homePosition, transform.position);
        Gizmos.DrawSphere(offsetPosition, 0.07f);
    }
    public void MoveToOffscreenPosition()
    {
        Vector3 direction = (transform.position - homePosition);
        direction *= 10f;

        SetMoving(true);
        transform.DOMove(transform.position + direction, 0.5f).SetEase(Ease.InQuint).OnComplete(RunCallback);
        DOTween.To(() => ScreenIndicatorPercent.value, x => ScreenIndicatorPercent.value = x, 0, 0.5f);
    }
    
    public void MoveToOriginalPosition()
    {
        SetMoving(true);
        transform.DOMove(homePosition, 0.5f).SetEase(Ease.OutQuint).OnComplete(() => { SetMoving(false); });
        transform.DORotate(homeRotation.eulerAngles, 0.5f).SetEase(Ease.OutQuint);

        DOTween.To(() => ScreenIndicatorPercent.value, x => ScreenIndicatorPercent.value = x, 0, 0.5f);

    }


    public void RunCallback()
    {
        callBack?.Invoke();
    }


    [ContextMenu("Reset Position")]
    public void ResetPosition()
    {
        transform.position = homePosition;
        transform.rotation = homeRotation;
        SetActivated(true);
        SetMoving(false);
    }

    public void SetActivated(bool _b)
    {
        activated = _b;
    }

    public void SetMoving(bool _b)
    {
        moving = _b;
    }

    public override void OnInputUpdate(object o, InputSOData _input)
    {
        if (!activated) return;
        
        Vector3 _pos = FormatMousePositionToWorldPosition(_input.position); // Current position in world space
        _pos -= offsetPosition; // Amount we've changed since touch
        
        Vector3 workingAngle = _pos.normalized;

        _pos *= movementMultiplier;

        _pos += homePosition; // Our new position

        
        float _newAngle = Vector3.Angle(_pos,homePosition);
        
        float _distance = homePosition.x - _pos.x;
        _newAngle *= Mathf.Clamp(_distance/1,-1f,1f);       // As we get closer to the original X position
                                                            // it will want to suddenly jump signs
                                                            // doing it this way means we gently ease into and out of 0
                                                            // as we approach it
        DebugAngle = _newAngle;

        transform.rotation = homeRotation * Quaternion.AngleAxis(_newAngle, Vector3.forward);

        transform.position = _pos;

        readyToSwipe = _input.absoluteDistance >= distanceToSwipe;

        if(ScreenIndicatorPercent != null)
        {
            ScreenIndicatorPercent.value = Mathf.Clamp(_input.absoluteDistance / distanceToSwipe, 0f, 1f);
        }
    }
}
