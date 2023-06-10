using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkPlayerInput_ChooseOption : InputSOReceiver
{
    protected bool activated = false;
    protected List<InkTextObject> choiceObjects;
    protected InkDelegate.CallbackInt choiceCallback;
    protected InkTextObject selectedChoice;

    public void Init(List<InkTextObject> _choiceObjects,InkDelegate.CallbackInt _choiceCallback)
    {
        activated = true;
        choiceObjects = _choiceObjects;
        choiceCallback = _choiceCallback;

        input.OnInputMouseOver += OnMouseOver;
        ShowAllChoices();
    }

    public virtual void ShowAllChoices()
    {
        foreach(InkTextObject ito in choiceObjects)
        {
            ito.ShowText();
        }
    }

    private void IdentifyClickedChoice(InputSOData _input)
    {
        int totalChoices = choiceObjects.Count;
        selectedChoice = null;

        Vector3 mouseWorldPosition = FormatMousePositionToWorldPosition(_input.position);

        for (var q = 0; q < totalChoices; q++)
        {

            if (choiceObjects[q].CheckForClick(mouseWorldPosition))
            {
                selectedChoice = choiceObjects[q];
                break;
            };
        }
    }
    private bool ConfirmClickedChoice(InputSOData _input)
    {
        if (selectedChoice == null) return false;

        //Vector3 mouseWorldPosition = FormatMousePositionToWorldPosition(_input.position);

        bool didWeConfirm = selectedChoice.CheckForClickOnLine(_input.position,cameraMain);
        Debug.Log("We did" + (!didWeConfirm ? " NOT" : "") + " confirm " + selectedChoice + " as clicked on.", selectedChoice);
        return didWeConfirm;
    }
    private void OnMouseOver(object sender, InputSOData _input)
    {
        if (!activated) return;
    }

    public override void OnInputEnd(object sendingSO, InputSOData _input)
    {
        if (!activated) return;
        if (!_input.clickSafe) return;
        input.ActivateClickProtection();

        if (ConfirmClickedChoice(_input))
        {
            CompleteChoicePoint();
        }

    }

    public void CompleteChoicePoint()
    {
        if (selectedChoice)
        {
            choiceCallback?.Invoke(selectedChoice.GetChoiceIndex());
        }
        else choiceCallback?.Invoke(-1); // Tell 'em we didn't get a choice

        activated = false;
    }
    public override void OnInputStart(object sendingSO, InputSOData _input)
    {
        if (!activated) return;
        IdentifyClickedChoice(_input);
    }

    public override void OnInputUpdate(object sendingSO, InputSOData _input)
    {
     
    }
}
