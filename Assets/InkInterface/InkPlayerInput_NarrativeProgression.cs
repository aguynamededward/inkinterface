using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InkPlayerInput_NarrativeProgression : InputSOReceiver
{
    public bool activated = false;

    List<InkTextObject> inkTextObjects;
    int currentDialogueIndex = -1;
    

    InkDelegate.Callback callbackOnNarrativeComplete;
    InkDelegate.CallbackInt callbackOnNarrativeProgression;

    public virtual void Init(List<InkTextObject> _dialogueObjects, int startingIndex, InkDelegate.CallbackInt _onChangeCallback,InkDelegate.Callback _onCompleteCallback)
    {
        inkTextObjects = _dialogueObjects;
        callbackOnNarrativeComplete = _onCompleteCallback;
        callbackOnNarrativeProgression = _onChangeCallback;

        // If we're already on the provided starting index, it means we're ready to go to the next line right away. Otherwise, start one before the starting index, so we roll into it.
        if(currentDialogueIndex != startingIndex) currentDialogueIndex = startingIndex - 1;

        activated = true;

        ShowNextLineOfDialogue();
    }

    protected virtual void ShowNextLineOfDialogue()
    {
        if (input.enableDebugMessages) Debug.Log("DialogueProgression: Show NextLineOfDialogue");
        if(currentDialogueIndex >= inkTextObjects.Count - 1)
        {
            // We're at the end, return the callback
            CompleteDialogueDisplay();
            return;
        }

        currentDialogueIndex++;
        callbackOnNarrativeProgression?.Invoke(currentDialogueIndex);
        //inkTextObjects[currentDialogueIndex].ShowText();
    }

    
    private void CompleteDialogueDisplay()
    {
        if (input.enableDebugMessages) Debug.Log("DialogueProgression: CompleteDialogueDisplay - finished dialogue, running callback");
        activated = false;
        callbackOnNarrativeComplete?.Invoke();
    }
    public override void OnInputEnd(object sendingSO, InputSOData _input)
    {
        if (input.enableDebugMessages) Debug.Log("DialogueProgression: OnInputEnd - Received click. activated: " + activated + " | clickSafe: " + _input.clickSafe + " | clickActivationTime: " + input.clickProtectionTimeStamp);
        if (!activated) return;
        if (!_input.clickSafe) return;

        input.ActivateClickProtection();
        ShowNextLineOfDialogue();
    }

    public override void OnInputStart(object sendingSO, InputSOData _input)
    {
      
    }

    public override void OnInputUpdate(object sendingSO, InputSOData _input)
    {
      
    }

}
