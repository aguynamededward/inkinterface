using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class InputSOTransmitter : MonoBehaviour, IPointerDownHandler, IPointerUpHandler, IPointerMoveHandler
{
    [SerializeField] InputSO inputScrob;

    private bool pointerDown = false;

    public virtual void OnPointerDown(PointerEventData eventData)
    {
        pointerDown = true;
        inputScrob?.StartInput(eventData.position);
    }

    public virtual void OnPointerUp(PointerEventData eventData)
    {
        pointerDown = false;
        inputScrob?.EndInput(eventData.position);
    }

    public virtual void OnPointerMove(PointerEventData eventData)
    {
        if (pointerDown == false) inputScrob?.UpdateMouseOver(eventData.position);
        else inputScrob?.UpdateInputPosition(eventData.position);
    }
}
