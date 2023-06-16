using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestInputReceiver : InputSOReceiver
{
    [SerializeField] float maxTravelDistance;
    [SerializeField] int numberOfLoops = 10000;
    private MeshRenderer meshRenderer;

    // Start is called before the first frame update
    void Start()
    {
        meshRenderer = GetComponent<MeshRenderer>();
    }

    Vector3 startPos;
    Vector3 endPos;
    Vector2 lastPosition;

    public override void OnInputEnd(object sender, InputSOData e)
    {
        meshRenderer.material.color = Color.grey;
        transform.position = FormatMousePositionToWorldPosition(e.position);
        
        endPos = CanvasInterface.WorldSpaceAtDepth(e.position, WorldDepth.Text);
        Debug.Log("End Pos: " + endPos);

        lastPosition = e.position;
    }

    [ContextMenu("Test Speed of CanvasInterface")]
    public void TestCanvasInterfaceSpeed()
    {
        System.Diagnostics.Stopwatch w = new System.Diagnostics.Stopwatch();

        Vector3 temp;
        w.Start();
        for(var q =0; q< numberOfLoops; q++)
        {
            temp = FormatMousePositionToWorldPosition(lastPosition);
        }

        w.Stop();
        Debug.Log("Camera Screen Ray: " + numberOfLoops + " times: " + w.ElapsedMilliseconds + " (" + (w.ElapsedMilliseconds / (float)numberOfLoops) + " each)");
        w.Reset();
        w.Start();
        for (var q = 0; q < numberOfLoops; q++)
        {
            temp = CanvasInterface.WorldSpaceAtDepth(lastPosition,WorldDepth.Background);
        }

        w.Stop();
        Debug.Log("CanvasInterface: " + numberOfLoops + " times: " + w.ElapsedMilliseconds + " (" + (w.ElapsedMilliseconds / (float)numberOfLoops) + " each)");

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
        startPos = CanvasInterface.WorldSpaceAtDepth(e.position, WorldDepth.Text);
        Debug.Log("Start Pos: " + startPos);
    }




    private void OnDrawGizmos()
    {
        Gizmos.DrawSphere(endPos, 0.2f);
        Gizmos.DrawSphere(startPos, 0.2f);
        Gizmos.DrawLine(endPos, startPos);
        
    }
}
