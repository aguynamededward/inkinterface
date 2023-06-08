using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TestClickBounds : InputSOReceiver
{
    [SerializeField] Transform inkTextObjectPrefab;

    InkTextObject inkTextObject;

    public void Start()
    {
        inkTextObject = Instantiate(inkTextObjectPrefab, transform).GetComponent<InkTextObject>();
        inkTextObject.Init(new InkParagraph("Test of the clicking system.",1));
        inkTextObject.ShowText();
    }
    public override void OnInputEnd(object sendingSO, InputSOData _input)
    {
        Vector3 mouseWorldPosition = FormatMousePositionToWorldPosition(_input.position);
        if (inkTextObject.CheckForClick(mouseWorldPosition))
            Debug.Log("Clicked the box!");
    }

    public override void OnInputStart(object sendingSO, InputSOData _input)
    {
       
    }

    public override void OnInputUpdate(object sendingSO, InputSOData _input)
    {
       
    }


}
