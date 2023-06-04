using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInputReceiver : InputSOReceiver
{
    [SerializeField] float maxTravelDistance;
    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    public override void OnInputEnd(object sender, InputSOData e)
    {
        meshRenderer.material.color = Color.grey;
        transform.position = FormatMousePositionToWorldPosition(e.position);
    }

    public override void OnInputUpdate(object sender, InputSOData e)
    {
        Color c = Color.Lerp(Color.green, Color.red, e.absoluteDistance / maxTravelDistance);
        meshRenderer.material.color = c;
        transform.position = FormatMousePositionToWorldPosition(e.position);
    }

    public override void OnInputStart(object sender, InputSOData e)
    {
        meshRenderer.material.color = Color.green;
        transform.position = FormatMousePositionToWorldPosition(e.position);
    }
    
}
