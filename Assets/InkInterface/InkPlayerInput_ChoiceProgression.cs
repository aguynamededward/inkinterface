using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkPlayerInput_ChoiceProgression : InputSOReceiver
{
    protected bool activated = false;
    protected List<InkTextObject> choiceObjects;
    protected InkDelegate.CallbackInt choiceCallback;
    protected InkTextObject selectedChoice;

    

    public void SetupChoiceMoment(List<InkTextObject> _choiceObjects,InkDelegate.CallbackInt _choiceCallback)
    {
        activated = true;
        choiceObjects = _choiceObjects;
        choiceCallback = _choiceCallback;

        ShowAllChoices();
    }

    protected override void _RegisterInput(InputSO _input)
    {
        if (!_input) return;

        _input.OnInputMouseOver += OnMouseOver;
        base._RegisterInput(_input);
    }

    protected override void _UnregisterInput(InputSO _input)
    {
        if (!_input) return;

        _input.OnInputMouseOver -= OnMouseOver;
        base._UnregisterInput(_input);
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

        for (var q = 0; q < totalChoices; q++)
        {

            if (choiceObjects[q].CheckForClickOnLine(_input.position,cameraMain))
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
        if (!activated || !selectedChoice) return;
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
